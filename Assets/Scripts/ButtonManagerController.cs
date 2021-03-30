using System;
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
            else if (Input.GetKey(KeyCode.A)) // 左移動.
            {
                Vec2D toVec;
                (toVec.dir, toVec.len) = (eDir.Left, eLen.One);
                sequenceManagerController.DoOneTurn(new Behavior(true, toVec)).Forget();
            }
            else if (Input.GetKey(KeyCode.W)) // 上移動.
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
                if (!Input.GetKey(KeyCode.LeftShift)) // 下移動.
                {
                    Vec2D toVec;
                    (toVec.dir, toVec.len) = (eDir.Down, eLen.One);
                    sequenceManagerController.DoOneTurn(new Behavior(true, toVec)).Forget();
                }
            }
            else if (Input.GetKey(KeyCode.D)) // 右移動.
            {
                Vec2D toVec;
                (toVec.dir, toVec.len) = (eDir.Right, eLen.One);
                sequenceManagerController.DoOneTurn(new Behavior(true, toVec)).Forget();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                sequenceManagerController.DoOneTurn(new Behavior(false, null, eAct.NoMove)).Forget();
            }
            else if (Input.GetKeyDown(KeyCode.L)) // ロード.
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    dataController.CustomLoad();

                    // ここから(最終的にLoad関数内で位置を決める).
                    enemyControllers = new List<EnemyController>();
                    enemyControllers.AddRange(Array.ConvertAll(GameObject.FindGameObjectsWithTag("Enemy"), g => g.GetComponent<EnemyController>()));
                    int i = 0;
                    foreach(EnemyController ec in enemyControllers)
                    {
                        ec.SetStatus(new Pos2D(10, 14 + i), eDir.Up);
                        i++;
                    }
                    // ここまで.
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
