using UnityEngine;

public class StationDoor : MonoBehaviour {

    public AudioClip openSound;
    public AudioClip closeSound;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<EnemyController>())
        {
            if (!animator.GetBool("Open"))
            {
                AudioSource.PlayClipAtPoint(openSound, transform.position);
                animator.SetBool("Open", true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<EnemyController>())
        {
            AudioSource.PlayClipAtPoint(closeSound, transform.position);
            animator.SetBool("Open", false);
        }
    }
}
