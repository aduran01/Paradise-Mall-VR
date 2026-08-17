using UnityEngine;
using UnityEngine.InputSystem; // Only used if the new Input System is installed

public class debugger : MonoBehaviour
{
     [Header("XRI Default Input Actions")]
    public InputActionReference move;
    public InputActionReference turn;
    void Start()
    {
#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
        Debug.Log("Both Input Systems are enabled (New + Old).");
#elif ENABLE_INPUT_SYSTEM
        Debug.Log("Using the New Input System.");
#elif ENABLE_LEGACY_INPUT_MANAGER
        Debug.Log("Using the Legacy Input Manager.");
#else
        Debug.Log("No input system is enabled.");
#endif
    }

    

    void Update()
    {
        Debug.Log($"Move: {move.action.enabled}  Turn: {turn.action.enabled}");
    }
}