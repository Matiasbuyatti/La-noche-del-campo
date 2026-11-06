using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;

    [FormerlySerializedAs("startButton")]
    [SerializeField] private Button closeButton;

    public event Action OnClosed;

    public bool IsOpen => tutorialPanel != null && tutorialPanel.activeSelf;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        // El tutorial nunca empieza abierto.
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);
    }

    public void Open()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
    }

    public void Close()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        OnClosed?.Invoke();
    }
}
