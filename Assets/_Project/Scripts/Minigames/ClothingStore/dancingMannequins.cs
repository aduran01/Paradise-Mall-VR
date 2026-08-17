using System.Collections;
using UnityEngine;

public class dancingMannequins : MonoBehaviour
{
    [Header("Characters that share ONE Animator Controller")]
    public Animator[] animators;

    [Header("Exact state names in the Base Layer")]
    public string[] stateNames;

    [Tooltip("Seconds for the cross‑fade between clips")]
    public float crossFadeDuration = 0.1f;

    [Tooltip("Prevent the same state playing twice in a row")]
    public bool avoidImmediateRepeat = true;

    private void Start()
    {
        if (stateNames.Length == 0)
        {
            Debug.LogError("RandomAnimationManager: stateNames list is empty!");
            enabled = false;
            return;
        }

        foreach (Animator a in animators)
            StartCoroutine(RandomLoop(a));
    }

    private IEnumerator RandomLoop(Animator anim)
    {
        int previousIndex = -1;

        // Give the Animator one frame to settle on its default state
        yield return null;

        while (true)
        {
            /* ---------- 1. Pick a new state ---------- */
            int index;
            do { index = Random.Range(0, stateNames.Length); }
            while (avoidImmediateRepeat && stateNames.Length > 1 && index == previousIndex);

            previousIndex = index;
            string state = stateNames[index];

            /* ---------- 2. Cross‑fade into it ---------- */
            anim.CrossFadeInFixedTime(state, crossFadeDuration);

            /* ---------- 3. Wait until that state is ACTIVE ---------- */
            yield return new WaitUntil(() =>
                anim.GetCurrentAnimatorStateInfo(0).IsName(state));

            /* ---------- 4. Wait for the clip’s true length ---------- */
            var info = anim.GetCurrentAnimatorStateInfo(0);

            // AnimatorStateInfo.length already factors in clip speed
            float remaining = info.length - (info.normalizedTime * info.length);
            if (remaining < 0f) remaining = 0f;

            yield return new WaitForSeconds(remaining);
            // Loop back to step 1
        }
    }
}
