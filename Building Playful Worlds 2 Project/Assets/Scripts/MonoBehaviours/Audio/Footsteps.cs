using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Footsteps : MonoBehaviour {

    //This script uses Animation Events to play footsteps audio and add dust to footsteps.
    public AudioClip[] footstepSoundsGeneric;
    public AudioClip[] footstepSoundsMetal;
    public GameObject[] footstepDustGeneric;
    public GameObject[] footstepDustMetal;
    public Transform rightFoot;
    public Transform leftFoot;

    public LayerMask groundLayers;
    public float volume = 0.25f;

    [SerializeField] bool isScenic = false;

    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Footstep(int foot) //foot 0 is right foot, foot 1 is left foot
    {
        //Check some conditions if Footstep() is actually possible.
        if (!isScenic)
        {
            if (anim.GetFloat("Speed") < .25f)
                return;
        }

        //Do a Raycast to get information about the material.
        RaycastHit ground;
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out ground, 0.2f, groundLayers))
        {
            //Do a special sound/dust if the ground has a specific material
            if (ground.collider.GetComponent<PhysicMaterial>())
            {
                Debug.Log(ground.collider.GetComponent<PhysicMaterial>().name);
            }
            //Otherwise, just play the generic footstep sound and add generic dust.
            else
            {
                AudioSource.PlayClipAtPoint(footstepSoundsGeneric[Rn(footstepSoundsGeneric)], ground.point, volume);
                Instantiate(footstepDustGeneric[Rn(footstepDustGeneric)], foot == 0 ? rightFoot.position : leftFoot.position, Quaternion.identity);
            }
        }
    }

    //Take a random int of an array
    private int Rn(System.Array array)
    {
        return Random.Range(0, array.Length);
    }
}
