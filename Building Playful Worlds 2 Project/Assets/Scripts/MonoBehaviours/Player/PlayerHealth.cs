using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour {

    [Range(0, 100)]
    public float health = 100;
    public GameObject hitParticles;

    public Image healthBarFill;

    public void TakeDamage(float damage, Vector3 hitPoint)
    {
        health -= damage;
        healthBarFill.fillAmount = health / 100;
        if (hitParticles)
            Instantiate(hitParticles, hitPoint, Quaternion.identity);
        ShakeScreen(damage, damage * 2);
    }

    private IEnumerator ShakeScreen(float duration, float intensity)
    {
        Vector3 originalPosition = Camera.main.transform.position;
        for (float i = 0; i < duration * 10; i++)
        {
            Camera.main.transform.position = Camera.main.transform.position + Random.insideUnitSphere * intensity / 10;
            intensity /= 1.1f;
            yield return null;
        }
        Camera.main.transform.position = originalPosition;
    }
}
