

using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [Header("Velocidades")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float speedRun = 30f;
    private float currentSpeed;

    [Header("Dash")]
    [SerializeField] private float dashForce = 15f;
    private float coolDowntimer = 0f;

    private Rigidbody rb;
    private Vector2 inputVector;
    private bool running;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentSpeed = speed;
    }

    void FixedUpdate()
    {
        currentSpeed = running ? speedRun : speed;

        Vector3 movement = new Vector3(inputVector.x, 0, inputVector.y);
        rb.AddForce(movement * currentSpeed, ForceMode.Acceleration);
    }

    private void Update()
    {
        if (coolDowntimer > 0)
        {
            coolDowntimer -= Time.deltaTime;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
    }

    public void Run(InputAction.CallbackContext context)
    {
        running = context.ReadValueAsButton();
    }


    public void Dash(InputAction.CallbackContext context)
    {
        if (coolDowntimer <= 0)
        {
            Vector3 dashDirection = new Vector3(inputVector.x, 0, inputVector.y);
            rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);
            coolDowntimer = 0;
        }
    }
}

