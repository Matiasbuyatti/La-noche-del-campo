
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

    private int hitsReceived;
    private bool isDead;

    public int HitsReceived => hitsReceived;
    public int HitsRemaining => Mathf.Max(0, maxHits - hitsReceived);
    public bool IsDead => isDead;

    private void Start()
    {
        if (damageOverlay != null)
            SetAlpha(damageOverlay, 0f);

        if (woundMarks != null)
            SetAlpha(woundMarks, 0f);

        if (darkOverlay != null)
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

        hitsReceived = Mathf.Min(hitsReceived + 1, maxHits);

        if (hitsReceived >= maxHits)
        {
            Die();
            return;
        }

        UpdateDamageVisuals();
    }

    private void UpdateDamageVisuals()
    {
        if (hitsReceived == 1)
        {
            if (damageOverlay != null)
                SetAlpha(damageOverlay, redAlpha);
        }

        if (hitsReceived == 2 && maxHits >= 4)
        {
            if (damageOverlay != null)
                SetAlpha(damageOverlay, redAlpha);

            if (woundMarks != null)
                SetAlpha(woundMarks, redMarksAlpha);
        }

        if (hitsReceived == maxHits - 1)
        {
            if (damageOverlay != null)
                SetAlpha(damageOverlay, 0f);

            if (woundMarks != null)
                SetAlpha(woundMarks, 0f);

            if (darkOverlay != null)
                SetAlpha(darkOverlay, darkAlpha);
        }
    }

    private void Die()
    {
        isDead = true;

        if (damageOverlay != null)
            SetAlpha(damageOverlay, 0f);

        if (woundMarks != null)
            SetAlpha(woundMarks, 0f);

        if (darkOverlay != null)
            SetAlpha(darkOverlay, 1f);

        if (deathPanel != null)
            deathPanel.SetActive(true);

        foreach (MonoBehaviour control in controlsToDisable)
        {
            if (control != null)
                control.enabled = false;
        }

        Debug.Log("El jugador ha muerto.");
    }

    private void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
