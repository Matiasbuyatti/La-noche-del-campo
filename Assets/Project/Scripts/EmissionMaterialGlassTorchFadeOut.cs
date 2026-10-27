using UnityEngine;

public class EmissionMaterialGlassTorchFadeOut : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color emissionColor = Color.white;
    [Min(0f)]
    [SerializeField] private float emissionMultiplier = 1f;

    private Material materialInstance;
    private static readonly int EmissionColorId =
        Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (targetRenderer == null)
        {
            Debug.LogWarning(
                "EmissionMaterialGlassTorchFadeOut: falta un Renderer.",
                this
            );
            enabled = false;
            return;
        }

        // Crea una instancia del material para no modificar el asset original.
        materialInstance = targetRenderer.material;

        materialInstance.EnableKeyword("_EMISSION");
        SetEmission(0f);
    }

    public void SetEmission(float intensity)
    {
        if (materialInstance == null)
            return;

        Color finalColor =
            emissionColor * Mathf.Max(0f, intensity) * emissionMultiplier;

        materialInstance.SetColor(EmissionColorId, finalColor);
    }

    public void OnEmission()
    {
        SetEmission(1f);
    }

    public void OffEmission()
    {
        SetEmission(0f);
    }

    public void TimeEmission(float intensity)
    {
        SetEmission(intensity);
    }
}