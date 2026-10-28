using UnityEngine;

public class ElectricTorchOnOff : MonoBehaviour
{
    public enum ModoLightChoose
    {
        noBattery,
        withBattery
    }

    [Header("Referencias")]
    [SerializeField] private Light torchLight;
    [SerializeField] private EmissionMaterialGlassTorchFadeOut emissionMaterial;

    [Header("Configuración")]
    public ModoLightChoose modoLightChoose = ModoLightChoose.noBattery;
    [Min(0f)] public float intensityLight = 2.5f;

    [Header("Batería")]
    [Min(0f)] public float batteryCapacity = 100f;
    [Min(0f)] public float batteryRemaining = 100f;
    [Min(0f)] public float batteryDrainPerSecond = 1f;

    private bool flashLightOn;

    public bool IsLightOn => flashLightOn;
    public float BatteryRemaining => batteryRemaining;

    private void Awake()
    {
        // Busca la luz en este objeto o en sus hijos.
        if (torchLight == null)
            torchLight = GetComponentInChildren<Light>(true);

        if (emissionMaterial == null)
            emissionMaterial =
                GetComponentInChildren<EmissionMaterialGlassTorchFadeOut>(true);

        batteryRemaining = Mathf.Clamp(
            batteryRemaining, 0f, batteryCapacity
        );

        SetLightState(false);
    }

    private void Update()
    {
        if (!flashLightOn)
            return;

        // En el modo con batería, la linterna consume energía.
        if (modoLightChoose == ModoLightChoose.withBattery)
        {
            batteryRemaining -= batteryDrainPerSecond * Time.deltaTime;
            batteryRemaining = Mathf.Max(0f, batteryRemaining);

            if (batteryRemaining <= 0f)
                SetLightState(false);
        }
    }

    public void ToggleLightState()
    {
        SetLightState(!flashLightOn);
    }

    public void SetLightState(bool isOn)
    {
        // No permite encender una linterna sin batería.
        if (isOn &&
            modoLightChoose == ModoLightChoose.withBattery &&
            batteryRemaining <= 0f)
        {
            isOn = false;
        }

        flashLightOn = isOn;

        if (torchLight != null)
        {
            torchLight.enabled = flashLightOn;
            torchLight.intensity = intensityLight;
        }

        if (emissionMaterial != null)
        {
            emissionMaterial.SetEmission(
                flashLightOn ? intensityLight : 0f
            );
        }
    }

    public void AddBattery(float amount)
    {
        if (amount <= 0f)
            return;

        batteryRemaining = Mathf.Clamp(
            batteryRemaining + amount,
            0f,
            batteryCapacity
        );
    }

    public void SetIntensity(float newIntensity)
    {
        intensityLight = Mathf.Max(0f, newIntensity);

        if (torchLight != null)
            torchLight.intensity = intensityLight;

        if (emissionMaterial != null && flashLightOn)
            emissionMaterial.SetEmission(intensityLight);
    }
}