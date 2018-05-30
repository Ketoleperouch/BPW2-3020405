using UnityEngine;
using UnityEngine.Audio;

public class MusicController : MonoBehaviour {

    public AudioMixerSnapshot ambienceSnapshot;
    public AudioMixerSnapshot dangerSnapshot;

    public float transitionDuration = 2f;
    public bool transition = false;

    private void Update()
    {
        if (transition)
        {
            transition = false;
            dangerSnapshot.TransitionTo(transitionDuration);
        }
    }
}
