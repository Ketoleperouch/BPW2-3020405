using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class PlayerHealth : MonoBehaviour {

    [Range(0, 100)]
    public float health = 100;
    public GameObject hitParticles;

    public Image healthBarFill;
    public CanvasGroup fader;

    private Ragdolify ragdolify; //For deaths

    private void Start()
    {
        ragdolify = GetComponent<Ragdolify>();
        //Return Data from Checkpoint Data Asset
        SceneLoader sL = FindObjectOfType<SceneLoader>();
        if (sL)
            health = sL.data.playerHealth;
    }

    public void TakeDamage(float damage, Vector3 hitPoint)
    {
        health -= damage;
        healthBarFill.fillAmount = health / 100;
        if (hitParticles)
            Instantiate(hitParticles, hitPoint, Quaternion.identity);
        if (damage > 10)
        {
            damage /= 2;
            StartCoroutine(ShakeScreen(damage, damage * 2));
        }
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GetComponent<PlayerController>().dead = true;
        Destroy(GetComponent<CapsuleCollider>());
        Destroy(GetComponent<Footsteps>());
        Destroy(GetComponent<PlayerIK>());
        Destroy(GetComponent<Animator>());
        ragdolify.MakeRagdoll();
        StartCoroutine(DeathReset());
    }

    private IEnumerator ShakeScreen(float duration, float intensity)
    {
        Vector3 originalPosition = Camera.main.transform.parent.position;
        for (float i = 0; i < duration * 10; i++)
        {
            Camera.main.transform.parent.position = Camera.main.transform.parent.position + Random.insideUnitSphere * intensity / 10;
            intensity /= 1.1f;
            yield return null;
        }
        Camera.main.transform.parent.position = originalPosition;
    }

    private IEnumerator DeathReset()
    {
        SceneLoader loader = FindObjectOfType<SceneLoader>();
        while (!Mathf.Approximately(fader.alpha, 1))
        {
            fader.alpha = Mathf.MoveTowards(fader.alpha, 1, Time.deltaTime);
            yield return null;
        }
        loader.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}