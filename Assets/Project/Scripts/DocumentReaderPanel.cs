
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DocumentReaderPanel : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject readerPanel;

    [Header("Contenido")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text contentText;

    [Header("Boton")]
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (readerPanel != null)
            readerPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CerrarDocumento);
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CerrarDocumento);

        GameCursor.SetFree(this, false);
    }

    public void AbrirDocumento(string titulo, string contenido)
    {
        if (readerPanel == null)
        {
            Debug.LogWarning("No asignaste Reader Panel.");
            return;
        }

        if (titleText != null)
            titleText.text = titulo;

        if (contentText != null)
            contentText.text = contenido;

        readerPanel.SetActive(true);
        GameCursor.SetFree(this, true);
    }

    public void CerrarDocumento()
    {
        if (readerPanel != null)
            readerPanel.SetActive(false);

        GameCursor.SetFree(this, false);
    }
}
