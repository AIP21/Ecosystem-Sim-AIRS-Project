using UnityEngine;

namespace WaterSim
{
    public class Sim_SurfaceWaterMesh : MonoBehaviour
    {
        private GameObject waterObject;
        public Material waterMaterial;

        public Transform sun;

        public Vector2Int meshSize;
        public int meshHeight = 128;

        public Sim_Water fluidSim;

        public void Start()
        {
            createWaterObject();
        }

        private void createWaterObject()
        {
            waterObject = new GameObject("Water");
            waterObject.AddComponent<MeshFilter>();
            waterObject.AddComponent<MeshRenderer>();
            waterObject.GetComponent<Renderer>().material = waterMaterial;
            waterObject.GetComponent<MeshFilter>().mesh = createMesh();
            waterObject.transform.localPosition = new Vector3(0, 0, 0);
        }

        private Mesh createMesh()
        {
            Mesh mesh = new Mesh();

            // mesh.bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(meshSize.x, meshHeight * 2, meshSize.y));
            mesh.name = "Water Mesh";

            Vector3[] vertices = new Vector3[(meshSize.x + 1) * (meshSize.y + 1)];
            Vector2[] uv = new Vector2[vertices.Length];
            Vector4[] tangents = new Vector4[vertices.Length];
            Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
            for (int i = 0, y = 0; y <= meshSize.y; y++)
            {
                for (int x = 0; x <= meshSize.x; x++, i++)
                {
                    vertices[i] = new Vector3(x, y);
                    uv[i] = new Vector2((float)x / meshSize.x, (float)y / meshSize.y);
                    tangents[i] = tangent;
                }
            }
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.tangents = tangents;

            int[] triangles = new int[meshSize.x * meshSize.y * 6];
            for (int ti = 0, vi = 0, y = 0; y < meshSize.y; y++, vi++)
            {
                for (int x = 0; x < meshSize.x; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + meshSize.x + 1;
                    triangles[ti + 5] = vi + meshSize.x + 2;
                }
            }
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        private Mesh createMesh2()
        {
            Vector3[] vertices = new Vector3[meshSize.x * meshSize.y];
            Vector2[] texcoords = new Vector2[meshSize.x * meshSize.y];
            Vector3[] normals = new Vector3[meshSize.x * meshSize.y];
            int[] indices = new int[meshSize.x * meshSize.y * 6];

            for (int x = 0; x < meshSize.x; x++)
            {
                for (int y = 0; y < meshSize.y; y++)
                {
                    Vector2 uv = new Vector3(x / (meshSize.x - 1.0f), y / (meshSize.y - 1.0f));
                    Vector3 pos = new Vector3(x, 0.0f, y);
                    Vector3 norm = new Vector3(0.0f, 1.0f, 0.0f);

                    texcoords[x + y * meshSize.x] = uv;
                    vertices[x + y * meshSize.x] = pos;
                    normals[x + y * meshSize.x] = norm;
                }
            }

            int num = 0;
            for (int x = 0; x < meshSize.x - 1; x++)
            {
                for (int y = 0; y < meshSize.y - 1; y++)
                {
                    indices[num++] = x + y * meshSize.x;
                    indices[num++] = x + (y + 1) * meshSize.x;
                    indices[num++] = (x + 1) + y * meshSize.x;

                    indices[num++] = x + (y + 1) * meshSize.x;
                    indices[num++] = (x + 1) + (y + 1) * meshSize.x;
                    indices[num++] = (x + 1) + y * meshSize.x;
                }
            }

            Mesh mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.uv = texcoords;
            mesh.triangles = indices;
            mesh.normals = normals;

            return mesh;
        }

        public void Update()
        {
            waterMaterial.SetTexture("waterMap", fluidSim.waterMap);
            waterMaterial.SetTexture("waterVelocity", fluidSim.velocityMap);
            waterMaterial.SetTexture("_MainTex", fluidSim.heightmap);

            waterMaterial.SetFloat("resolution", (float)fluidSim.resolution);
            waterMaterial.SetVector("sunDirection", sun.forward * -1.0f);
        }
    }
}