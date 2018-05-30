using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CutsceneController : MonoBehaviour {

    [System.Serializable]
    public class CutsceneEvent
    {
        public float timestamp;
        public UnityEvent sceneEvent;
        [HideInInspector] public bool done = false;
    }

    public CanvasGroup fader;
    public CutsceneEvent[] events;
    public Camera[] cameras;
    public Animator animator;
    public Text bigText;
    public Text logText;
    public CutsceneEvent onSkip;
    public AudioSource music;

    private int activeCamera = 0;
    private bool fading = false;
    private SceneLoader sceneLoader;

    private void Start()
    {
        sceneLoader = FindObjectOfType<SceneLoader>();
        if (!sceneLoader)
        {
            Debug.LogWarning("No Scene Loader found! Please add a Scene Loader in any loaded scene.");
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        for (int i = 0; i < events.Length; i++)
        {
            if (Time.timeSinceLevelLoad - 2 >= events[i].timestamp && !events[i].done)
            {
                events[i].done = true;
                events[i].sceneEvent.Invoke();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            onSkip.sceneEvent.Invoke();
        }
    }

    public void FadeToVisible(float speed)
    {
        if (fading)
        {
            StopAllCoroutines();
        }
        StartCoroutine(IFade(0, speed));
    }

    public void FadeToBlack(float speed)
    {
        if (fading)
        {
            StopAllCoroutines();
        }
        StartCoroutine(IFade(1, speed));
    }

    public void EnableCamera(int active)
    {
        activeCamera = active;

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].GetComponent<AudioListener>().enabled = false;
            cameras[i].enabled = false;
        }
        cameras[activeCamera].enabled = true;
        cameras[activeCamera].GetComponent<AudioListener>().enabled = true;
    }

    public void ZoomCamera(float fov)
    {
        StartCoroutine(IZoomCamera(cameras[activeCamera], fov));
    }

    public void ActivateAnimation(string animationName)
    {
        animator.Play(animationName);
    }

    public void DisplayBigText(string text)
    {
        StartCoroutine(IDisplayText(text, true));
    }
    public void DisplaySmallText(string text)
    {
        StartCoroutine(IDisplayText(text, false));
    }

    public void LoadScene(int buildIndex)
    {
        sceneLoader.LoadScene(buildIndex);
    }

    public void FadeOutMusic(float speed)
    {
        StartCoroutine(IFadeOutMusic(speed));
    }

    private IEnumerator IFade(float a, float speed)
    {
        fading = true;
        while (!Mathf.Approximately(fader.alpha, a))
        {
            fader.alpha = Mathf.MoveTowards(fader.alpha, a, Time.deltaTime * speed);
            yield return null;
        }
        fading = false;
    }

    private IEnumerator IZoomCamera(Camera cam, float fov)
    {
        while (!Mathf.Approximately(cam.fieldOfView, fov))
        {
            cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, fov, Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator IDisplayText(string text, bool big)
    {
        string displayMe = "";
        for (int i = 0; i < text.Length; i++)
        {
            displayMe += text[i];
            if (big)
            {
                bigText.text = displayMe;
                yield return new WaitForSeconds(0.05f);
            }
            else
            {
                logText.text = displayMe;
                yield return null;
            }
            
        }
        yield return new WaitForSeconds(text.Length / 15);
        if (big)
            bigText.text = "";
        else
            logText.text = "";
    }

    private IEnumerator IFadeOutMusic(float speed)
    {
        while (music.volume > 0)
        {
            music.volume = Mathf.MoveTowards(music.volume, 0, Time.deltaTime * speed);
            yield return null;
        }
    }
}
