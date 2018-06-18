using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public sealed class LogTextDirect : MonoBehaviour {

    public static LogTextDirect logText;

    [SerializeField] Text text;

    private void Start()
    {
        //Make sure there is only one LogTextDirect instance
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
        logText.StartCoroutine(Log(text));
    }

    private IEnumerator Log(string t)
    {
        logText.text.text = t;
        yield return new WaitForSeconds(t.Length / 15);
        logText.text.text = "";
    }
}
