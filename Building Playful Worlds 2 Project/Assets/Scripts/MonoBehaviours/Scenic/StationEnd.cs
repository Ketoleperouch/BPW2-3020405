using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StationEnd : MonoBehaviour {

    public GameObject musicObject;
    public ParticleSystem explosion;
    public AudioSource explosionSound;
    public AudioSource endMusic;
    public CanvasGroup fader;
    public Image healthFill;

    private bool activated = false;

    private void OnTriggerStay(Collider player)
    {
        if (player.CompareTag("Player") && Input.GetKeyDown(KeyCode.E) && !activated)
        {
            if (CheckIfThereAreAnyEnemiesAround())
            {
                LogTextDirect.logText.LogText("There are still enemies around! Finish them off first!");
            }
            else
            {
                MissionController.mission.NextMission();
                activated = true;
            }
        }
    }

    private bool CheckIfThereAreAnyEnemiesAround()
    {
        //:)
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].stats.health > 0)
            {
                return true;
            }
        }
        return false;
    }

    public void Ending()
    {
        Destroy(musicObject);
        explosion.Play();
        explosionSound.Play();
        endMusic.Play();
        Ragdolify player = FindObjectOfType<Ragdolify>();
        player.GetComponent<PlayerController>().dead = true;
        Destroy(player.GetComponent<CapsuleCollider>());
        Destroy(player.GetComponent<Footsteps>());
        Destroy(player.GetComponent<PlayerIK>());
        Destroy(player.GetComponent<Animator>());
        Destroy(GetComponent<MeshRenderer>());
        healthFill.color = Color.clear;
        player.MakeRagdoll();
        FindObjectOfType<PlayerHealth>().GetComponent<Rigidbody>().AddExplosionForce(1000, transform.position, 50);
        StartCoroutine(SlowFadeToBlack());
    }

    private IEnumerator SlowFadeToBlack()
    {
        yield return new WaitForSeconds(7);
        while (!Mathf.Approximately(fader.alpha, 1))
        {
            fader.alpha = Mathf.MoveTowards(fader.alpha, 1, Time.deltaTime * 0.05f);
            yield return null;
        }
        FindObjectOfType<MissionController>().LoadScene(7);
    }
}
