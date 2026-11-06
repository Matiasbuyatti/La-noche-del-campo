using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ObjectivesUIController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ObjectiveManager objectiveManager;
    [SerializeField] private GameObject objectivesPanel;
    [SerializeField] private Button objectivesButton;

    [Header("Tecla para abrir/cerrar")]
    [SerializeField] private Key toggleKey = Key.O;

    [Header("Lista de objetivos")]
    [SerializeField] private Transform objectivesList;
    [SerializeField] private TMP_Text objectiveEntryPrefab;

    private void Awake()
    {
        if (objectivesButton != null)
            objectivesButton.onClick.AddListener(ToggleObjectivesPanel);

        if (objectivesPanel != null)
            objectivesPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (objectiveManager != null)
            objectiveManager.OnObjectivesChanged += RefreshObjectives;
    }

    private void Start()
    {
        RefreshObjectives();
    }

    private void Update()
    {
        // No se abre durante la pausa.
        if (Keyboard.current == null || Time.timeScale == 0f)
            return;

        if (Keyboard.current[toggleKey].wasPressedThisFrame)
            ToggleObjectivesPanel();
    }

    private void OnDisable()
    {
        if (objectiveManager != null)
            objectiveManager.OnObjectivesChanged -= RefreshObjectives;

        GameCursor.SetFree(this, false);
    }

    private void OnDestroy()
    {
        if (objectivesButton != null)
            objectivesButton.onClick.RemoveListener(ToggleObjectivesPanel);
    }

    public void ToggleObjectivesPanel()
    {
        if (objectivesPanel == null)
            return;

        bool open = !objectivesPanel.activeSelf;

        objectivesPanel.SetActive(open);
        GameCursor.SetFree(this, open);

        if (open)
            RefreshObjectives();
    }

    private void RefreshObjectives()
    {
        if (objectiveManager == null || objectivesList == null || objectiveEntryPrefab == null)
            return;

        for (int i = objectivesList.childCount - 1; i >= 0; i--)
        {
            Transform child = objectivesList.GetChild(i);

            if (child.gameObject != objectiveEntryPrefab.gameObject)
                Destroy(child.gameObject);
        }

        foreach (Objective objective in objectiveManager.Objectives)
        {
            TMP_Text entry = Instantiate(objectiveEntryPrefab, objectivesList);
            entry.gameObject.SetActive(true);
            entry.text = (objective.completed ? "✓ " : "○ ") + objective.title;
        }
    }
}
