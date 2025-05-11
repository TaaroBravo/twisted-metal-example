using UnityEngine;

public class SimpleCarController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 10f;      
    public float turnSpeed = 100f;

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.A))
            transform.Rotate(Vector3.up, -turnSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.D))
            transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime);
    }
}
