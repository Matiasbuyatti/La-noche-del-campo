using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Escena del juego")]
    [SerializeField] private string gameSceneName = "Scene_MatiasBuyatti";

    [Header("Paneles")]
    [SerializeField] private GameObject tutorialPanel;

    [Header("Botones (opcional)")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button closeTutorialButton;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);

        if (tutorialButton != null)
            tutorialButton.onClick.AddListener(OpenTutorial);

        if (closeTutorialButton != null)
            closeTutorialButton.onClick.AddListener(CloseTutorial);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(PlayGame);

        if (tutorialButton != null)
            tutorialButton.onClick.RemoveListener(OpenTutorial);

        if (closeTutorialButton != null)
            closeTutorialButton.onClick.RemoveListener(CloseTutorial);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(ExitGame);
    }

    public void PlayGame()
    {
        if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            Debug.LogError($"MainMenuController: la escena '{gameSceneName}' no está en Build Profiles.", this);
            return;
        }

        Time.timeScale = 1f;

        // El estado del cursor se mantiene al cambiar de escena.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
    }

    public void CloseTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
