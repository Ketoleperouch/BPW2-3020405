using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour {

    [Range(0, 100)]
    public float health = 100;

    public Image healthBarFill;

    private void Update()
    {
        healthBarFill.fillAmount = health / 100;
    }
}
