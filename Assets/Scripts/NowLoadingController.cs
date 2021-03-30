using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NowLoadingController : MonoBehaviour
{
    private Text text;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        text.text = "";
    }

    public IEnumerator NowLoadingAnimation()
    {
        text.text = "";
        yield return new WaitForSeconds(0.2f);
        while (true)
        {
            text.text = "Now Loading";
            yield return new WaitForSeconds(0.2f);
            text.text = "Now Loading.";
            yield return new WaitForSeconds(0.2f);
            text.text = "Now Loading..";
            yield return new WaitForSeconds(0.2f);
            text.text = "Now Loading...";
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void DeleteText()
    {
        text.text = "";
    }
}
