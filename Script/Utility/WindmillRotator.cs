using UnityEngine;

public class WindmillRotator : MonoBehaviour
{
    public float speed = 60f; // ��]���x�i�x/�b�j

    void Update()
    {
        transform.Rotate(Vector3.forward, speed * Time.deltaTime);
    }
}
