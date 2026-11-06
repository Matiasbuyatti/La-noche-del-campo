using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Los clics se detectan por código (posición del mouse vs. rectángulo del botón), sin EventSystem.
public class PauseMenuController : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject tutorialPanel;

    [Header("Botones de pausa")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Botón del tutorial")]
    [SerializeField] private Button startTutorialButton;

    [Header("Jugador")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private MonoBehaviour[] controlsToDisable;

    private const int MainMenuSceneIndex = 0;

    private readonly List<(Button button, Action action)> targets =
        new List<(Button button, Action action)>();

    private bool isPaused;

    private void Awake()
    {
        GameCursor.Clear();

        Register(continueButton, Resume);
        Register(restartButton, Restart);
        Register(tutorialButton, OpenTutorial);
        Register(mainMenuButton, GoToMainMenu);
        Register(startTutorialButton, Resume);
    }

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        GameCursor.SetFree(this, false);
    }

    private void Update()
    {
        HandleEscape();
        HandleMouseClick();
    }

    public void Pause() => SetPaused(true);

    public void Resume() => SetPaused(false);

    private void Register(Button button, Action action)
    {
        if (button != null)
            targets.Add((button, action));
    }

    private void HandleEscape()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (playerHealth != null && playerHealth.IsDead)
            return;

        if (tutorialPanel != null && tutorialPanel.activeSelf)
        {
            tutorialPanel.SetActive(false);

            if (pausePanel != null)
                pausePanel.SetActive(true);

            return;
        }

        SetPaused(!isPaused);
    }

    private void HandleMouseClick()
    {
        if (!isPaused || Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        foreach ((Button button, Action action) in targets)
        {
            if (button == null || !button.gameObject.activeInHierarchy || !button.interactable)
                continue;

            Canvas canvas = button.GetComponentInParent<Canvas>();

            Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

            if (RectTransformUtility.RectangleContainsScreenPoint(
                    (RectTransform)button.transform, mousePosition, cam))
            {
                action();
                return;
            }
        }
    }

    private void SetPaused(bool paused)
    {
        isPaused = paused;

        if (pausePanel != null)
            pausePanel.SetActive(paused);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        foreach (MonoBehaviour control in controlsToDisable)
        {
            if (control != null)
                control.enabled = !paused;
        }

        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;
        GameCursor.SetFree(this, paused);
    }

    private void OpenTutorial()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
    }

    private void Restart()
    {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void GoToMainMenu()
    {
        LoadScene(MainMenuSceneIndex);
    }

    private void LoadScene(int buildIndex)
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(buildIndex);
    }
}
