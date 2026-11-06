
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    [SerializeField, Min(1)] private int maxHits = 4;

    [Header("Recuperacion")]
    [SerializeField] private float tiempoRecuperacion = 6f;

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
    private Coroutine recoveryCoroutine;

    public int HitsReceived => hitsReceived;
    public int HitsRemaining => Mathf.Max(0, maxHits - hitsReceived);
    public bool IsDead => isDead;

    private void Start()
    {
        ResetDamageVisuals();

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

        UpdateDamageVisuals();

        if (hitsReceived >= maxHits)
        {
            Die();
            return;
        }

        // Cada nuevo golpe reinicia el tiempo de recuperación.
        if (recoveryCoroutine != null)
            StopCoroutine(recoveryCoroutine);

        recoveryCoroutine = StartCoroutine(RecoverAfterTime());
    }

    private IEnumerator RecoverAfterTime()
    {
        yield return new WaitForSeconds(tiempoRecuperacion);

        hitsReceived = 0;
        ResetDamageVisuals();

        recoveryCoroutine = null;
    }

    private void UpdateDamageVisuals()
    {
        if (hitsReceived == 1)
            SetAlpha(damageOverlay, redAlpha);

        if (hitsReceived == 2 && maxHits >= 4)
            SetAlpha(woundMarks, redMarksAlpha);

        if (hitsReceived == maxHits - 1)
        {
            SetAlpha(damageOverlay, 0f);
            SetAlpha(woundMarks, 0f);
            SetAlpha(darkOverlay, darkAlpha);
        }
    }

    private void ResetDamageVisuals()
    {
        SetAlpha(damageOverlay, 0f);
        SetAlpha(woundMarks, 0f);
        SetAlpha(darkOverlay, 0f);
    }

    private void Die()
    {
        isDead = true;

        if (recoveryCoroutine != null)
        {
            StopCoroutine(recoveryCoroutine);
            recoveryCoroutine = null;
        }

        SetAlpha(damageOverlay, 0f);
        SetAlpha(woundMarks, 0f);
        SetAlpha(darkOverlay, 1f);

        if (deathPanel != null)
            deathPanel.SetActive(true);
        else
            Debug.LogWarning("No asignaste Death Panel en PlayerHealth.");

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
