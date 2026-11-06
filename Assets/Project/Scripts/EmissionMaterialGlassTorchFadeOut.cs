using UnityEngine;

public class EmissionMaterialGlassTorchFadeOut : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color emissionColor = Color.white;
    [Min(0f)]
    [SerializeField] private float emissionMultiplier = 1f;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private Material materialInstance;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (targetRenderer == null)
        {
            enabled = false;
            return;
        }

        // .material crea una instancia, así no se modifica el asset original.
        materialInstance = targetRenderer.material;
        materialInstance.EnableKeyword("_EMISSION");

        SetEmission(0f);
    }

    public void SetEmission(float intensity)
    {
        if (materialInstance == null)
            return;

        materialInstance.SetColor(
            EmissionColorId,
            emissionColor * Mathf.Max(0f, intensity) * emissionMultiplier);
    }

    public void OnEmission() => SetEmission(1f);

    public void OffEmission() => SetEmission(0f);

    public void TimeEmission(float intensity) => SetEmission(intensity);
}
