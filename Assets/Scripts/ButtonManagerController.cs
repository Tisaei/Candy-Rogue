using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManagerController : MonoBehaviour
{
    [SerializeField]
    private GameObject TileMap;
    private TilemapController tilemapController;
    [SerializeField]
    private GameObject Player;
    private PlayerController playerController;
    [SerializeField]
    private GameObject Data;
    private DataController dataController;

    // Start is called before the first frame update
    void Start()
    {
        tilemapController = TileMap.GetComponent<TilemapController>();
        playerController = Player.GetComponent<PlayerController>();
        dataController = Data.GetComponent<DataController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) // マップ生成.
        {
            tilemapController.GnerateArray();
        }
        else if (Input.GetKey(KeyCode.A) && !playerController.isMoving) // 左移動.
        {
            Vec2D toVec;
            (toVec.dir, toVec.len) = (eDir.Left, eLen.One);
            playerController.SetMove(toVec);
        }
        else if (Input.GetKey(KeyCode.W) && !playerController.isMoving) // 上移動.
        {
            Vec2D toVec;
            (toVec.dir, toVec.len) = (eDir.Up, eLen.One);
            playerController.SetMove(toVec);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            if (Input.GetKey(KeyCode.LeftShift)) // セーブ.
            {
                dataController.Save();
            }
            else if (!playerController.isMoving) // 下移動.
            {
                Vec2D toVec;
                (toVec.dir, toVec.len) = (eDir.Down, eLen.One);
                playerController.SetMove(toVec);
            }
        }
        else if (Input.GetKey(KeyCode.D) && !playerController.isMoving) // 右移動.
        {
            Vec2D toVec;
            (toVec.dir, toVec.len) = (eDir.Right, eLen.One);
            playerController.SetMove(toVec);
        }
        else if (Input.GetKeyDown(KeyCode.L)) // ロード.
        {
            dataController.Load();
        }
    }
}
