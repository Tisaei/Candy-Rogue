using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using CandyRogueBase;

public class DataController : MonoBehaviour
{
    [SerializeField]
    private GameObject TileMap, Player, SequenceManager, Canvas;

    private TilemapController tilemapController;
    private PlayerController playerController;
    private SequenceManagerController sequenceManagerController;

    [System.Serializable]
    public class Data
    {
        public dataY[] mapDataY;
        [System.Serializable]
        public struct dataY
        {
            public eMapGimmick[] dataX;
        }

        public actorData[] actorDatas;
        [System.Serializable]
        public struct actorData
        {
            public int id;
            public Pos2D pos;
            public eDir dir;
            public int hp;
            public int atk;
            public int def;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        tilemapController = TileMap.GetComponent<TilemapController>();
        playerController = Player.GetComponent<PlayerController>();
        sequenceManagerController = SequenceManager.GetComponent<SequenceManagerController>();
    }

    // Update is called once per frame
    private void Update()
    {

    }

    public void Save()
    {
        Data SaveData = new Data();

        // マップ情報.
        Array2D data = tilemapController.GetArray2D();
        SaveData.mapDataY = new Data.dataY[data.GetHeight()];
        int height = data.GetHeight();
        for (int y = 0; y < height; y++)
        {
            SaveData.mapDataY[y].dataX = new eMapGimmick[data.GetWidth()];
            for (int x = 0; x < data.GetWidth(); x++) SaveData.mapDataY[y].dataX[x] = data.Get(x, height - 1 - y);
        }

        // アクター情報.
        List<EnemyController> enemyList = sequenceManagerController.GetEnemyControllerList();
        SaveData.actorDatas = new Data.actorData[enemyList.Count + 1];
        // プレイヤー情報.
        SaveData.actorDatas[0].id = playerController.actorData.id;
        (SaveData.actorDatas[0].pos, SaveData.actorDatas[0].dir, SaveData.actorDatas[0].hp, SaveData.actorDatas[0].atk, SaveData.actorDatas[0].def) = playerController.GetStatus();
        // 敵情報.
        for (int i = 1; i <= enemyList.Count; i++)
        {
            SaveData.actorDatas[i].id = enemyList[i - 1].actorData.id;
            (SaveData.actorDatas[i].pos, SaveData.actorDatas[i].dir, SaveData.actorDatas[i].hp, SaveData.actorDatas[i].atk, SaveData.actorDatas[i].def) = enemyList[i - 1].GetStatus();
        }

        string jsonstr = JsonUtility.ToJson(SaveData);
        var writer = new StreamWriter(Application.dataPath + "/save.json", false);
        writer.Write(jsonstr);
        writer.Flush();
        writer.Close();
        Debug.Log("Saved");
    }
    public void Load(bool isCustom = false)
    {
        StreamReader reader;
        if (isCustom) reader = new StreamReader(Application.dataPath + "/save-custom.json"); else reader = new StreamReader(Application.dataPath + "/save.json");
        string datastr = reader.ReadToEnd();
        reader.Close();
        Data data = JsonUtility.FromJson<Data>(datastr);

        // マップ情報.
        eMapGimmick[,] mapData = new eMapGimmick[data.mapDataY[0].dataX.Length, data.mapDataY.Length];
        int height = mapData.GetLength(1);
        for (int y = 0; y < height; y++) { for (int x = 0; x < mapData.GetLength(0); x++) mapData[x, height - 1 - y] = data.mapDataY[y].dataX[x]; }
        tilemapController.LoadArray(mapData);

        // アクター情報.
        foreach (GameObject e in GameObject.FindGameObjectsWithTag("Enemy")) Destroy(e); // まずEnemyタグのついたオブジェクトをすべて削除.
        GameObject[] enemies = Resources.LoadAll<GameObject>("Prefabs"); // Resourcesフォルダ内のプレファブをすべて取得，プレファブ配列の作成.
        int[] enemiesID = Array.ConvertAll(enemies, e => e.GetComponent<EnemyController>().actorData.id); // プレファブ配列から敵のID配列を抜き出す.
        Debug.Log(string.Join(", ", enemiesID));
        foreach (Data.actorData ad in data.actorDatas) // セーブデータからひとつづつ.
        {
            if (ad.id == 0)
            {// プレイヤー情報.
                playerController.SetStatus(ad.pos, ad.dir, ad.hp, ad.atk, ad.def);
            }
            else
            {//敵情報.
                int index = Array.IndexOf(enemiesID, ad.id); // ID配列におけるインデックスを取得.(プレファブ配列と同じ)
                if (index != -1)
                {
                    GameObject e = Instantiate(enemies[index]); // プレファブのインスタンス化.
                    e.GetComponent<EnemyController>().SetStatus(ad.pos, ad.dir, ad.hp, ad.atk, ad.def); // ステータスのセット.
                    e.transform.SetParent(Canvas.transform); // Canvasオブジェクトの子に指定.
                }
            }
        }
        sequenceManagerController.isResetEnemyControllerList = true;
    }
}
