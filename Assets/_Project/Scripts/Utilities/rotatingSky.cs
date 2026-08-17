using UnityEngine;

public class rotatingSky : MonoBehaviour
{
    public float rotationSpeed = 1f; // degrees per second
    private float currentRotation = 0f;

    void Update()
    {
        currentRotation += rotationSpeed * Time.deltaTime;
        RenderSettings.skybox.SetFloat("_Rotation", currentRotation);
    }
}
