using UnityEngine;

public class WindmillRotator : MonoBehaviour
{
    public float speed = 60f; // 回転速度（度/秒）

    void Update()
    {
        transform.Rotate(Vector3.forward, speed * Time.deltaTime);
    }
}
