using UnityEngine;
using UnityEngine.InputSystem;

public class ChooseLightCookie : MonoBehaviour
{
    [Header("Luz")]
    [SerializeField] private Light torchLight;

    [Header("Texturas del haz")]
    [SerializeField] private Texture[] lightCookies;

    [Header("Entrada")]
    [SerializeField] private InputActionReference changeCookieAction;

    private int currentCookieIndex;

    private void Awake()
    {
        if (torchLight == null)
            torchLight = GetComponentInChildren<Light>(true);
    }

    private void OnEnable()
    {
        if (changeCookieAction == null ||
            changeCookieAction.action == null)
            return;

        changeCookieAction.action.performed += OnChangeCookie;
        changeCookieAction.action.Enable();
    }

    private void OnDisable()
    {
        if (changeCookieAction == null ||
            changeCookieAction.action == null)
            return;

        changeCookieAction.action.performed -= OnChangeCookie;
    }

    private void OnChangeCookie(InputAction.CallbackContext context)
    {
        if (torchLight == null ||
            lightCookies == null ||
            lightCookies.Length == 0)
            return;

        currentCookieIndex =
            (currentCookieIndex + 1) % lightCookies.Length;

        torchLight.cookie = lightCookies[currentCookieIndex];
    }
}