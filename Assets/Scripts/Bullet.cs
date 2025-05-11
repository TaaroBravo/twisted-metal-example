using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void Update()
    {
        transform.position += transform.forward * 10 * Time.deltaTime;
    }
}