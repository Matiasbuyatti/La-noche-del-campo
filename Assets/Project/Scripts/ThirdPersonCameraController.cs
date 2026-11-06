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
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        transform.position = player.position + Vector3.up * targetHeight;

        // El mouse es un delta por frame: no se multiplica por deltaTime. El stick sí.
        yaw += mouseDelta.x * mouseSensitivity + gamepadLook.x * gamepadSensitivity * Time.deltaTime;
        pitch -= mouseDelta.y * mouseSensitivity + gamepadLook.y * gamepadSensitivity * Time.deltaTime;
        mouseDelta = Vector2.zero;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();

        if (context.control != null && context.control.device is Mouse)
        {
            if (context.performed)
                mouseDelta += value;
        }
        else
        {
            gamepadLook = value;
        }
    }
}
