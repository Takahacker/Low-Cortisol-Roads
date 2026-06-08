using UnityEngine;
using System.Collections.Generic;

public class EnvironmentManager : MonoBehaviour
{
    [Header("Mountain Colors")]
    public Color mountainFarColor = new Color(0.35f, 0.40f, 0.55f);
    public Color mountainMidColor = new Color(0.18f, 0.22f, 0.38f);
    public Color mountainNearColor = new Color(0.08f, 0.10f, 0.22f);

    [Header("Cloud Color")]
    public Color cloudColor = new Color(0.92f, 0.87f, 0.85f);

    [Header("Reference Materials")]
    public Material mountainRefMaterial;
    public Material cloudRefMaterial;

    private Transform ball;
    private List<GameObject> envObjects = new List<GameObject>();
    private float lastSpawnZ = 0f;
    private float spawnInterval = 120f;

    private struct MountainLayer
    {
        public float minDist, maxDist, minH, maxH, baseSize;
        public Color color;
    }
    private MountainLayer[] layers;

    void Start()
    {
        ball = GameObject.FindWithTag("Player")?.transform;

        // Create ref materials if not assigned
        if (mountainRefMaterial == null)
        {
            mountainRefMaterial = CreateMaterial(mountainNearColor);
        }
        if (cloudRefMaterial == null)
        {
            cloudRefMaterial = CreateMaterial(Color.white);
            SetTransparent(cloudRefMaterial);
        }

        layers = new MountainLayer[]
        {
            new MountainLayer { minDist = 250f, maxDist = 400f, minH = 50f, maxH = 100f, baseSize = 80f,
                color = mountainFarColor },
            new MountainLayer { minDist = 120f, maxDist = 250f, minH = 35f, maxH = 70f, baseSize = 55f,
                color = mountainMidColor },
            new MountainLayer { minDist = 50f, maxDist = 120f, minH = 25f, maxH = 50f, baseSize = 40f,
                color = mountainNearColor },
        };

        for (float z = -200f; z < 500f; z += spawnInterval)
            SpawnBatch(z);
    }

    Material CreateMaterial(Color col)
    {
        // Use the same shader as an existing material in the project
        Material baseMat = GetBaseMaterial();
        Material mat = new Material(baseMat);
        mat.color = col;
        return mat;
    }

    Material GetBaseMaterial()
    {
        // Try to find any existing material to copy its shader
        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (var r in renderers)
        {
            if (r.sharedMaterial != null && r.sharedMaterial.shader != null)
                return r.sharedMaterial;
        }
        // Last resort
        return new Material(Shader.Find("Standard"));
    }

    void Update()
    {
        if (!ball) return;

        while (lastSpawnZ < ball.position.z + 500f)
        {
            SpawnBatch(lastSpawnZ);
            lastSpawnZ += spawnInterval;
        }

        for (int i = envObjects.Count - 1; i >= 0; i--)
        {
            if (envObjects[i] == null || ball.position.z - envObjects[i].transform.position.z > 400f)
            {
                if (envObjects[i] != null) Destroy(envObjects[i]);
                envObjects.RemoveAt(i);
            }
        }
    }

    void SpawnBatch(float z)
    {
        foreach (var layer in layers)
        {
            for (int i = 0; i < 2; i++)
            {
                float side = (i == 0) ? -1f : 1f;
                float dist = Random.Range(layer.minDist, layer.maxDist);
                float x = ball.position.x + side * dist;
                float h = Random.Range(layer.minH, layer.maxH);
                float b = Random.Range(layer.baseSize * 0.8f, layer.baseSize * 1.3f);

                Vector3 pos = new Vector3(x, ball.position.y - 10f, z + Random.Range(-50f, 50f));
                envObjects.Add(CreateMountain(pos, h, b, layer.color));
            }
        }

        for (int i = 0; i < 8; i++)
        {
            float x = ball.position.x + Random.Range(-200f, 200f);
            float y = ball.position.y + Random.Range(-8f, 5f);
            Vector3 pos = new Vector3(x, y, z + Random.Range(-60f, 60f));
            envObjects.Add(CreateCloud(pos));
        }
    }

    GameObject CreateMountain(Vector3 pos, float height, float baseSize, Color col)
    {
        GameObject mt = new GameObject("Mt");
        mt.transform.position = pos;

        float b = baseSize;
        float px = Random.Range(-b * 0.08f, b * 0.08f);

        Vector3[] verts = new Vector3[]
        {
            new Vector3(-b, 0, -b * 0.5f),
            new Vector3(-b * 0.4f, 0, -b),
            new Vector3(b * 0.4f, 0, -b),
            new Vector3(b, 0, -b * 0.5f),
            new Vector3(b, 0, b * 0.5f),
            new Vector3(b * 0.4f, 0, b),
            new Vector3(-b * 0.4f, 0, b),
            new Vector3(-b, 0, b * 0.5f),
            new Vector3(px, height, 0),
        };

        int[] tris = { 0,8,1, 1,8,2, 2,8,3, 3,8,4, 4,8,5, 5,8,6, 6,8,7, 7,8,0 };

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        mt.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = mt.AddComponent<MeshRenderer>();

        Material mat = new Material(mountainRefMaterial);
        float v = Random.Range(-0.02f, 0.02f);
        mat.color = new Color(col.r + v, col.g + v, col.b + v);
        mr.material = mat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        return mt;
    }

    GameObject CreateCloud(Vector3 pos)
    {
        GameObject cloud = new GameObject("Cl");
        cloud.transform.position = pos;

        int layers = Random.Range(2, 4);
        for (int i = 0; i < layers; i++)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(p.GetComponent<Collider>());
            p.transform.parent = cloud.transform;

            Material mat = new Material(cloudRefMaterial);
            float brightness = Random.Range(0.9f, 1.05f);
            float alpha = Random.Range(0.3f, 0.5f);
            mat.color = new Color(
                cloudColor.r * brightness,
                cloudColor.g * brightness,
                cloudColor.b * brightness,
                alpha);
            SetTransparent(mat);

            float sx = Random.Range(40f, 90f);
            float sy = Random.Range(0.8f, 2f);
            float sz = Random.Range(30f, 60f);
            p.transform.localScale = new Vector3(sx, sy, sz);

            p.transform.localPosition = new Vector3(
                Random.Range(-15f, 15f),
                i * 0.5f,
                Random.Range(-10f, 10f)
            );

            p.GetComponent<Renderer>().material = mat;
            p.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            p.GetComponent<Renderer>().receiveShadows = false;
        }

        return cloud;
    }

    void SetTransparent(Material mat)
    {
        try
        {
            mat.SetFloat("_Surface", 1);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
        catch { }
    }
}