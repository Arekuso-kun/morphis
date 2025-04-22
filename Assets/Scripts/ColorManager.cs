using UnityEngine;

public class ColorManager : MonoBehaviour
{
    [SerializeField] private Material[] baseHueMaterials;
    [SerializeField] private Material[] saturatedTransparentMaterials;
    [SerializeField] private Material[] reversedHueMaterials;
    [SerializeField] private Material[] gradientMaterials;
    [SerializeField] private Material[] reversedGradientMaterials;
    [SerializeField] private Material[] outlineMaterials;

    private Color currentColor;

    private readonly float baseSaturation = 0.55f;
    private readonly float baseValue = 1.0f;
    private readonly float baseAlpha = 1.0f;

    private readonly float transparentSaturation = 0.8f;
    private readonly float transparentValue = 1.0f;
    private readonly float transparentAlpha = 0.8f;

    private readonly float fogSaturation = 0.65f;
    private readonly float fogValue = 0.9f;
    private readonly float fogAlpha = 1.0f;

    private readonly float gradientSaturation = 0.8f;
    private readonly float gradientValue = 0.3f;
    private readonly float gradientAlpha = 1.0f;

    private readonly float outlineSaturation = 0.6f;
    private readonly float outlineValue = 0.9f;
    private readonly float outlineAlpha = 1.0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float hue = Random.value;
        SetHue(hue);
    }

    public void SetHue(float hue)
    {
        float reversedHue = (hue + 0.5f) % 1.0f;

        Color baseColor = Color.HSVToRGB(hue, baseSaturation, baseValue);
        baseColor.a = baseAlpha;

        Color saturatedTransparentColor = Color.HSVToRGB(reversedHue, transparentSaturation, transparentValue);
        saturatedTransparentColor.a = transparentAlpha;

        Color reversedColor = Color.HSVToRGB(reversedHue, baseSaturation, baseValue);
        reversedColor.a = baseAlpha;

        Color gradientColor = Color.HSVToRGB(hue, gradientSaturation, gradientValue);
        gradientColor.a = gradientAlpha;

        Color reversedGradientColor = Color.HSVToRGB(reversedHue, gradientSaturation, gradientValue);
        reversedGradientColor.a = gradientAlpha;

        Color outlineColor = Color.HSVToRGB(reversedHue, outlineSaturation, outlineValue);
        outlineColor.a = outlineAlpha;

        ApplyColorToMaterials(baseHueMaterials, baseColor);
        ApplyColorToMaterials(saturatedTransparentMaterials, saturatedTransparentColor);
        ApplyColorToMaterials(reversedHueMaterials, reversedColor);
        ApplyColorToMaterials(gradientMaterials, gradientColor);
        ApplyColorToMaterials(reversedGradientMaterials, reversedGradientColor);
        ApplyColorToMaterials(outlineMaterials, outlineColor);

        currentColor = baseColor;

        Color fogColor = Color.HSVToRGB(hue, fogSaturation, fogValue);
        fogColor.a = fogAlpha;

        RenderSettings.fogColor = fogColor;
    }

    private void ApplyColorToMaterials(Material[] materials, Color color)
    {
        if (materials == null) return;

        foreach (var mat in materials)
        {
            if (mat == null) continue;

            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

            if (mat.HasProperty("_Bottom")) mat.SetColor("_Bottom", color);

            if (mat.HasProperty("_Tint")) mat.SetColor("_Tint", color);

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);

            if (mat.HasProperty("_EmissionColor"))
            {
                float hue, saturation, value;
                Color.RGBToHSV(color, out hue, out saturation, out value);
                Color emissionColor = Color.HSVToRGB(hue, 0.97f, 0.69f);
                emissionColor.a = 1.0f;

                mat.SetColor("_EmissionColor", emissionColor * 12.0f);
            }

            if (mat.HasProperty("_Outline_Color")) mat.SetColor("_Outline_Color", color);

            if (mat.HasProperty("_Changed_Color")) mat.SetColor("_Changed_Color", color);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
