using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Transformer : MonoBehaviour
{
    public Vector3 translationRate;
    public Vector3 rotationRate;
    public string key;

    private KeyCode parsedKey;


void Start() {

     if (!string.IsNullOrEmpty(key))
    {
        if (Enum.TryParse(key, true, out parsedKey))
        {
            Debug.Log($"Parsed key: {parsedKey}");
        }
        else
        {
            Debug.LogWarning($"Could not parse '{key}' into a valid KeyCode.");
        }
    }
    
}
    void Update()
    {
       
        if (Input.GetKeyDown(parsedKey))
        {
            float direction = 1f;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                direction = -1f;
            } else {
                direction = 1f;
            }

            if (rotationRate != Vector3.zero)
            {
                transform.localRotation *= Quaternion.Euler(rotationRate * direction * Time.deltaTime);
            }

            if (translationRate != Vector3.zero)
            {
                transform.localPosition += translationRate * direction * Time.deltaTime;
            }
        

        }
    }
}

