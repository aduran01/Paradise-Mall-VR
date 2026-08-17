using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallGuide : MonoBehaviour
{
     public float guideForce = 5f;
    
    // The direction the ball should be guided.
    // You can set this in the inspector. For example, if your ramp is oriented so that its
    // forward vector is the upward (rolling) direction, then Vector3.forward is appropriate.
    public Vector3 guidanceDirection = Vector3.forward;

    // This function is called every frame the ball remains within the trigger collider.
    private void OnTriggerStay(Collider other)
    {
        // Check if the collider belongs to the ball by verifying its tag.
        if (other.CompareTag("Ball"))
        {
            // Get the Rigidbody component of the ball.
            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // Normalize the guidance direction to ensure consistent force regardless of vector magnitude.
                // Then, add the force to the ball using Acceleration mode for a smooth influence.
                rb.AddForce((-transform.forward).normalized * guideForce, ForceMode.Acceleration);
            }
        }
    }
}
