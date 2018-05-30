using UnityEngine;

public class CameraLookAt : MonoBehaviour {

    public Transform target;

    private Vector3 offset;

    private void Update()
    {
        transform.LookAt(target.position + offset);
        offset += new Vector3(0, -Time.deltaTime, Time.deltaTime);
    }
}
