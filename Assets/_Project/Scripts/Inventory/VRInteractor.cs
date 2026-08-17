using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;


/* ──────────────────────────────────────────────────────────────────────────── */
/*  VRInteractor                                                               */
/* ──────────────────────────────────────────────────────────────────────────── */
public class VRInteractor : MonoBehaviour
{
    /* ───────── Detection (items) ───────── */
    [Header("Detection")]
    [Min(0.1f)] public float pickupRadius = 2f;
    public LayerMask pickupMask;
    [Min(0f)] public float scanInterval = 0.1f;

    /* ───────── Help‑Button Hover Press (NEW) ───────── */
    [Header("Help‑Button Press")]
    public LayerMask helpButtonMask;                       // set to “HelpButton”
    [Range(0f, 1f)] public float triggerThreshold = 0.1f; // shared with pivot

    /* ───────── Input (XR) ───────── */
    [Header("Input (XR)")]
    [Tooltip("Controller that holds the A‑button + right trigger.")]
    public XRNode controllerNode = XRNode.RightHand;

    /* ───────── Pivot Rotation (unchanged) ───────── */
    [Header("Pivot Point Rotation")]
    public Transform pivotPoint;              // drag “Pivot Point” here
    public XRNode leftControllerNode = XRNode.LeftHand;

    private AudioSource audioSource;
    public AudioClip yay;

    /* ───────── Internals ───────── */
    readonly Collider[] hits = new Collider[16];

    ItemInScenes current;                     // nearest inventory item
    HelpButton hoveredButton;               // nearest help button
    float nextScan;

    InputDevice device;                       // right controller
    InputDevice leftDevice;                   // left  controller

    Quaternion pivotRestRot;
    bool pivotOn;
    bool rtHeldPrev;                    // rising‑edge test for right trigger


    /* ───────── Unity Lifecycle ───────── */
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        device = InputDevices.GetDeviceAtXRNode(controllerNode);
        leftDevice = InputDevices.GetDeviceAtXRNode(leftControllerNode);

        if (pivotPoint) pivotRestRot = pivotPoint.localRotation;
    }

    void Update()
    {
        /* 0 ── keep devices valid */
        if (!device.isValid)
            device = InputDevices.GetDeviceAtXRNode(controllerNode);

        if (!leftDevice.isValid)
            leftDevice = InputDevices.GetDeviceAtXRNode(leftControllerNode);

        /* 1 ── periodic scans */
        if (scanInterval == 0f || Time.time >= nextScan)
        {
            ScanForItems();
            ScanForHelpButtons();             // << NEW
            nextScan = Time.time + scanInterval;
        }

        /* 2 ── A‑button → pick up item */
        if (current &&
            device.TryGetFeatureValue(CommonUsages.primaryButton, out bool aPressed) &&
            aPressed)
        {
            if (VRInventoryManager.Instance.AddItem(current.data, current.quantity))
                Destroy(current.gameObject);
                audioSource.PlayOneShot(yay);

        }

        // /* 3 ── RIGHT trigger → press hovered help button */
        // if (device.TryGetFeatureValue(CommonUsages.trigger, out float rtVal))
        // {
        //     bool rtHeld = rtVal > triggerThreshold;

        //     if (rtHeld && !rtHeldPrev && hoveredButton)     // rising edge only
        //         hoveredButton.Press();

        //     rtHeldPrev = rtHeld;
        // }

        /* 4 ── LEFT trigger → flip pivot point */
        if (pivotPoint &&
            leftDevice.TryGetFeatureValue(CommonUsages.trigger, out float ltVal))
        {
            bool held = ltVal > triggerThreshold;

            if (held && !pivotOn)
            {
                pivotPoint.localRotation = Quaternion.Euler(90f, 0f, 0f);
                pivotOn = true;


            }
            else if (!held && pivotOn)
            {
                pivotPoint.localRotation = pivotRestRot;
                pivotOn = false;
            }
        }
    }

    /* ───────── Item Scan (as before) ───────── */
    void ScanForItems()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, pickupRadius, hits, pickupMask,
            QueryTriggerInteraction.Collide);

        current = null;
        float bestSqr = float.PositiveInfinity;

        for (int i = 0; i < count; i++)
        {
            var wi = hits[i].GetComponent<ItemInScenes>();
            if (!wi) continue;

            float sqr = (wi.transform.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                current = wi;
            }
        }
    }

    /* ───────── Help‑Button Scan (NEW) ───────── */
    void ScanForHelpButtons()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, pickupRadius, hits, helpButtonMask,
            QueryTriggerInteraction.Collide);

        hoveredButton = null;
        float bestSqr = float.PositiveInfinity;

        for (int i = 0; i < count; i++)
        {
            var hb = hits[i].GetComponent<HelpButton>();
            if (!hb) continue;

            float sqr = (hb.transform.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                hoveredButton = hb;
            }
        }
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (!other.CompareTag("validItem")) return;

    //     if (VRInventoryManager.Instance.AddItem(current.data, current.quantity))
    //         Destroy(current.gameObject);
    //     Debug.Log("Item was successfully collected");
    // }


#if UNITY_EDITOR
    /* ───────── Debug ───────── */
    [Header("Debug")]
    public bool drawGizmo = true;
    void OnDrawGizmosSelected()
    {
        if (!drawGizmo) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
#endif
}
/* ──────────────────────────────────────────────────────────────────────────── */
/*  Simple “HelpButton” script for your buttons                                */
/* ──────────────────────────────────────────────────────────────────────────── */
public class HelpButton : MonoBehaviour
{
    public UnityEvent onPressed;

    /// <summary>Called by VRInteractor when the right trigger is squeezed.</summary>
    public void Press() => onPressed?.Invoke();
}



