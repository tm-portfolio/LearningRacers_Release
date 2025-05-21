using UnityEngine;

public class FloatyMovement : MonoBehaviour
{
    public float amplitude = 0.5f;   // ã‰ºˆÚ“®‚Ì•
    public float frequency = 1f;     // üŠú

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
