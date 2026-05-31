using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Movement")]
    public float forwardForce = 30f;
    public float turnForce = 40f;
    public float maxSpeed = 25f;
    public float gravity = 120f; // Gravidade customizável

    private Rigidbody rb;
    private GameManager gameManager;
    private Collider ballCollider;
    private bool isAlive = true;
    private float fallThreshold = -5f;
    private bool initialized = false;
    private int frameCount = 0;
    private float ballRadius = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindFirstObjectByType<GameManager>();
        ballCollider = GetComponent<Collider>();
        frameCount = 0;
        
        // Desabilitar gravidade padrão para usar gravidade customizável
        rb.useGravity = false;
        
        // Cache o raio da bola
        ballRadius = ballCollider.bounds.extents.y;
    }

    void Update()
    {
        // Espera alguns frames para garantir que a pista foi totalmente construída
        frameCount++;
        if (!initialized && frameCount > 5)
        {
            // Posiciona bem alto e usa raycast para encontrar a pista
            Vector3 startPos = new Vector3(0, 50f, 0);
            transform.position = startPos;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            // Raycast para encontrar a superfície da pista
            RaycastHit hit;
            if (Physics.Raycast(startPos, Vector3.down, out hit, 100f))
            {
                // Posiciona a bola na superfície da pista + raio da bola
                transform.position = new Vector3(hit.point.x, hit.point.y + ballRadius, hit.point.z);
            }
            
            initialized = true;
        }
    }

    void FixedUpdate()
    {
        if (!isAlive) return;

        // Aplicar gravidade customizada
        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        // Forward force - seguir a direção da pista
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
        float horizontalMagSq = vel.x * vel.x + vel.z * vel.z;
        float maxSpeedSq = maxSpeed * maxSpeed;
        if (horizontalMagSq > maxSpeedSq)
        {
            float scale = maxSpeed / Mathf.Sqrt(horizontalMagSq);
            rb.linearVelocity = new Vector3(vel.x * scale, vel.y, vel.z * scale);
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