using UnityEngine;

[RequireComponent(typeof(Light))]
public class discoLight : MonoBehaviour
{
    public float cycleSpeed = 0.2f; // Speed at which color changes

    private Light spotLight;
    private float hue = 0f;

    void Start()
    {
        spotLight = GetComponent<Light>();
    }

    void Update()
    {
        // Increase hue over time, looping from 0 to 1
        hue += cycleSpeed * Time.deltaTime;
        if (hue > 1f) hue -= 1f;

        // Convert HSV to RGB and assign it
        Color color = Color.HSVToRGB(hue, 1f, 1f);
        spotLight.color = color;
    }
}
