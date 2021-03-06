﻿using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using CandyRogueBase;

public class ButtonManagerController : MonoBehaviour
{
    [SerializeField]
    private GameObject TileMap, Player, SequenceManager, Data, NowLoading;

    private TilemapController tilemapController;
    private PlayerController playerController;
    private SequenceManagerController sequenceManagerController;
    private DataController dataController;
    private NowLoadingController nowLoadingController;
    private List<EnemyController> enemyControllers;

    // Start is called before the first frame update
    void Start()
    {
        tilemapController = TileMap.GetComponent<TilemapController>();
        playerController = Player.GetComponent<PlayerController>();
        sequenceManagerController = SequenceManager.GetComponent<SequenceManagerController>();
        dataController = Data.GetComponent<DataController>();
        nowLoadingController = NowLoading.GetComponent<NowLoadingController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (sequenceManagerController.status == SequenceManagerController.Status.KEY_INPUT)
        {
            if (Input.GetKeyDown(KeyCode.G)) // マップ生成.
            {
                tilemapController.GnerateArray();
            }
            else if (Input.GetKey(KeyCode.A) && playerController.GetNowHp() != 0) // 左移動.
            {
                Vec2D toVec;
                (toVec.dir, toVec.len) = (eDir.Left, eLen.One);
                sequenceManagerController.DoOneTurn(new Behavior(true, toVec)).Forget();
            }
            else if (Input.GetKey(KeyCode.W) && playerController.GetNowHp() != 0) // 上移動.
            {
                Vec2D toVec;
                (toVec.dir, toVec.len) = (eDir.Up, eLen.One);
                sequenceManagerController.DoOneTurn(new Behavior(true, toVec)).Forget();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                if (Input.GetKey(KeyCode.LeftShift)) dataController.Save(); // セーブ.
            }
            else if (Input.GetKey(KeyCode.S))
            {
                if (!Input.GetKey(KeyCode.LeftShift) && playerController.GetNowHp() != 0) // 下移動.
                {
                    Vec2D toVec;
                    (toVec.dir, toVec.len) = (eDir.Down, eLen.One);
                    sequenceManagerController.DoOneTurn(new Behavior(true, toVec)).Forget();
                }
            }
            else if (Input.GetKey(KeyCode.D) && playerController.GetNowHp() != 0) // 右移動.
            {
                Vec2D toVec;
                (toVec.dir, toVec.len) = (eDir.Right, eLen.One);
                sequenceManagerController.DoOneTurn(new Behavior(true, toVec)).Forget();
            }
            else if (Input.GetKeyDown(KeyCode.Space) && playerController.GetNowHp() != 0)
            {
                sequenceManagerController.DoOneTurn(new Behavior(false, null, eAct.NoAct)).Forget();
            }
            else if (Input.GetKeyDown(KeyCode.L)) // ロード.
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    dataController.Load(true);
                }
                else
                {
                    dataController.Load();
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            StartCoroutine(TestNowLoading());
        }
    }

    private IEnumerator TestNowLoading()
    {
        var nl = StartCoroutine(nowLoadingController.NowLoadingAnimation());
        yield return new WaitForSeconds(3.0f);
        StopCoroutine(nl);
        nowLoadingController.DeleteText();
    }
}
