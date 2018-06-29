using UnityEngine;
using UnityEngine.Events;

public class StationElevator : MonoBehaviour {

    public Animator animator;
    public bool activate;
    public Vector3 size, offset;
    public LayerMask activatorLayers;
    public UnityEvent onActivate;

    private void Update()
    {
        Collider[] inTrigZone = Physics.OverlapBox(transform.position + offset, size / 2, Quaternion.identity, activatorLayers);
        for (int i = 0; i < inTrigZone.Length; i++)
        {
            if (inTrigZone[i].CompareTag("Player") && Input.GetKeyDown(KeyCode.E) && !animator.enabled)
            {
                activate = true;
            }
        }
        if (activate)
        {
            //Parent the player to the object to remove stuttering
            OnActivate(FindObjectOfType<MissionController>().transform);
            activate = false;
        }
    }

    private void OnActivate(Transform activator)
    {
        animator.enabled = true;
        activator.SetParent(transform);
        onActivate.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + offset, size);
    }
}
