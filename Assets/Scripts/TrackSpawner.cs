using UnityEngine;
using System.Collections.Generic;

public class TrackSpawner : MonoBehaviour
{
    [Header("Track")]
    public float trackWidth = 4.5f;
    public float stepSize = 3f;
    public int pointsPerChunk = 30;

    [Header("Curves")]
    public float turnSpeed = 0.4f;
    public float descentAngle = 8f;

    [Header("Gates")]
    public float gateSpacing = 80f;

    public Material trackMaterial;
    public Material edgeMaterial;
    public Material gateMaterial;
    public Material archMaterial;

    private Transform ball;
    private GameManager gameManager;

    private float currentYaw, targetYaw, yawTimer;
    private Vector3 nextPos = Vector3.zero;
    private Vector3 lastFwd = Vector3.forward;
    private Vector3 lastRight = Vector3.right;
    private Vector3 lastUp = Vector3.up;
    private Vector3 overlapPos;
    private Vector3 overlapFwd, overlapRight, overlapUp;
    private bool hasOverlap = false;

    private float distSinceGate = 20f;
    private int gateNumber = 1;

    private List<GameObject> chunks = new List<GameObject>();
    private List<GameObject> arches = new List<GameObject>();
    private int chunksAhead = 5;
    private int chunksBehind = 2;
    private int totalChunks = 0;

    void Start()
    {
        ball = GameObject.FindWithTag("Player")?.transform;
        gameManager = FindFirstObjectByType<GameManager>();
        LoadMats();

        for (int i = 0; i < chunksAhead + chunksBehind; i++)
            SpawnChunk(i < 1);
    }

    void LoadMats()
    {
#if UNITY_EDITOR
        if (!trackMaterial) trackMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TrackMaterial.mat");
        if (!edgeMaterial) edgeMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/EdgeMaterial.mat");
        if (!gateMaterial) gateMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/GateMaterial.mat");
        if (!archMaterial) archMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ArchMaterial.mat");
#endif
        // In builds, create materials by copying shader from existing scene renderers
        if (!trackMaterial || !edgeMaterial || !gateMaterial || !archMaterial)
        {
            Material baseMat = FindBaseMaterial();
            if (!trackMaterial) { trackMaterial = new Material(baseMat); trackMaterial.color = new Color(0.06f, 0.08f, 0.18f); }
            if (!edgeMaterial) { edgeMaterial = new Material(baseMat); edgeMaterial.color = new Color(0.3f, 0.8f, 1f); }
            if (!gateMaterial) { gateMaterial = new Material(baseMat); gateMaterial.color = new Color(0.3f, 0.8f, 1f); }
            if (!archMaterial) { archMaterial = new Material(baseMat); archMaterial.color = new Color(0.92f, 0.92f, 0.95f); }
        }
    }

    Material FindBaseMaterial()
    {
        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (var r in renderers)
        {
            if (r.sharedMaterial != null && r.sharedMaterial.shader != null)
                return r.sharedMaterial;
        }
        return new Material(Shader.Find("Standard"));
    }

    void Update()
    {
        if (!ball) return;

        float distToEnd = Vector3.Distance(ball.position, nextPos);
        if (distToEnd < stepSize * pointsPerChunk * chunksAhead)
            SpawnChunk(false);

        while (chunks.Count > chunksAhead + chunksBehind + 2)
        {
            if (chunks[0] != null) Destroy(chunks[0]);
            chunks.RemoveAt(0);
        }

        // Clean old arches
        for (int i = arches.Count - 1; i >= 0; i--)
        {
            if (arches[i] == null || ball.position.z - arches[i].transform.position.z > stepSize * pointsPerChunk * 3)
            {
                if (arches[i] != null) Destroy(arches[i]);
                arches.RemoveAt(i);
            }
        }
    }

    void SpawnChunk(bool straight)
    {
        int n = pointsPerChunk + 1;
        Vector3[] pts = new Vector3[n];
        Vector3[] rs = new Vector3[n];
        Vector3[] us = new Vector3[n];
        Vector3[] fs = new Vector3[n];

        for (int i = 0; i < n; i++)
        {
            if (i == 0 && hasOverlap)
            {
                pts[0] = overlapPos; fs[0] = overlapFwd;
                rs[0] = overlapRight; us[0] = overlapUp;
                continue;
            }

            if (!straight)
            {
                yawTimer -= stepSize;
                if (yawTimer <= 0f)
                {
                    targetYaw = Random.Range(-50f, 50f);
                    yawTimer = Random.Range(20f, 60f);
                }
                currentYaw = Mathf.Lerp(currentYaw, targetYaw, turnSpeed * stepSize / 10f);
            }

            Quaternion rot = Quaternion.Euler(-descentAngle, currentYaw, 0);
            Vector3 fwd = rot * Vector3.forward;
            Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;
            Vector3 up = Vector3.Cross(fwd, right).normalized;

            pts[i] = nextPos; fs[i] = fwd; rs[i] = right; us[i] = up;
            lastFwd = fwd; lastRight = right; lastUp = up;
            nextPos += fwd * stepSize;
        }

        overlapPos = pts[n - 1]; overlapFwd = fs[n - 1];
        overlapRight = rs[n - 1]; overlapUp = us[n - 1];
        hasOverlap = true;

        GameObject chunk = new GameObject("Chunk_" + totalChunks);
        BuildChunkMesh(chunk, pts, rs, us, n);

        for (int i = 1; i < n; i++)
        {
            distSinceGate += Vector3.Distance(pts[i], pts[i - 1]);
            if (distSinceGate >= gateSpacing && !straight && totalChunks > 0)
            {
                SpawnArch(pts[i], fs[i], rs[i], us[i]);
                distSinceGate = 0f;
            }
        }

        chunks.Add(chunk);
        totalChunks++;
    }

    void BuildChunkMesh(GameObject obj, Vector3[] pts, Vector3[] rs, Vector3[] us, int n)
    {
        float halfW = trackWidth / 2f;
        float edge = 0.12f;
        float th = 0.25f;

        Vector3[] v = new Vector3[n * 4];
        Vector2[] uv = new Vector2[n * 4];
        float ud = 0f;

        for (int i = 0; i < n; i++)
        {
            if (i > 0) ud += Vector3.Distance(pts[i], pts[i - 1]);
            int b = i * 4;
            v[b] = pts[i] - rs[i] * (halfW + edge) + us[i] * th;
            v[b + 1] = pts[i] - rs[i] * halfW + us[i] * th;
            v[b + 2] = pts[i] + rs[i] * halfW + us[i] * th;
            v[b + 3] = pts[i] + rs[i] * (halfW + edge) + us[i] * th;
            float uy = ud * 0.05f;
            uv[b] = new Vector2(0, uy); uv[b + 1] = new Vector2(.1f, uy);
            uv[b + 2] = new Vector2(.9f, uy); uv[b + 3] = new Vector2(1, uy);
        }

        int[] tt = new int[(n - 1) * 6];
        int[] et = new int[(n - 1) * 12];
        int ti = 0, ei = 0;

        for (int i = 0; i < n - 1; i++)
        {
            int b = i * 4, nb = (i + 1) * 4;
            et[ei++] = b; et[ei++] = nb; et[ei++] = nb + 1;
            et[ei++] = b; et[ei++] = nb + 1; et[ei++] = b + 1;
            tt[ti++] = b + 1; tt[ti++] = nb + 1; tt[ti++] = nb + 2;
            tt[ti++] = b + 1; tt[ti++] = nb + 2; tt[ti++] = b + 2;
            et[ei++] = b + 2; et[ei++] = nb + 2; et[ei++] = nb + 3;
            et[ei++] = b + 2; et[ei++] = nb + 3; et[ei++] = b + 3;
        }

        Mesh m = new Mesh();
        m.vertices = v; m.uv = uv;
        m.subMeshCount = 2;
        m.SetTriangles(tt, 0); m.SetTriangles(et, 1);
        m.RecalculateNormals(); m.RecalculateBounds();

        obj.AddComponent<MeshFilter>().sharedMesh = m;
        var mr = obj.AddComponent<MeshRenderer>();
        mr.materials = new Material[] { trackMaterial, edgeMaterial };
        obj.AddComponent<MeshCollider>().sharedMesh = m;
    }

    void SpawnArch(Vector3 pos, Vector3 fwd, Vector3 right, Vector3 up)
    {
        float hw = trackWidth / 2f + 2.5f;
        float archHeight = 4.5f;
        float legHeight = 50f;
        float archWidth = 0.7f;
        float archDepth = 0.25f;
        int archSegments = 20;
        int legSegments = 8;

        GameObject arch = new GameObject("Arch_" + gateNumber);
        arch.transform.position = pos + Vector3.up * 0.5f;
        // Flatten forward to horizontal so arch stands straight up
        Vector3 flatFwd = new Vector3(fwd.x, 0, fwd.z).normalized;
        arch.transform.rotation = Quaternion.LookRotation(flatFwd, Vector3.up);

        Mesh archMesh = BuildArchMesh(hw, archHeight, legHeight, archWidth, archDepth, archSegments, legSegments);

        GameObject archObj = new GameObject("ArchMesh");
        archObj.transform.parent = arch.transform;
        archObj.transform.localPosition = Vector3.zero;
        archObj.transform.localRotation = Quaternion.identity;
        archObj.AddComponent<MeshFilter>().sharedMesh = archMesh;
        var mr = archObj.AddComponent<MeshRenderer>();
        mr.material = archMaterial;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        // Render behind track to avoid visual overlap
        if (mr.material != null)
            mr.material.renderQueue = 2000;

        // Gate number floating above arch (wrapped to prevent build errors)
        try
        {
            GameObject numObj = new GameObject("Num");
            numObj.transform.parent = arch.transform;
            numObj.transform.localPosition = new Vector3(0, archHeight + 0.8f, 0);
            var tmp = numObj.AddComponent<TMPro.TextMeshPro>();
            tmp.text = gateNumber.ToString();
            tmp.fontSize = 6;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = new Color(0.3f, 0.8f, 1f);
        }
        catch { }

        // Trigger
        BoxCollider trigger = arch.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(trackWidth + 1f, archHeight + 1f, 2f);
        trigger.center = new Vector3(0, archHeight / 2f, 0);

        Gate gs = arch.AddComponent<Gate>();
        gs.gateNumber = gateNumber;
        gs.gameManager = gameManager;
        arches.Add(arch);
        gateNumber++;
    }

    Mesh BuildArchMesh(float halfWidth, float height, float legH, float beamWidth, float beamDepth, int archSegs, int legSegs)
    {
        // Path: right leg (down to up) → arch curve (right to left) → left leg (up to down)
        int totalPoints = legSegs + archSegs + 1 + legSegs;
        Vector3[] centers = new Vector3[totalPoints];
        Vector3[] norms = new Vector3[totalPoints];
        int idx = 0;

        // Right leg: from bottom (-legH) up to track level (0)
        for (int i = 0; i < legSegs; i++)
        {
            float t = (float)i / legSegs;
            centers[idx] = new Vector3(halfWidth, -legH * (1f - t), 0);
            norms[idx] = Vector3.right; // outward = right
            idx++;
        }

        // Arch curve: semicircle from right (0°) to left (180°)
        for (int i = 0; i <= archSegs; i++)
        {
            float t = (float)i / archSegs;
            float angle = t * Mathf.PI;
            float ax = halfWidth * Mathf.Cos(angle);
            float ay = height * Mathf.Sin(angle);
            centers[idx] = new Vector3(ax, ay, 0);

            float tx = -halfWidth * Mathf.Sin(angle);
            float ty = height * Mathf.Cos(angle);
            Vector3 tangent = new Vector3(tx, ty, 0).normalized;
            norms[idx] = Vector3.Cross(tangent, Vector3.forward).normalized;
            idx++;
        }

        // Left leg: from track level (0) down to bottom (-legH)
        for (int i = 1; i <= legSegs; i++)
        {
            float t = (float)i / legSegs;
            centers[idx] = new Vector3(-halfWidth, -legH * t, 0);
            norms[idx] = Vector3.left; // outward = left
            idx++;
        }

        int n = idx;
        float hw = beamWidth / 2f;
        float hd = beamDepth / 2f;

        Vector3[] verts = new Vector3[n * 4];
        List<int> tris = new List<int>();

        for (int i = 0; i < n; i++)
        {
            Vector3 c = centers[i];
            Vector3 nm = norms[i];
            Vector3 bn = Vector3.forward;

            int b = i * 4;
            verts[b + 0] = c + nm * hw + bn * hd;
            verts[b + 1] = c + nm * hw - bn * hd;
            verts[b + 2] = c - nm * hw - bn * hd;
            verts[b + 3] = c - nm * hw + bn * hd;
        }

        for (int i = 0; i < n - 1; i++)
        {
            int b = i * 4;
            int nb = (i + 1) * 4;

            tris.Add(b); tris.Add(nb); tris.Add(nb + 1);
            tris.Add(b); tris.Add(nb + 1); tris.Add(b + 1);

            tris.Add(b + 1); tris.Add(nb + 1); tris.Add(nb + 2);
            tris.Add(b + 1); tris.Add(nb + 2); tris.Add(b + 2);

            tris.Add(b + 2); tris.Add(nb + 2); tris.Add(nb + 3);
            tris.Add(b + 2); tris.Add(nb + 3); tris.Add(b + 3);

            tris.Add(b + 3); tris.Add(nb + 3); tris.Add(nb);
            tris.Add(b + 3); tris.Add(nb); tris.Add(b);
        }

        // End caps
        tris.Add(0); tris.Add(1); tris.Add(2);
        tris.Add(0); tris.Add(2); tris.Add(3);
        int e = (n - 1) * 4;
        tris.Add(e); tris.Add(e + 3); tris.Add(e + 2);
        tris.Add(e); tris.Add(e + 2); tris.Add(e + 1);

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}