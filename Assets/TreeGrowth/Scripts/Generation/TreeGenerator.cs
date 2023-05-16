using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TreeGrowth.Generation
{
    // [RequireComponent(typeof(MeshRenderer))]
    // [RequireComponent(typeof(MeshFilter))]
    public class TreeGenerator : MonoBehaviour
    {
        #region Public
        public TreeParameters parameters;
        public int Iteration = 0;
        public ColorGenerator[] ColorSchemes;

        [HideInInspector]
        public Transform BranchColliders;
        [HideInInspector]
        public GameObject LeafColliders;
        [HideInInspector]
        public int RayCastCount = 0;
        #endregion

        #region Private
        private MeshFilter meshFilter;
        // private MeshRenderer meshRenderer;
        private Node Root;
        #endregion

        public void Awake()
        {
            if (this.parameters.GenerateBranchMesh || this.parameters.GenerateLeafMesh)
                this.meshFilter = this.GetComponent<MeshFilter>();

            this.Reset();
        }

        public void Reset()
        {
            this.transform.DeleteChildren();
            this.Root = new Node(Vector3.zero, this);
            this.RayCastCount = 0;
            this.Iteration = 0;
            this.recalculateEnergy();

            if (this.parameters.GenerateBranchMesh || this.parameters.GenerateLeafMesh)
                this.meshFilter.sharedMesh = null;

            if (this.parameters.GenerateBranchColliders)
            {
                GameObject obj = new GameObject();
                obj.name = "Branch Colliders";
                this.BranchColliders = obj.transform;
                this.BranchColliders.SetParent(this.transform);
                this.BranchColliders.localPosition = Vector3.zero;
            }

            if (this.parameters.GenerateLeafColliders)
            {
                this.LeafColliders = new GameObject();
                this.LeafColliders.name = "Leaf Colliders";
                this.LeafColliders.layer = LayerMask.NameToLayer("Leaf");
                Transform trans = this.LeafColliders.transform;
                trans.SetParent(this.transform);
                trans.localPosition = Vector3.zero;
            }
        }

        public void SetParameters(TreeParameters toSet)
        {
            this.parameters = toSet;
        }

        #region Growth
        public void IterateGrowth(TreeParameters growthParameters)
        {
            this.parameters = growthParameters;

            // If this is the first iteration, then reset the tree
            if (this.Iteration == 0)
                this.Reset();

            // If we still have iterations left to grow
            if (this.Iteration < this.parameters.Iterations / this.parameters.BatchSize)
            {
                // this.transform.DeleteChildren();

                List<Node> newNodes = this.Grow(this.parameters.BatchSize);

                if (this.parameters.GenerateBranchMesh)
                    this.meshFilter.sharedMesh = this.CreateMesh(this.parameters.MeshSubdivisions);

                if (this.parameters.GenerateBranchColliders)
                    this.createBranchColliders(newNodes);

                this.Iteration++;
            }
            else if (this.Iteration == this.parameters.Iterations) // This tree is done growing
            {
                // this.Prune(0.2f);

                if (this.parameters.GenerateLeafMesh)
                    this.generateColor();

                if (this.parameters.GenerateBranchMesh)
                    this.meshFilter.sharedMesh = this.CreateMesh(this.parameters.MeshSubdivisions);

                if (this.parameters.GenerateBranchColliders)
                    this.createBranchColliders(this.Root.GetTree());

                this.Iteration++;
            }
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

            if (this.parameters.GenerateBranchMesh)
                this.meshFilter.sharedMesh = this.CreateMesh(this.parameters.MeshSubdivisions);

            if (this.parameters.GenerateBranchColliders)
                this.createBranchColliders(this.Root.GetTree());

            if (this.parameters.GenerateLeafMesh)
                this.generateColor();
        }

        /**
        Grow or branch a given number of nodes (highest energy nodes first)
        **/
        public List<Node> Grow(int batchSize)
        {
            // Get every node in this tree, ordered by energy (highest energy first)
            Node[] nodes = this.Root.GetTree().OrderByDescending(n => n.Energy).ToArray();

            List<Node> newNodes = new List<Node>();

            int remainingOperations = batchSize; // The number of nodes that can grow or branch in this batch (the batch size)

            foreach (Node node in nodes) // For every node in the tree
            {
                Node newNode = null;

                if (node.Children.Length == 0) // If the node has no children. The trunk nodes
                {
                    this.Root.CalculateSubtreeSize();
                    node.CalculateTrunkiness(this.Root.SubtreeSize);

                    newNode = node.Grow(); // Grow a child branch
                    remainingOperations--;
                }
                else if (node.Children.Length < this.parameters.MaxBranchesPerNode && node.Depth > 1) // If the node has less than the max children and is not the root branch (not the trunk, basically)
                {
                    this.Root.CalculateSubtreeSize();
                    node.CalculateTrunkiness(this.Root.SubtreeSize);

                    newNode = node.Branch(); // Branch the node outwards
                    remainingOperations--;
                }

                if (newNode != null) // If a new node was created
                    newNodes.Add(newNode); // Add it to the list of new nodes

                if (remainingOperations == 0) // No more operations can be performed in this batch
                    break;
            }

            // Recalculate the new energy after growing one more step
            this.recalculateEnergy();

            return newNodes;
        }

        public void Prune(float amount)
        {
            // Get every node in this tree, ordered by energy (highest energy first)
            Node[] nodes = this.Root.GetTree().Where(n => n.Children.Length == 0).OrderByDescending(n => n.Energy).ToArray();

            // Iterate through the top given percentage of nodes
            for (int i = (int)(nodes.Length * amount) - 1; i >= 0; i--)
            {
                if (nodes[i].Parent == null) // If this is not the trunk
                    continue;

                nodes[i].RemoveLeafCollider();
                nodes[i].Parent.Children = nodes[i].Parent.Children.Where(n => n != nodes[i]).ToArray(); // Remove this node from its parent's children
            }
        }

        public List<Vector3> GetSeedLocations(int amount)
        {
            // Get a few random leaf nodes
            List<Node> leaves = chooseRandomLeafNodes(amount);

            List<Vector3> positions = new List<Vector3>();

            foreach (Node leaf in leaves)
            {
                Vector3 pos = leaf.GetPosition();

                if (Physics.Raycast(new Ray(pos, Vector3.down), out RaycastHit hit, 100, this.parameters.GroundLayerMask))
                {
                    pos = hit.point;
                }

                positions.Add(pos);
            }

            return positions;
        }
        #endregion

        #region Math
        private float getBranchRadius(Node node)
        {
            return node.Children.Length == 0 ? 0 : map(0, 1, this.parameters.StemSize, this.parameters.BranchSize, Mathf.Pow(map(1, this.Root.SubtreeSize, 1, 0, node.SubtreeSize), this.parameters.SizeFalloff));
        }

        public Mesh CreateMesh(int subdivisions)
        {
            this.Root.CalculateSubtreeSize();

            Node[] nodes = this.Root.GetTree().ToArray(); // Get every node in the tree
            Node[] leafNodes = nodes.Where(node => node.Children.Length == 0).ToArray(); // Get every leaf node in the tree

            int edgeCount = nodes.Sum(node => node.Children.Length); // Get the sum of all the child branches of every branch
            int vertexCount = nodes.Length * subdivisions; // The number of vertices in the tree is the number of branches times the number of subdivisions

            if (edgeCount == 0)
                return null;

            int[] treeTriangles = new int[(edgeCount * 6 - leafNodes.Length * 3) * (subdivisions - 1)];
            int[] leafTriangles = null;

            if (this.parameters.GenerateLeafMesh)
            {
                vertexCount += leafNodes.Length * parameters.QuadsPerLeaf * 4;
                leafTriangles = new int[leafNodes.Length * parameters.QuadsPerLeaf * 6];
            }

            Vector3[] vertices = new Vector3[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            Dictionary<Node, int> indices = new Dictionary<Node, int>();

            int vertexIndex = 0;
            foreach (Node node in nodes)
            {
                indices[node] = vertexIndex;

                Vector3 direction = (node.Children.Any() && node.Parent != null) ? node.Children.Aggregate<Node, Vector3>(Vector3.zero, (v, n) => v + n.Direction).normalized : node.Direction;

                if (node.Parent == null)
                {
                    node.MeshOrientation = Vector3.Cross(Vector3.forward, direction);
                }
                else
                {
                    node.MeshOrientation = (node.Parent.MeshOrientation - direction * Vector3.Dot(direction, node.Parent.MeshOrientation)).normalized;
                }

                for (int i = 0; i < subdivisions; i++) // Create the vertices for this branch
                {
                    float progress = (float)i / (subdivisions - 1);

                    Vector3 normal = Quaternion.AngleAxis(360f * progress, direction) * node.MeshOrientation;

                    normal.Normalize();
                    normals[vertexIndex] = normal;

                    float offset = 0;

                    if (node.Depth < 4)
                        offset = Mathf.Pow(Mathf.Abs(Mathf.Sin(progress * 2f * Mathf.PI * 5f)), 0.5f) * 0.5f * (3 - node.Depth) / 3f;

                    vertices[vertexIndex] = node.Position + normal * this.getBranchRadius(node) * (1f + offset);
                    uvs[vertexIndex] = new Vector2(progress * 6f, (node.Depth % 2) * 3f);
                    vertexIndex++;
                }
            }

            int triangleIndex = 0;
            foreach (Node node in nodes)
            {
                int nodeIndex = indices[node];

                foreach (Node child in node.Children)
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

            if (this.parameters.GenerateLeafMesh)
            {
                Vector3[] leafDirections = new Vector3[parameters.QuadsPerLeaf];
                Vector3[] tangents1 = new Vector3[parameters.QuadsPerLeaf];
                Vector3[] tangents2 = new Vector3[parameters.QuadsPerLeaf];
                float increment = Mathf.PI * (3f - Mathf.Sqrt(5f));

                for (int i = 0; i < parameters.QuadsPerLeaf; i++)
                {
                    float y = ((i * 2f / parameters.QuadsPerLeaf) - 1) + (1f / parameters.QuadsPerLeaf);
                    float r = Mathf.Sqrt(1 - Mathf.Pow(y, 2f));
                    float phi = (i + 1f) * increment;
                    leafDirections[i] = new Vector3(Mathf.Cos(phi) * r, y, Mathf.Sin(phi) * r);
                }
                for (int i = 0; i < parameters.QuadsPerLeaf; i++)
                {
                    tangents1[i] = Vector3.Cross(leafDirections[i], leafDirections[(i + 1) % parameters.QuadsPerLeaf]);
                    tangents2[i] = Vector3.Cross(leafDirections[i], tangents1[i]);
                }

                foreach (Node node in leafNodes)
                {
                    Quaternion orientation = Quaternion.LookRotation(Random.onUnitSphere, Random.onUnitSphere);
                    for (int i = 0; i < parameters.QuadsPerLeaf; i++)
                    {
                        Vector3 normal = orientation * leafDirections[i];
                        Vector3 tangent1 = orientation * tangents1[i];
                        Vector3 tangent2 = orientation * tangents2[i];

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

            Mesh mesh = new Mesh();

            mesh.subMeshCount = 2;
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;

            mesh.SetTriangles(treeTriangles, 0);

            if (this.parameters.GenerateLeafMesh)
                mesh.SetTriangles(leafTriangles, 1);

            return mesh;
        }

        private void createBranchColliders(IEnumerable<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                if (node.Parent == null || node.Depth > this.parameters.BranchColliderDepth)
                    continue;

                Vector3 position1 = node.Parent.Position;
                Vector3 position2 = node.Position;

                GameObject container = new GameObject();
                container.name = "Branch Collider";
                Transform containerTransform = container.transform;
                container.transform.SetParent(this.BranchColliders);
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

            foreach (ColorGenerator item in this.ColorSchemes)
            {
                if (item.Probability > roll)
                {
                    // this.meshRenderer.materials[1].color = item.GetColor();
                    return;
                }
                else
                    roll -= item.Probability;
            }
        }

        /**
        Recalculate the sunlight energy of every node in the tree (for nodes that are able to grow more children)
        **/
        private void recalculateEnergy()
        {
            foreach (Node node in this.Root.GetTree())
            {
                if (node.Children.Length >= parameters.MaxBranchesPerNode)
                    continue;

                node.RecalculateEnergy();
            }
        }

        public int Age()
        {
            return this.Iteration; // map(0, this.parameters.Iterations, 0, 1, this.Iteration);
        }

        private List<Node> chooseRandomLeafNodes(int num)
        {
            List<Node> leafNodes = this.Root.GetTree().Where(n => n.Children.Length == 0).ToList();

            List<Node> chosenNodes = new List<Node>();
            for (int i = 0; i < num; i++)
            {
                int index = Random.Range(0, leafNodes.Count);
                chosenNodes.Add(leafNodes[index]);
                leafNodes.RemoveAt(index);
            }

            return chosenNodes;
        }

        public float CalculateWaterUseThisTick()
        {
            recalculateEnergy();

            float waterUse = 0;
            foreach (Node node in this.Root.GetTree())
                waterUse += nodeWaterUse(node);

            return waterUse;
        }

        // Uses radius, length, and energy.
        // If a leaf node, add extra water use dependent on the size of the leaf and its quad count (density of leaves)
        private float nodeWaterUse(Node node)
        {
            float branchRadius = getBranchRadius(node);

            float sizeMult = 0.06f;
            float leafMult = 0.025f;
            float isLeaf = node.Children.Length == 0 ? 1 : 0;

            float waterUse = (sizeMult * (node.Energy * (Mathf.Pow(branchRadius, 2) * Mathf.Pow(node.GetLength(), 2)))) + (isLeaf * (leafMult * (this.parameters.QuadsPerLeaf * this.parameters.LeafQuadRadius)));

            return waterUse;
        }

        public float CalculateWaterAbsorptionThisTick()
        {
            float waterAbsorption = 0;
            foreach (Node node in this.Root.GetTree())
            {
                // If this is a stem node
                if (Mathf.Pow(map(1, this.Root.SubtreeSize, 1, 0, node.SubtreeSize), this.parameters.SizeFalloff) == 0)
                {
                    float stemAbsorptionMult = 0.3f;
                    waterAbsorption += stemAbsorptionMult * (getBranchRadius(node) + node.GetLength());
                }
            }

            return waterAbsorption;
        }
        #endregion

        private static float map(float inLower, float inUpper, float outLower, float outUpper, float value)
        {
            return outLower + (value - inLower) * (outUpper - outLower) / (inUpper - inLower);
        }

        public Mesh GetMesh()
        {
            if (this.meshFilter == null)
                return null;
            else
                return this.meshFilter.mesh;
        }
    }
}