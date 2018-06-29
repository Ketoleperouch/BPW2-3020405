using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public sealed class LogMissionTextDirect : MonoBehaviour {

    public static LogMissionTextDirect logText;

    [SerializeField] Text text;

    private void Start()
    {
        //Make sure there is only one LogMissionTextDirect instance
        if (logText)
        {
            Destroy(this);
            return;
        }
        logText = this;
        logText.text = logText.GetComponent<Text>();
    }

    public void LogText(string text)
    {
        StopAllCoroutines();
        logText.StartCoroutine(Log(text));
    }

    private IEnumerator Log(string t)
    {
        logText.text.text = t;
        yield return new WaitForSeconds(t.Length / 8);
        logText.text.text = "";
    }
}
