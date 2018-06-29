using UnityEngine;
using UnityEngine.Audio;

[DisallowMultipleComponent]
public class MusicController : MonoBehaviour {

    public static MusicController music;

    public AudioMixerSnapshot ambienceSnapshot;
    public AudioMixerSnapshot dangerSnapshot;
    public AudioSource dangerSource;
    
    public float transitionDuration = 2f;
    public bool transition = false;
    public bool unused = false;             //Optimization in order to be able to leave out MusicController Component availability-checks
    
    public bool hasTransitioned { get; set; }

    private void Start()
    {
        if (!music)
            music = this;
        else
            Destroy(this);
    }

    private void Update()
    {
        if (transition)
        {
            transition = false;
            SetSnapshot(dangerSnapshot);
        }
    }

    public void SetSnapshot(AudioMixerSnapshot snapshot)
    {
        if (unused)
        {
            return;
        }
        if ((hasTransitioned && snapshot == dangerSnapshot) || (!hasTransitioned && snapshot == ambienceSnapshot))
        {
            Debug.LogWarning("Attempting to snapshot which is already active. Aborting.");
            return;
        }
        if (snapshot == dangerSnapshot && !hasTransitioned)
        {
            hasTransitioned = true;
            dangerSource.Play();
        }
        else
        {
            hasTransitioned = false;
            dangerSource.Stop();
        }
        snapshot.TransitionTo(transitionDuration);
        
    }
}
