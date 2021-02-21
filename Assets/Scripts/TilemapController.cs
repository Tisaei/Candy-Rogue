using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapController : MonoBehaviour
{
    private Tilemap Tilemap;

    [SerializeField]
    private TileBase FloorTile;
    [SerializeField]
    private TileBase StairTile;
    [SerializeField]
    private TileBase WallTile;

    [SerializeField]
    private int mapWidth;
    [SerializeField]
    private int mapHeight;

    private Array2D mapData;
    // Start is called before the first frame update
    void Start()
    {
        Tilemap = GetComponent<Tilemap>();
        Tilemap.ClearAllTiles();
        mapData = new Array2D(mapWidth, mapHeight);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GnerateArray()
    {
        // ローグライクのダンジョンマップを自動生成する.
        for (int x = 0; x < mapData.GetWidth(); x++)
        {
            for (int y = 0; y < mapData.GetHeight(); y++)
            {
                mapData.Set(x, y, 0);
            }
        }
    }

    public void LoadArray(int[,] loadData)
    {
        // セーブデータからダンジョンマップをロードする.
        for (int x = 0; x < mapData.GetWidth(); x++)
        {
            for (int y = 0; y < mapData.GetHeight(); y++)
            {
                mapData.Set(x, y, loadData[x, y]);
            }
        }
    }

    public void RenderMap()
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
        if (x >= map.GetUpperBound(0) || y >= map.GetUpperBound(1) || x < 0 || y < 0)
        {
            return eMapGimmick.Null;
        }
        return map[x, y];
    }
    public int Set(int x, int y, int v)
    {
        if (x >= map.GetUpperBound(0) || y >= map.GetUpperBound(1) || x < 0 || y < 0)
        {
            return -1;
        }
        switch (v)
        {
            case 0:
                map[x, y] = eMapGimmick.Wall;
                break;
            case 1:
                map[x, y] = eMapGimmick.Floor;
                break;
            case 2:
                map[x, y] = eMapGimmick.Stair;
                break;
            default:
                map[x, y] = eMapGimmick.Null;
                break;
        }
        return 0;
    }
    public int GetWidth()
    {
        return map.GetUpperBound(0);
    }
    public int GetHeight()
    {
        return map.GetUpperBound(1);
    }
}
