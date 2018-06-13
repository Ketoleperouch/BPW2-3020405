using System.Collections;
using UnityEngine;

public class FadeInOnLoad : MonoBehaviour {

    public CanvasGroup fader { get { return GetComponent<CanvasGroup>(); } }

    private IEnumerator Start()
    {
        while (!Mathf.Approximately(fader.alpha, 0))
        {
            fader.alpha = Mathf.MoveTowards(fader.alpha, 0, Time.deltaTime * 0.1f);
            yield return null;
        }
        Destroy(this);
    }
}
