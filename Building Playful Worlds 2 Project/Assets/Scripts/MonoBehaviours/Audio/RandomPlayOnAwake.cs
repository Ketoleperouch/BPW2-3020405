using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomPlayOnAwake : MonoBehaviour {

    public AudioClip[] clips;

    private AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();

        if (!source)
        {
            Destroy(this);
        }
        else
        {
            source.playOnAwake = false;
            source.clip = clips[Random.Range(0, clips.Length)];
            source.Play();
        }
    }
}
