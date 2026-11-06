using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinalGameController : MonoBehaviour
{
    public static FinalGameController Instance { get; private set; }

    [Header("Pantalla final")]
    [SerializeField] private Image blackOverlay;
    [SerializeField] private TMP_Text victoryText;
    [SerializeField] private Button mainMenuButton;

    [Header("Audio final")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip finalVoice;

    [Header("Transición")]
    [SerializeField] private float fadeDuration = 3f;

    [Header("Menú principal")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool finalStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Hide(blackOverlay);
        Hide(victoryText);

        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(false);
            mainMenuButton.onClick.AddListener(VolverAlMenu);
        }
    }

    private void OnDestroy()
    {
        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(VolverAlMenu);

        if (Instance == this)
            Instance = null;
    }

    public void IniciarFinal()
    {
        if (finalStarted)
            return;

        finalStarted = true;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (audioSource != null && finalVoice != null)
        {
            audioSource.clip = finalVoice;
            audioSource.Play();
        }

        StartCoroutine(MostrarFinal());
    }

    private IEnumerator MostrarFinal()
    {
        if (blackOverlay != null)
        {
            blackOverlay.gameObject.SetActive(true);
            blackOverlay.transform.SetAsLastSibling();
        }

        // El juego está en timeScale 0, por eso el fade usa tiempo sin escalar.
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            if (blackOverlay != null)
            {
                Color color = blackOverlay.color;
                color.a = Mathf.Clamp01(elapsed / fadeDuration);
                blackOverlay.color = color;
            }

            yield return null;
        }

        if (blackOverlay != null)
            blackOverlay.color = Color.black;

        if (victoryText != null)
        {
            victoryText.gameObject.SetActive(true);
            victoryText.color = Color.white;
            victoryText.text = "¡HAS GANADO!";
            victoryText.transform.SetAsLastSibling();
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(true);
            mainMenuButton.transform.SetAsLastSibling();
        }
    }

    private void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private static void Hide(Graphic graphic)
    {
        if (graphic == null)
            return;

        Color color = graphic.color;
        color.a = 0f;
        graphic.color = color;
        graphic.gameObject.SetActive(false);
    }
}
