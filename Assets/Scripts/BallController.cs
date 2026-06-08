using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Movement")]
    public float forwardForce = 30f;
    public float turnForce = 15f;
    public float maxSpeed = 25f;

    private Rigidbody rb;
    private GameManager gameManager;
    private bool isAlive = true;
    private float fallThreshold = -20f;
    private bool started = false;
    private float startDelay = 0.3f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindFirstObjectByType<GameManager>();
        // Freeze ball until track is ready
        rb.isKinematic = true;
        // Move ball up to avoid falling through
        transform.position = new Vector3(transform.position.x, 3f, transform.position.z);
        Invoke("StartBall", startDelay);
    }

    void StartBall()
    {
        rb.isKinematic = false;
        started = true;
    }

    void FixedUpdate()
    {
        if (!isAlive || !started) return;

        // Forward force
        rb.AddForce(Vector3.forward * forwardForce, ForceMode.Acceleration);

        // Lateral steering
        float horizontal = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            horizontal = -1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            horizontal = 1f;

        rb.AddForce(Vector3.right * horizontal * turnForce, ForceMode.Acceleration);

        // Cap speed
        Vector3 vel = rb.linearVelocity;
        Vector3 horizontalVel = new Vector3(vel.x, 0, vel.z);
        if (horizontalVel.magnitude > maxSpeed)
        {
            horizontalVel = horizontalVel.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(horizontalVel.x, vel.y, horizontalVel.z);
        }

        // Fall detection
        if (transform.position.y < fallThreshold)
        {
            Die();
        }
    }

    public void Die()
    {
        if (!isAlive) return;
        isAlive = false;
        if (gameManager != null)
            gameManager.GameOver();
    }

    public void SetAlive(bool alive)
    {
        isAlive = alive;
    }
}