using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {

    public GameObject loader;
    public GameObject cam;
    public GameObject startLoader;
    public CheckpointData data;

    [SerializeField] bool initialize = false;

    private bool executing = false;

    private void Start()
    {
        if (initialize)
            StartCoroutine(Load(1));
        if (!data)
        {
            Debug.Log("New Data Instance created");
            CheckpointData newCheckpointData = ScriptableObject.CreateInstance<CheckpointData>();
            newCheckpointData.name = "Local " + System.DateTime.Now;
            data = newCheckpointData;
            if (initialize)
            {
                data.playerHealth = 100;
            }
        }
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

        //Load data from Checkpoint Data to Player.
        PlayerHealth pData = FindObjectOfType<PlayerHealth>();
        if (pData)
        {
            pData.health = data.playerHealth;
            pData.healthBarFill.fillAmount = pData.health / 100;
        }
    }
}
