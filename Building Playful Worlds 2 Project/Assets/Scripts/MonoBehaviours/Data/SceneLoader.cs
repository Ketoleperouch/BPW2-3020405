using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {

    public GameObject loader;
    public GameObject cam;
    public GameObject startLoader;

    [SerializeField] bool initialize = false;

    private bool executing = false;

    private void Start()
    {
        if (initialize || SceneManager.sceneCount == 1)
            initialize = true;
            StartCoroutine(Load(1));
    }

    public void LoadScene(int buildIndex)
    {
        if (!executing)
            StartCoroutine(Load(buildIndex));
    }

    private IEnumerator Load(int index)
    {
        executing = true;
        cam.SetActive(true);
        if (initialize)
        {
            startLoader.SetActive(true);
            yield return new WaitForSeconds(2);
        }
        else
        {
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            loader.SetActive(true);
        }
        yield return SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
        Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        SceneManager.SetActiveScene(newScene);
        if (initialize)
        {
            initialize = false;
            startLoader.SetActive(false);
        }
        else
        {
            loader.SetActive(false);
        }
        cam.SetActive(false);
        executing = false;
    }
}
