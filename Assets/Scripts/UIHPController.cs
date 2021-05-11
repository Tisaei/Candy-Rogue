using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHPController : MonoBehaviour
{
    [SerializeField]
    private GameObject Player;
    private PlayerController playerController;
    private Text HPText;
    private Slider HPBar;
    
    void Start()
    {
        HPText = transform.Find("HPText").gameObject.GetComponent<Text>();
        HPBar = transform.Find("HPBar").gameObject.GetComponent<Slider>();
        playerController = Player.GetComponent<PlayerController>();
        HPText.text = $"{0}/{0}";
    }

    void Update()
    {
        int maxHP = playerController.actorData.maxHp;
        int nowHP = playerController.GetNowHp();
        HPBar.maxValue = maxHP;
        HPBar.value = nowHP;
        if(nowHP == 0)
        {
            HPText.color = new Color(1f, 0.1f, 0.1f);
        }
        else
        {
            HPText.color = new Color(1f, 1f, 1f);
        }
        HPText.text = $"{nowHP}/{maxHP}";
    }
}
