using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeGripped : MonoBehaviour
{
    public Transform hand;

    Rigidbody rb;

    int tCount;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        tCount = 0; //set count in start, so no unneccesary updates.
    }

    private void OnTriggerEnter(Collider other) //grabbing on to block
    {
        if (other.CompareTag("Tip")) //find all instances of fingertips
        {
            tCount++;// adds up all fingertips that are touching the block

            if (tCount == 3) //if all fingertips are touching the block
            {
                transform.SetParent(hand);
                rb.isKinematic = true;
            }
        }
    }

    private void OnTriggerExit(Collider other) //letting go of block
    {
        if (other.CompareTag("Tip")) 
        {
            tCount--; //resets the counter and indicates that no fingertips are touching the block

            if (tCount < 2) // if only one finger is touching the block
            {
                transform.SetParent(null);
                rb.isKinematic = false;
            }
        }
    }
}

