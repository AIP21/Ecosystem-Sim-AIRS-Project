using UnityEngine;

public class WaterMesh : MonoBehaviour
{
    private GameObject waterObject;
    public Material waterMaterial;

    public Transform sun;

    public Vector2Int meshSize;
    public int meshHeight = 128;

    public ShallowWater fluidSim;

    public void Start()
    {
        createWaterObject();
    }

    private void createWaterObject()
    {
        Mesh mesh = createMesh();

        mesh.bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(meshSize.x, meshHeight * 2, meshSize.y));

        waterObject = new GameObject("Water");
        waterObject.AddComponent<MeshFilter>();
        waterObject.AddComponent<MeshRenderer>();
        waterObject.GetComponent<Renderer>().material = waterMaterial;
        waterObject.GetComponent<MeshFilter>().mesh = mesh;
        waterObject.transform.localPosition = new Vector3(0, 0, 0);
    }

    private Mesh createMesh()
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
        waterMaterial.SetTexture("waterHeight", fluidSim.waterMap);
        waterMaterial.SetTexture("waterVelocity", fluidSim.velocityMap);
        waterMaterial.SetTexture("_MainTex", fluidSim.heightmap);

        waterMaterial.SetFloat("resolution", (float)fluidSim.resolution);
        waterMaterial.SetVector("sunDirection", sun.forward * -1.0f);
    }
}