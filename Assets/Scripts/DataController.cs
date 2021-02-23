using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class DataController : MonoBehaviour
{
    [SerializeField]
    private GameObject TileMap;
    private TilemapController TilemapC;

    [System.Serializable]
    public class Data
    {
        public dataX[] mapDataX;
        [System.Serializable]
        public struct dataX
        {
            public eMapGimmick[] dataY;
        }
    }
    private Data SaveData = new Data();

    // Start is called before the first frame update
    void Start()
    {
        TilemapC = TileMap.GetComponent<TilemapController>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.L))
        {
            Data data = Load();
            eMapGimmick[,] mapData = new eMapGimmick[data.mapDataX.Length, data.mapDataX[0].dataY.Length];
            for (int x = 0; x < mapData.GetLength(0); x++)
            {
                for (int y = 0; y < mapData.GetLength(1); y++)
                {
                    mapData[x, y] = data.mapDataX[x].dataY[y];
                }
            }
            TilemapC.LoadArray(mapData);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Array2D data = TilemapC.GetArray2D();
            SaveData.mapDataX = new Data.dataX[data.GetWidth()];
            for(int x = 0; x < data.GetWidth(); x++)
            {
                SaveData.mapDataX[x].dataY = new eMapGimmick[data.GetHeight()];
                for (int y = 0; y < data.GetHeight(); y++)
                {
                    SaveData.mapDataX[x].dataY[y] = data.Get(x, y);
                }
            }
            Save();
            Debug.Log("Saved");
        }
    }

    private void Save()
    {
        string jsonstr = JsonUtility.ToJson(SaveData);
        Debug.Log("Saving\n"+jsonstr);
        var writer = new StreamWriter(Application.dataPath + "/save.json", false);
        writer.Write(jsonstr);
        writer.Flush();
        writer.Close();
    }
    private Data Load()
    {
        StreamReader reader = new StreamReader(Application.dataPath + "/save.json");
        string datastr = reader.ReadToEnd();
        reader.Close();
        Data data = JsonUtility.FromJson<Data>(datastr);
        return data;
    }
}
