using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arcadeLock : MonoBehaviour
{
    // Start is called before the first frame update
   void Update()
    {
        if (VRInventoryManager.Instance != null && VRInventoryManager.Instance.Items.Count >= 5)
        {
            gameObject.SetActive(false);
        }
    }
}