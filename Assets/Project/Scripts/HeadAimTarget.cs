
using UnityEngine;

public class HeadAimTarget : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Transform head;
    [SerializeField] private Transform characterRoot;

    [Header("Distancia")]
    [SerializeField] private float distance = 5f;

    [Header("Limites")]
    [SerializeField] private float maxYaw = 70f;
    [SerializeField] private float maxPitchUp = 35f;
    [SerializeField] private float maxPitchDown = 25f;

    [Header("Suavidad")]
    [SerializeField] private float smoothSpeed = 8f;

    private void LateUpdate()
    {
        if (cameraTarget == null ||
            head == null ||
            characterRoot == null)
            return;

        // Dirección de la cámara expresada respecto
        // de la orientación estable del personaje.
        Vector3 localDirection =
            characterRoot.InverseTransformDirection(
                cameraTarget.forward
            );

        float yaw = Mathf.Atan2(
            localDirection.x,
            localDirection.z
        ) * Mathf.Rad2Deg;

        float pitch = -Mathf.Atan2(
            localDirection.y,
            new Vector2(
                localDirection.x,
                localDirection.z
            ).magnitude
        ) * Mathf.Rad2Deg;

        yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);
        pitch = Mathf.Clamp(
            pitch,
            -maxPitchDown,
            maxPitchUp
        );

        Vector3 limitedDirection =
            characterRoot.rotation *
            Quaternion.Euler(pitch, yaw, 0f) *
            Vector3.forward;

        Vector3 targetPosition =
            head.position + limitedDirection * distance;

        float t = 1f - Mathf.Exp(
            -smoothSpeed * Time.deltaTime
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            t
        );
    }
}
