using UnityEngine;

public class ArrowLightTrigger : MonoBehaviour
{
    [SerializeField] private GameObject leftArrowLight; 
    [SerializeField] private GameObject rightArrowLight; 
    [SerializeField] private GameObject upArrowLight; 
    [SerializeField] private GameObject downArrowLight; 

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object you collided with is named "LArrowTrigger"
        if (other.gameObject.name == "LArrowTrigger")
        {
            // Activate the Left Arrow Light GameObject
            leftArrowLight.SetActive(true);
        }

        if (other.gameObject.name == "RArrowTrigger")
        {
            // Activate the Left Arrow Light GameObject
            rightArrowLight.SetActive(true);
        }

        if (other.gameObject.name == "UArrowTrigger")
        {
            // Activate the Left Arrow Light GameObject
            upArrowLight.SetActive(true);
        }

        if (other.gameObject.name == "DArrowTrigger")
        {
            // Activate the Left Arrow Light GameObject
            downArrowLight.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // When leaving the trigger
        if (other.gameObject.name == "LArrowTrigger")
        {
            // Deactivate the Left Arrow Light
            leftArrowLight.SetActive(false);
        }

         if (other.gameObject.name == "RArrowTrigger")
        {
            // Activate the Left Arrow Light GameObject
            rightArrowLight.SetActive(false);
        }

        if (other.gameObject.name == "UArrowTrigger")
        {
            // Activate the Left Arrow Light GameObject
            upArrowLight.SetActive(false);
        }

        if (other.gameObject.name == "DArrowTrigger")
        {
            // Activate the Left Arrow Light GameObject
            downArrowLight.SetActive(false);
        }
    }
}
