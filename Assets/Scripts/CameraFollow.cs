using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 6f, -6f);
    public float smoothSpeed = 6f;
    public float lookAhead = 8f;

    private Transform target;
    private Vector3 desiredPos;
    private Vector3 currentPos;

    void Start()
    {
        GameObject ball = GameObject.FindWithTag("Player");
        if (ball != null)
            target = ball.transform;
        
        currentPos = transform.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        desiredPos = target.position + offset;
        currentPos = Vector3.Lerp(currentPos, desiredPos, smoothSpeed * Time.deltaTime);
        transform.position = currentPos;

        Vector3 lookTarget = target.position + Vector3.forward * lookAhead;
        lookTarget.y = target.position.y + 0.5f;
        transform.LookAt(lookTarget);
    }
}