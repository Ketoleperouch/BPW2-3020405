using UnityEngine;

public class RotateObject : MonoBehaviour {

    public enum Axis { X,Y,Z};
    public Axis axis;
    public Space space = Space.Self;

    public float turnSpeed;

    private void Update()
    {
        Vector3 a = Vector3.zero;
        switch (axis)
        {
            case Axis.X:
                a = Vector3.right;
                break;
            case Axis.Y:
                a = Vector3.up;
                break;
            case Axis.Z:
                a = Vector3.forward;
                break;
        }

        transform.Rotate(a, turnSpeed * Time.deltaTime);
    }
}
