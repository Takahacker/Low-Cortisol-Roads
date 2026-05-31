using UnityEngine;

[ExecuteInEditMode]
public class SkyboxSetup : MonoBehaviour
{
    [Header("Colors")]
    public Color topColor = new Color(0.45f, 0.55f, 0.78f);
    public Color midColor = new Color(0.88f, 0.68f, 0.58f);
    public Color bottomColor = new Color(0.08f, 0.10f, 0.20f);

    [Header("Section Sizes")]
    [Range(0.05f, 0.9f)] public float midStart = 0.25f;
    [Range(0.1f, 0.95f)] public float midEnd = 0.45f;
    [Range(0.01f, 0.5f)] public float blendSmoothness = 0.12f;

    [Header("Sun")]
    public Color sunColor = new Color(1f, 0.9f, 0.7f);
    [Range(0, 0.2f)] public float sunSize = 0.06f;
    [Range(-1, 1)] public float sunX = 0.3f;
    [Range(-0.5f, 1)] public float sunY = 0.25f;

    private Material skyMat;

    void Start()
    {
        CreateSkybox();
    }

    void Update()
    {
        // Live update in editor and play mode
        if (skyMat != null)
            ApplySettings();
    }

    void CreateSkybox()
    {
        Shader shader = Shader.Find("Custom/GradientSkybox");
        if (shader == null) { Debug.LogWarning("GradientSkybox shader not found!"); return; }

        skyMat = new Material(shader);
        ApplySettings();

        RenderSettings.skybox = skyMat;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = topColor;
        RenderSettings.ambientEquatorColor = midColor;
        RenderSettings.ambientGroundColor = bottomColor;

        Camera.main.clearFlags = CameraClearFlags.Skybox;
    }

    void ApplySettings()
    {
        skyMat.SetColor("_TopColor", topColor);
        skyMat.SetColor("_MidColor", midColor);
        skyMat.SetColor("_BotColor", bottomColor);
        skyMat.SetFloat("_MidStart", midStart);
        skyMat.SetFloat("_MidEnd", Mathf.Max(midEnd, midStart + 0.05f));
        skyMat.SetFloat("_Blend", blendSmoothness);
        skyMat.SetColor("_SunColor", sunColor);
        skyMat.SetFloat("_SunSize", sunSize);
        skyMat.SetFloat("_SunX", sunX);
        skyMat.SetFloat("_SunY", sunY);
    }
}