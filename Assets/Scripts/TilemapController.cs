using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapController : MonoBehaviour
{
    private Tilemap Tilemap;
    private Transform TilemapTransform;

    [SerializeField]
    private TileBase FloorTile;
    [SerializeField]
    private TileBase StairTile;
    [SerializeField]
    private TileBase WallTile;

    [SerializeField]
    private int mapWidth = 25;
    [SerializeField]
    private int mapHeight = 25;

    [SerializeField]
    private int charactorDots = 16;

    private Array2D mapData;

    // Start is called before the first frame update
    void Start()
    {
        Tilemap = GetComponent<Tilemap>();
        Tilemap.ClearAllTiles();
        TilemapTransform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GnerateArray();
        }
    }

    public void GnerateArray()
    {
        mapData = new Array2D(mapWidth, mapHeight);
        // ローグライクのダンジョンマップを自動生成する.
        bool placedStair = false;
        for (int x = 0; x < mapData.GetWidth(); x++)
        {
            for (int y = 0; y < mapData.GetHeight(); y++)
            {
                eMapGimmick v;
                if (placedStair)
                {
                    v = (eMapGimmick)Enum.ToObject(typeof(eMapGimmick), UnityEngine.Random.Range(0, 2));
                }
                else
                {
                    v = (eMapGimmick)Enum.ToObject(typeof(eMapGimmick), UnityEngine.Random.Range(0, 3));
                }
                if (v == eMapGimmick.Stair)
                {
                    placedStair = true;
                }
                mapData.Set(x, y, v);
            }
        }
        RenderMap();
        Debug.Log("Generatd");
    }

    public void LoadArray(eMapGimmick[,] loadData)
    {
        mapData = new Array2D(loadData.GetLength(0), loadData.GetLength(1));
        // セーブデータからダンジョンマップをロードする.
        for (int x = 0; x < mapData.GetWidth(); x++)
        {
            for (int y = 0; y < mapData.GetHeight(); y++)
            {
                mapData.Set(x, y, loadData[x, y]);
            }
        }
        RenderMap();
        Debug.Log("Loaded");
    }

    public Array2D GetArray2D()
    {
        return mapData;
    }

    private void RenderMap()
    {
        // ダンジョンマップからTilemapを生成する.

        // マップの幅の分、周回する.
        for (int x = 0; x < mapData.GetWidth(); x++)
        {
            // マップの高さの分、周回する.
            for (int y = 0; y < mapData.GetHeight(); y++)
            {
                switch(mapData.Get(x, y))
                {
                    case eMapGimmick.Floor:
                        Tilemap.SetTile(new Vector3Int(x, y, 0), FloorTile);
                        break;
                    case eMapGimmick.Stair:
                        Tilemap.SetTile(new Vector3Int(x, y, 0), StairTile);
                        break;
                    default:
                        Tilemap.SetTile(new Vector3Int(x, y, 0), WallTile);
                        break;
                }
            }
        }
    }


    public Pos2D CoordinateMoveTo(Pos2D position, Vec2D vec)
    {
        // positionからvec分移動したのちの座標を返す.
        float nowX = (float)position.x;
        float nowY = (float)position.y;
        float dirX;
        float dirY;
        int nLoop;
        switch (vec.dir)
        {
            case eDir.Left: //左
                dirX = -1f;
                dirY = 0f;
                break;
            case eDir.Up:   //上
                dirX = 0f;
                dirY = -1f;
                break;
            case eDir.Right://右
                dirX = 1f;
                dirY = 0f;
                break;
            default:        //下
                dirX = 0f;
                dirY = 1f;
                break;
        }
        nLoop = (int)vec.len;
        for(int i = 0; i < nLoop; i++)
        {
            var gimmick = mapData.Get((int)Math.Round(nowX + dirX), (int)Math.Round(nowY + dirY));
            if (gimmick == eMapGimmick.Wall || gimmick == eMapGimmick.Null) break;
            nowX += dirX;
            nowY += dirY;
        }
        return new Pos2D((int)Math.Round(nowX), (int)Math.Round(nowY));
    }

    int ToWorldX(int xgrid)
    {
        return charactorDots * xgrid + charactorDots / 2 + (int)TilemapTransform.position.x;
    }
    int ToWorldY(int ygrid)
    {
        return charactorDots * ygrid + charactorDots / 2 + (int)TilemapTransform.position.y;
    }
    int ToGridX(int xworld)
    {
        return (xworld - (int)TilemapTransform.position.x - charactorDots / 2) / charactorDots;
    }
    int ToGridY(int yworld)
    {
        return (yworld - (int)TilemapTransform.position.y - charactorDots / 2) / charactorDots;
    }
    bool IsCollide(Pos2D position) // 指定の座標が壁かどうかをチェック.
    {
        var gimmick = mapData.Get(position.x, position.y);
        return (gimmick == eMapGimmick.Wall || gimmick == eMapGimmick.Null);
    }
}

public class Array2D
{
    private eMapGimmick[,] map;
    public Array2D(int width, int height)
    {
        map = new eMapGimmick[width, height];
    }
    public eMapGimmick Get(int x, int y)
    {
        if (x >= map.GetLength(0) || y >= map.GetLength(1) || x < 0 || y < 0)
        {
            return eMapGimmick.Null;
        }
        return map[x, y];
    }
    public int Set(int x, int y, eMapGimmick v)
    {
        if (x >= map.GetLength(0) || y >= map.GetLength(1) || x < 0 || y < 0)
        {
            return -1;
        }
        map[x, y] = v;
        return 0;
    }
    public int GetWidth()
    {
        return map.GetLength(0);
    }
    public int GetHeight()
    {
        return map.GetLength(1);
    }
}
