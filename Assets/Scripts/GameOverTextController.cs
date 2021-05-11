using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverTextController : MonoBehaviour
{
    private Text gameOverText;
    // Start is called before the first frame update
    void Start()
    {
        gameOverText = GetComponent<Text>();
        HideGameOver();
    }

    public void HideGameOver()
    {
        gameOverText.text = "";
    }

    public void ShowGameOver()
    {
        gameOverText.text = "GAME OVER";
    }
}
