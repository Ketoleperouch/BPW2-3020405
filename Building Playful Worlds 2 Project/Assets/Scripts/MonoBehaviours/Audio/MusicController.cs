using UnityEngine;
using UnityEngine.Audio;

[DisallowMultipleComponent]
public class MusicController : MonoBehaviour {

    public static MusicController music;

    public AudioMixerSnapshot ambienceSnapshot;
    public AudioMixerSnapshot dangerSnapshot;

    public float transitionDuration = 2f;
    public bool transition = false;

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
            dangerSnapshot.TransitionTo(transitionDuration);
        }
    }
}
