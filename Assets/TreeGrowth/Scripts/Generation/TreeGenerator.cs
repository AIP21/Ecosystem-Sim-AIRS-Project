using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TreeGrowth.Generation
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class TreeGenerator : MonoBehaviour
    {
        public TreeParameters parameters;

        [HideInInspector]
        public int RayCastCount = 0;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        public Node Root;

        [HideInInspector]
        public GameObject LeafColliders;

        public ColorGenerator[] ColorSchemes;

        public void Awake()
        {
            this.meshFilter = this.GetComponent<MeshFilter>();
            this.meshRenderer = this.GetComponent<MeshRenderer>();
        }

        private static float map(float inLower, float inUpper, float outLower, float outUpper, float value)
        {
            return outLower + (value - inLower) * (outUpper - outLower) / (inUpper - inLower);
        }

        private void calculateEnergy()
        {
            foreach (var node in this.Root.GetTree())
            {
                if (node.Children.Length >= parameters.MaxChildrenPerNode)
                {
                    continue;
                }

                node.CalculateEnergy();
            }
        }

        public void Reset()
        {
            this.transform.DeleteChildren();
            this.Root = new Node(Vector3.zero, this);
            this.RayCastCount = 0;
            this.calculateEnergy();

            this.LeafColliders = new GameObject();
            this.LeafColliders.name = "Leaf Colliders";
            this.LeafColliders.transform.SetParent(this.transform);
            this.LeafColliders.transform.localPosition = Vector3.zero;
        }

        public void IterateGrowth(TreeParameters parameters)
        {
            this.parameters = parameters;

            this.Grow(this.parameters.BatchSize);

            this.Prune(0.2f);

            this.meshFilter.sharedMesh = this.CreateMesh(this.parameters.MeshSubdivisions);

            this.createBranchColliders();
        }

        public void Build(TreeParameters parameters)
        {
            this.parameters = parameters;

            this.Reset();
            for (int i = 0; i < this.parameters.Iterations / this.parameters.BatchSize; i++)
            {
                this.Grow(this.parameters.BatchSize);
                // yield return null;
            }

            this.Prune(0.2f);
            this.meshFilter.sharedMesh = this.CreateMesh(this.parameters.MeshSubdivisions);
            this.createBranchColliders();
            this.LeafColliders.layer = 9;

            if (this.parameters.GenerateLeaves)
            {
                this.generateColor();
            }

            // var iterator = this.BuildCoroutine(parameters);
            // while (iterator.MoveNext()) ;
        }

        public void Grow(int batchSize)
        {
            Node[] nodes = this.Root.GetTree().OrderByDescending(n => n.Energy).ToArray();

            int remainingOperations = batchSize;

            foreach (Node node in nodes) // For every node in the tree
            {
                if (node.Children.Length == 0) // If that node has no children
                {
                    node.Grow(); // 
                    remainingOperations--;
                }
                else if (node.Children.Length < this.parameters.MaxChildrenPerNode && node.Depth > 1)
                {
                    node.Branch();
                    remainingOperations--;
                }

                if (remainingOperations == 0)
                {
                    break;
                }
            }

            this.calculateEnergy();
        }

        public void Prune(float amount)
        {
            var nodes = this.Root.GetTree().Where(n => n.Children.Length == 0).OrderByDescending(n => n.Energy).ToArray();

            for (int i = 0; i < nodes.Length * amount; i++)
            {
                if (nodes[i].Parent == null)
                {
                    continue;
                }
                nodes[i].RemoveLeafCollider();
                nodes[i].Parent.Children = nodes[i].Parent.Children.Where(n => n != nodes[i]).ToArray();
            }
        }

        private float getBranchRadius(Node node)
        {
            return node.Children.Length == 0 ? 0 : map(0, 1, this.parameters.StemSize, this.parameters.BranchSize, Mathf.Pow(map(1, this.Root.SubtreeSize, 1, 0, node.SubtreeSize), this.parameters.SizeFalloff));
        }

        public Mesh CreateMesh(int subdivisions)
        {
            this.Root.CalculateSubtreeSize();

            Node[] nodes = this.Root.GetTree().ToArray();
            Node[] leafNodes = nodes.Where(node => node.Children.Length == 0).ToArray();

            int edgeCount = nodes.Sum(node => node.Children.Length);
            int vertexCount = nodes.Length * subdivisions;

            if (edgeCount == 0)
            {
                return null;
            }

            var treeTriangles = new int[(edgeCount * 6 - leafNodes.Length * 3) * (subdivisions - 1)];
            int[] leafTriangles = null;
            if (this.parameters.GenerateLeaves)
            {
                vertexCount += leafNodes.Length * parameters.QUADS_PER_LEAF * 4;
                leafTriangles = new int[leafNodes.Length * parameters.QUADS_PER_LEAF * 6];
            }
            var vertices = new Vector3[vertexCount];
            var normals = new Vector3[vertexCount];
            var uvs = new Vector2[vertexCount];
            var indices = new Dictionary<Node, int>();

            int vertexIndex = 0;
            foreach (var node in nodes)
            {
                indices[node] = vertexIndex;
                var direction = (node.Children.Any() && node.Parent != null) ? node.Children.Aggregate<Node, Vector3>(Vector3.zero, (v, n) => v + n.Direction).normalized : node.Direction;
                if (node.Parent == null)
                {
                    node.MeshOrientation = Vector3.Cross(Vector3.forward, direction);
                }
                else
                {
                    node.MeshOrientation = (node.Parent.MeshOrientation - direction * Vector3.Dot(direction, node.Parent.MeshOrientation)).normalized;
                }
                for (int i = 0; i < subdivisions; i++)
                {
                    float progress = (float)i / (subdivisions - 1);
                    var normal = Quaternion.AngleAxis(360f * progress, direction) * node.MeshOrientation;
                    normal.Normalize();
                    normals[vertexIndex] = normal;
                    float offset = 0;
                    if (node.Depth < 4)
                    {
                        offset = Mathf.Pow(Mathf.Abs(Mathf.Sin(progress * 2f * Mathf.PI * 5f)), 0.5f) * 0.5f * (3 - node.Depth) / 3f;
                    }
                    vertices[vertexIndex] = node.Position + normal * this.getBranchRadius(node) * (1f + offset);
                    uvs[vertexIndex] = new Vector2(progress * 6f, (node.Depth % 2) * 3f);
                    vertexIndex++;
                }
            }

            int triangleIndex = 0;
            foreach (var node in nodes)
            {
                int nodeIndex = indices[node];

                foreach (var child in node.Children)
                {
                    int childIndex = indices[child];

                    for (int i = 0; i < subdivisions - 1; i++)
                    {
                        treeTriangles[triangleIndex++] = nodeIndex + i;
                        treeTriangles[triangleIndex++] = nodeIndex + i + 1;
                        treeTriangles[triangleIndex++] = childIndex + i;
                    }

                    if (child.Children.Length != 0)
                    {
                        for (int i = 0; i < subdivisions - 1; i++)
                        {
                            treeTriangles[triangleIndex++] = nodeIndex + i + 1;
                            treeTriangles[triangleIndex++] = childIndex + i + 1;
                            treeTriangles[triangleIndex++] = childIndex + i;
                        }
                    }
                }
            }
            triangleIndex = 0;

            if (this.parameters.GenerateLeaves)
            {
                var leafDirections = new Vector3[parameters.QUADS_PER_LEAF];
                var tangents1 = new Vector3[parameters.QUADS_PER_LEAF];
                var tangents2 = new Vector3[parameters.QUADS_PER_LEAF];
                float increment = Mathf.PI * (3f - Mathf.Sqrt(5f));

                for (int i = 0; i < parameters.QUADS_PER_LEAF; i++)
                {
                    float y = ((i * 2f / parameters.QUADS_PER_LEAF) - 1) + (1f / parameters.QUADS_PER_LEAF);
                    float r = Mathf.Sqrt(1 - Mathf.Pow(y, 2f));
                    float phi = (i + 1f) * increment;
                    leafDirections[i] = new Vector3(Mathf.Cos(phi) * r, y, Mathf.Sin(phi) * r);
                }
                for (int i = 0; i < parameters.QUADS_PER_LEAF; i++)
                {
                    tangents1[i] = Vector3.Cross(leafDirections[i], leafDirections[(i + 1) % parameters.QUADS_PER_LEAF]);
                    tangents2[i] = Vector3.Cross(leafDirections[i], tangents1[i]);
                }

                foreach (var node in leafNodes)
                {
                    var orientation = Quaternion.LookRotation(Random.onUnitSphere, Random.onUnitSphere);
                    for (int i = 0; i < parameters.QUADS_PER_LEAF; i++)
                    {
                        var normal = orientation * leafDirections[i];
                        var tangent1 = orientation * tangents1[i];
                        var tangent2 = orientation * tangents2[i];

                        vertices[vertexIndex + 0] = node.Position + tangent1 * this.parameters.LeafQuadRadius;
                        vertices[vertexIndex + 1] = node.Position + tangent2 * this.parameters.LeafQuadRadius;
                        vertices[vertexIndex + 2] = node.Position - tangent1 * this.parameters.LeafQuadRadius;
                        vertices[vertexIndex + 3] = node.Position - tangent2 * this.parameters.LeafQuadRadius;
                        normals[vertexIndex + 0] = normal;
                        normals[vertexIndex + 1] = normal;
                        normals[vertexIndex + 2] = normal;
                        normals[vertexIndex + 3] = normal;
                        uvs[vertexIndex + 0] = new Vector2(0f, 1f);
                        uvs[vertexIndex + 1] = new Vector2(1f, 1f);
                        uvs[vertexIndex + 2] = new Vector2(1f, 0f);
                        uvs[vertexIndex + 3] = new Vector2(0f, 0f);
                        leafTriangles[triangleIndex++] = vertexIndex + 0;
                        leafTriangles[triangleIndex++] = vertexIndex + 1;
                        leafTriangles[triangleIndex++] = vertexIndex + 2;
                        leafTriangles[triangleIndex++] = vertexIndex + 2;
                        leafTriangles[triangleIndex++] = vertexIndex + 3;
                        leafTriangles[triangleIndex++] = vertexIndex + 0;
                        vertexIndex += 4;
                    }
                }
            }

            var mesh = new Mesh();
            mesh.subMeshCount = 2;
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.SetTriangles(treeTriangles, 0);
            if (this.parameters.GenerateLeaves)
            {
                mesh.SetTriangles(leafTriangles, 1);
            }
            return mesh;
        }

        private void createBranchColliders()
        {
            foreach (Node node in this.Root.GetTree())
            {
                if (node.Parent == null || node.Depth > 6)
                    continue;

                Vector3 position1 = node.Parent.Position;
                Vector3 position2 = node.Position;

                GameObject container = new GameObject();
                container.name = "Branch Collider";
                container.transform.SetParent(this.transform);
                container.transform.localPosition = (position1 + position2) * 0.5f;
                container.transform.localRotation = Quaternion.LookRotation(position2 - position1);

                CapsuleCollider collider = container.AddComponent<CapsuleCollider>();
                float radius = this.getBranchRadius(node.Parent);
                collider.radius = radius;
                collider.height = (position2 - position1).magnitude + radius * 2f;
                collider.direction = 2;
            }
        }

        private void generateColor()
        {
            float totalProbability = this.ColorSchemes.Sum(s => s.Probability);
            float roll = Random.Range(0f, totalProbability);
            foreach (var item in this.ColorSchemes)
            {
                if (item.Probability > roll)
                {
                    this.meshRenderer.materials[1].color = item.GetColor();
                    return;
                }
                else
                {
                    roll -= item.Probability;
                }
            }
        }
    }
}