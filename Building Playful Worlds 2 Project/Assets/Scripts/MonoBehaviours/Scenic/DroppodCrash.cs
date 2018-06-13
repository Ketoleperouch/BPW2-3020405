using System.Collections;
using UnityEngine;

public class DroppodCrash : MonoBehaviour {

    public GameObject impactExplode;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        Instantiate(impactExplode, transform.position, Quaternion.identity);
        StartCoroutine(Fadeout());
    }

    private IEnumerator Fadeout()
    {
        while(!Mathf.Approximately(audioSource.volume, 0))
        {
            audioSource.volume = Mathf.Lerp(audioSource.volume, 0, Time.deltaTime * 10);
            yield return null;
        }
    }

}
