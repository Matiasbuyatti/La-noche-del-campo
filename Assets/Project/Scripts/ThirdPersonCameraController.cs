
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player;

    [Header("Posición")]
    [SerializeField] private float targetHeight = 1.4f;

    [Header("Ratón")]
    [SerializeField] private float mouseSensitivity = 0.12f;

    [Header("Gamepad")]
    [SerializeField] private float gamepadSensitivity = 120f;

    [Header("Límites verticales")]
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 65f;

    private float yaw;
    private float pitch = 10f;

    private Vector2 mouseDelta;
    private Vector2 gamepadLook;

    private void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = 10f;
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        // Mantiene el objetivo a la altura del torso/cabeza.
        transform.position =
            player.position + Vector3.up * targetHeight;

        // El ratón utiliza delta, sin multiplicar por deltaTime.
        yaw += mouseDelta.x * mouseSensitivity;
        pitch -= mouseDelta.y * mouseSensitivity;
        mouseDelta = Vector2.zero;

        // El stick del mando es una entrada continua.
        yaw += gamepadLook.x
            * gamepadSensitivity * Time.deltaTime;

        pitch -= gamepadLook.y
            * gamepadSensitivity * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(
            pitch, yaw, 0f);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();

        if (context.control != null &&
            context.control.device is Mouse)
        {
            if (context.performed)
                mouseDelta += value;
        }
        else
        {
            gamepadLook = value;
        }
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
