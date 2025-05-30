using UnityEngine;

public class FloatyMovement : MonoBehaviour
{
    public float amplitude = 0.5f;   // 上下移動の幅
    public float frequency = 1f;     // 周期

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = startPos + new Vector3(0f, y, 0f);
    }
}
