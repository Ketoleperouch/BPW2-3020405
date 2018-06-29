using UnityEngine;

public class Ragdolify : MonoBehaviour {

    [System.Serializable]
    public struct Bone
    {
        [Tooltip("The transform of the bone that should be affected.")]
        public Transform boneTransform;
        [Tooltip("The transform of the bone that the character joint should be attached to.")]
        public Transform boneParentTransform;
        public enum ColliderType { Box, Capsule };
        [Tooltip("The type of collider you want to attach to the bone.")]
        public ColliderType colliderType;
        [Tooltip("The size of the collider.\nIn Box Colliders, this determines the bounding box size, not the half extents.\nIn Capsule Colliders, the X value determines the radius and the Y value determines the height and the Z value determines the direction.\n(0 = X, 1 = Y, 2 = Z)")]
        public Vector3 colliderMeasures;
        [Tooltip("The offset of the collider relative to the bone.")]
        public Vector3 colliderOffset;
        [Tooltip("Check this if the bone should only get a rigidbody (for dropped weapons and such).")]
        public bool onlyRigidbody;
        [Tooltip("Check this if the bone should be unparented (for dropped weapons and such).")]
        public bool unparent;
    };

    public Bone[] bones;
    public bool makeRagdoll = false;

    public Rigidbody rb { get { return GetComponent<Rigidbody>(); } }

    private void Update()
    {
        if (makeRagdoll)
        {
            GetComponent<PlayerHealth>().TakeDamage(100, transform.position);
            makeRagdoll = false;
        }
    }

    public void MakeRagdoll()
    {
        rb.isKinematic = false;
        //Make Ragdoll
        for (int i = 0; i < bones.Length; i++)
        {
            AddBone(bones[i].boneTransform.gameObject, bones[i]);
        }
    }

    private void AddBone(GameObject target, Bone info)
    {
        //Add a rigidbody to the bone
        if (!target.GetComponent<Rigidbody>())
        {
            Rigidbody trb = target.AddComponent<Rigidbody>();
            trb.mass = 10f;
        }

        if (info.unparent)
        {
            target.transform.parent = null;
        }

        if (info.onlyRigidbody)
        {
            return;
        }

        //Check which type of collider should be added
        if (info.colliderType == 0)
        {
            BoxCollider boneCollider = target.AddComponent<BoxCollider>();
            boneCollider.center += info.colliderOffset;
            boneCollider.size = info.colliderMeasures;
        }
        else
        {
            CapsuleCollider boneCollider = target.AddComponent<CapsuleCollider>();
            boneCollider.center += info.colliderOffset;
            boneCollider.radius = info.colliderMeasures.x;
            boneCollider.height = info.colliderMeasures.y;
            boneCollider.direction = (int)info.colliderMeasures.z;
        }

        //Create a character joint and attach to parent bone
        CharacterJoint joint = target.AddComponent<CharacterJoint>();

        //Make sure there is a parent rigidbody to attach to
        if (info.boneParentTransform)
        {
            if (!info.boneParentTransform.GetComponent<Rigidbody>())
            {
                info.boneParentTransform.gameObject.AddComponent<Rigidbody>();
            }
            joint.connectedBody = info.boneParentTransform.GetComponent<Rigidbody>();
        }
        else
        {
            joint.connectedBody = rb;
        }
    }
}
