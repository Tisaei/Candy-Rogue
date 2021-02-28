using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class DataController : MonoBehaviour
{
    [SerializeField]
    private GameObject TileMap;
    private TilemapController tilemapController;
    [SerializeField]
    private GameObject Player;
    private PlayerController playerController;

    [System.Serializable]
    public class Data
    {
        public dataY[] mapDataY;
        [System.Serializable]
        public struct dataY
        {
            public eMapGimmick[] dataX;
        }
        public Pos2D playerPos;
        public eDir playerDir;
    }
    private Data SaveData = new Data();

    // Start is called before the first frame update
    private void Start()
    {
        tilemapController = TileMap.GetComponent<TilemapController>();
        playerController = Player.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    public void Save()
    {
        Array2D data = tilemapController.GetArray2D();
        SaveData.mapDataY = new Data.dataY[data.GetHeight()];
        int height = data.GetHeight();
        for (int y = 0; y < height; y++)
        {
            SaveData.mapDataY[y].dataX = new eMapGimmick[data.GetWidth()];
            for (int x = 0; x < data.GetWidth(); x++)
            {
                SaveData.mapDataY[y].dataX[x] = data.Get(x, height - 1 - y);
            }
        }

        var (playerPos, playerDir) = playerController.GetStatus();
        SaveData.playerPos = playerPos;
        SaveData.playerDir = playerDir;


        string jsonstr = JsonUtility.ToJson(SaveData);
        var writer = new StreamWriter(Application.dataPath + "/save.json", false);
        writer.Write(jsonstr);
        writer.Flush();
        writer.Close();
        Debug.Log("Saved");
    }
    public void Load()
    {
        StreamReader reader = new StreamReader(Application.dataPath + "/save.json");
        string datastr = reader.ReadToEnd();
        reader.Close();
        Data data = JsonUtility.FromJson<Data>(datastr);


        eMapGimmick[,] mapData = new eMapGimmick[data.mapDataY[0].dataX.Length, data.mapDataY.Length];
        int height = mapData.GetLength(1);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < mapData.GetLength(0); x++)
            {
                mapData[x, height - 1 - y] = data.mapDataY[y].dataX[x];
            }
        }
        tilemapController.LoadArray(mapData);

        playerController.SetStatus(data.playerPos, data.playerDir);
    }
}
