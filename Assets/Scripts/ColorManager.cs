using UnityEngine;

public class ColorManager : MonoBehaviour
{
    [SerializeField] private Material[] baseHueMaterials;
    [SerializeField] private Material[] saturatedTransparentMaterials;
    [SerializeField] private Material[] reversedHueMaterials;
    [SerializeField] private Material[] gradientMaterials;
    [SerializeField] private Material[] reversedgradientMaterials;

    private Color currentColor;

    private readonly float baseSaturation = 0.55f;
    private readonly float baseValue = 1.0f;

    private readonly float transparentSaturation = 0.81f;
    private readonly float transparentValue = 1.0f;
    private readonly float transparentAlpha = 0.8f;

    private readonly float fogSaturation = 0.65f;
    private readonly float fogValue = 0.90f;

    private readonly float gradientSaturation = 0.8f;
    private readonly float gradientValue = 0.3f;


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
        baseColor.a = 1.0f;

        Color saturatedTransparentColor = Color.HSVToRGB(reversedHue, transparentSaturation, transparentValue);
        saturatedTransparentColor.a = transparentAlpha;

        Color reversedColor = Color.HSVToRGB(reversedHue, baseSaturation, baseValue);
        reversedColor.a = 1.0f;

        Color gradientColor = Color.HSVToRGB(hue, gradientSaturation, gradientValue);
        gradientColor.a = 1.0f;

        Color reversedGradientColor = Color.HSVToRGB(reversedHue, gradientSaturation, gradientValue);
        reversedGradientColor.a = 1.0f;

        ApplyColorToMaterials(baseHueMaterials, baseColor);
        ApplyColorToMaterials(saturatedTransparentMaterials, saturatedTransparentColor);
        ApplyColorToMaterials(reversedHueMaterials, reversedColor);
        ApplyColorToMaterials(gradientMaterials, gradientColor);
        ApplyColorToMaterials(reversedgradientMaterials, reversedGradientColor);

        currentColor = baseColor;

        Color fogColor = Color.HSVToRGB(hue, fogSaturation, fogValue);
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
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
