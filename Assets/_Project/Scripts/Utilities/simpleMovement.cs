using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simpleMovement : MonoBehaviour
{
    public float moveSpeed = 5f;           // Movement speed
    public float mouseSensitivity = 150f;  // Mouse sensitivity

    private float xRotation = 0f;          // Vertical rotation of the camera
    private Transform playerCamera;        // Reference to the camera transform

    void Start()
    {
        // Find the camera component in the child objects
        playerCamera = GetComponentInChildren<Camera>().transform;

        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // ----- Movement -----

        // Get input from arrow keys
        float moveHorizontal = Input.GetAxis("Horizontal"); // Left and Right arrow keys
        float moveVertical = Input.GetAxis("Vertical");     // Up and Down arrow keys

        // Create a movement vector based on input
        Vector3 movement = new Vector3(moveHorizontal, 0, moveVertical);

        // Move the object relative to its own orientation
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.Self);

        // ----- Mouse Look -----

        // Get mouse movement
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the player object around the Y-axis (horizontal rotation)
        transform.Rotate(Vector3.up * mouseX);

        // Adjust the vertical rotation of the camera and clamp it
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevent flipping over

        // Rotate the camera around its local X-axis (vertical rotation)
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
