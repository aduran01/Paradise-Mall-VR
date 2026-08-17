using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemInScenes : MonoBehaviour
{
    public ItemData data;   // Drag Gun.asset or Knife.asset here
    public int quantity = 1;

    // Nothing else needed; picked up by the interactor
}

