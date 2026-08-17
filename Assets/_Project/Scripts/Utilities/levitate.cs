using UnityEngine;

public class levitate : MonoBehaviour
{
    public float amplitude = 1f;    // height of the oscillation
    public float frequency = 1f;    // speed of the oscillation

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offsetY = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(startPos.x, startPos.y + offsetY, startPos.z);
    }
}
