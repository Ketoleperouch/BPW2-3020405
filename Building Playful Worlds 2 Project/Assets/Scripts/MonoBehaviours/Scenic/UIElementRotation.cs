using UnityEngine;

public class UIElementRotation : MonoBehaviour {

    public float speed = 15f;

    private void Update()
    {
        transform.Rotate(0, 0, speed * Time.deltaTime);
    }
}
