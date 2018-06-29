using System.Collections;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public CanvasGroup fader;

    private SceneLoader loader;
    private int index;
    private bool running;

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        loader = FindObjectOfType<SceneLoader>();
        StartCoroutine(Fade(0, false));
    }

    public void StartGame(int buildIndex)
    {
        index = buildIndex;
        if (!running)
            StartCoroutine(Fade(1, true));
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    private IEnumerator Fade(float alpha, bool starting)
    {
        running = true;
        while (!Mathf.Approximately(fader.alpha, alpha))
        {
            fader.alpha = Mathf.MoveTowards(fader.alpha, alpha, Time.deltaTime);
            yield return null;
        }
        if (starting)
        {
            loader.data.playerHealth = 100;
            loader.LoadScene(index);
        }
        running = false;
    }
}