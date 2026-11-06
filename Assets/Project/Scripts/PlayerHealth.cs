using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    [SerializeField, Min(1)] private int maxHits = 4;

    [Header("UI de daño")]
    [SerializeField] private Image damageOverlay;
    [SerializeField] private Image woundMarks;
    [SerializeField] private Image darkOverlay;
    [SerializeField] private GameObject deathPanel;

    [Header("Efectos visuales")]
    [SerializeField, Range(0f, 1f)] private float redAlpha = 0.3f;
    [SerializeField, Range(0f, 1f)] private float redMarksAlpha = 0.65f;
    [SerializeField, Range(0f, 1f)] private float darkAlpha = 0.8f;

    [Header("Al morir")]
    [SerializeField] private MonoBehaviour[] controlsToDisable;

    [Header("Audio")]
    [SerializeField] private SfxVariado sfxGolpe;

    private int hitsReceived;
    private bool isDead;

    public int HitsReceived => hitsReceived;
    public int HitsRemaining => Mathf.Max(0, maxHits - hitsReceived);
    public bool IsDead => isDead;

    private void Start()
    {
        SetAlpha(damageOverlay, 0f);
        SetAlpha(woundMarks, 0f);
        SetAlpha(darkOverlay, 0f);

        if (deathPanel != null)
            deathPanel.SetActive(false);
    }

    public void TakeDamage()
    {
        TakeHit();
    }

    public void TakeHit()
    {
        if (isDead)
            return;

        if (sfxGolpe != null)
            sfxGolpe.Reproducir();

        hitsReceived = Mathf.Min(hitsReceived + 1, maxHits);

        if (hitsReceived >= maxHits)
            Die();
        else
            UpdateDamageVisuals();
    }

    private void UpdateDamageVisuals()
    {
        if (hitsReceived == 1)
            SetAlpha(damageOverlay, redAlpha);

        if (hitsReceived == 2 && maxHits >= 4)
        {
            SetAlpha(damageOverlay, redAlpha);
            SetAlpha(woundMarks, redMarksAlpha);
        }

        if (hitsReceived == maxHits - 1)
        {
            SetAlpha(damageOverlay, 0f);
            SetAlpha(woundMarks, 0f);
            SetAlpha(darkOverlay, darkAlpha);
        }
    }

    private void Die()
    {
        isDead = true;

        SetAlpha(damageOverlay, 0f);
        SetAlpha(woundMarks, 0f);
        SetAlpha(darkOverlay, 1f);

        if (deathPanel != null)
            deathPanel.SetActive(true);

        foreach (MonoBehaviour control in controlsToDisable)
        {
            if (control != null)
                control.enabled = false;
        }
    }

    private static void SetAlpha(Image image, float alpha)
    {
        if (image == null)
            return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
