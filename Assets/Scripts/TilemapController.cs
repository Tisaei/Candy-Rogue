using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using CandyRogueBase;

public class TilemapController : MonoBehaviour
{
    private Tilemap Tilemap;
    private Transform TilemapTransform;
    private List<EnemyController> enemyControllers;
    [SerializeField]
    private GameObject Player;
    private PlayerController playerController;

    [SerializeField]
    private TileBase FloorTile, StairTile, WallTile;

    [SerializeField]
    private int mapWidth = 25, mapHeight = 25;

    [SerializeField]
    private int charactorDots = 16;

    private Array2D mapData;

    // Start is called before the first frame update
    private void Start()
    {
        Tilemap = GetComponent<Tilemap>();
        Tilemap.ClearAllTiles();
        TilemapTransform = GetComponent<Transform>();
        playerController = Player.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    private void Update()
    {
        
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

    public void RefCopyEnemyControllers(List<EnemyController> enemyControllers) => this.enemyControllers = enemyControllers;
    public (ActorController collideActor, Vec2D newVec, Pos2D newPos) CoordinateMoveTo(Pos2D position, Vec2D vec, bool onlyWall = false)
    {
        // positionからvec分移動したのちの座標を返す.
        // onlyWallがfalseの時，もし別のActorにぶつかったのならその手前の座標と，ぶつかったActorを返す.
        // もし壁にぶつかったのならその手前の座標を返し，collideActorはnullを返す.
        float nowX = position.x;
        float nowY = position.y;
        var (dirX, dirY) = Vec2D.ToUnitPos2D(vec.dir);
        int nLoop = (int)vec.len;
        ActorController collideActor = null;
        int i;
        for(i = 0; i < nLoop; i++)
        {
            if (isCollideWall((int)Math.Round(nowX + dirX), (int)Math.Round(nowY + dirY))) break;
            if (!onlyWall)
            {
                collideActor = CollideActor((int)Math.Round(nowX + dirX), (int)Math.Round(nowY + dirY));
                if (collideActor != null) break;
            }
            nowX += dirX;
            nowY += dirY;
        }
        return (
            collideActor,
            new Vec2D(vec.dir, (eLen)Enum.ToObject(typeof(eLen), i)),
            new Pos2D((int)Math.Round(nowX), (int)Math.Round(nowY))
        );
    }
    public bool isCollideWall(int xgrid, int ygrid) // 指定の座標が壁かどうかをチェック.
    {
        var gimmick = mapData.Get(xgrid, ygrid);
        return (gimmick == eMapGimmick.Wall || gimmick == eMapGimmick.Null);
    }
    private ActorController CollideActor(int xgrid, int ygrid) // 指定の座標に敵orプレイヤーがいるかどうかをチェック.
    {
        Pos2D playerc = playerController.GetNowPosGrid();
        if (playerc.x == xgrid && playerc.y == ygrid) return playerController;
        foreach(EnemyController ec in enemyControllers)
        {
            Pos2D enemyc = ec.GetNowPosGrid();
            if (enemyc.x == xgrid && enemyc.y == ygrid) return ec;
        }
        return null;
    }

    public float ToWorldX(int xgrid)
    {
        return charactorDots * xgrid + charactorDots / 2 + TilemapTransform.position.x;
    }
    public float ToWorldY(int ygrid)
    {
        return charactorDots * ygrid + charactorDots / 2 + TilemapTransform.position.y;
    }
    public int ToGridX(float xworld)
    {
        return ((int)xworld - (int)TilemapTransform.position.x - charactorDots / 2) / charactorDots;
    }
    public int ToGridY(float yworld)
    {
        return ((int)yworld - (int)TilemapTransform.position.y - charactorDots / 2) / charactorDots;
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
