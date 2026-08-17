using UnityEngine;
using UnityEngine.EventSystems;

public class pressHelp : MonoBehaviour, IPointerClickHandler
{
    //private AudioSource audioSource;
    //public AudioClip help;

    void Start()
    {
      //audioSource = GetComponent<AudioSource>();
    }
    // Called when the user presses the Select / Trigger while hovering
    public void OnPointerClick(PointerEventData eventData)
    {
       GetComponent<AudioSource>().Play();
    }
}
