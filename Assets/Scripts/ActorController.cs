using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class ActorController : MonoBehaviour
{
    [SerializeField]
    private GameObject Tilemap;
    private TilemapController TilemapController;

    [SerializeField]
    private int firstPosGridX = 0;
    [SerializeField]
    private int firstPosGridY = 0;

    private Pos2D nowPosGrid;
    private eDir dir;

    private Vector3 targetPosWorld; //移動先のワールド座標.
    private Vector3 acceleration; //加速度.
    private Vector3 nowVelocity; //現在の移動速度.

    private float moveStartTime; //キャラの移動開始時刻.
    private Vector3 moveStartVelocity; //移動開始速度.

    [SerializeField]
    private bool doConstantVMotion = false; //キャラを等速運動させるか否か.
    [SerializeField]
    private float moveTime = 0.25f; //キャラの1回の移動時間(秒).

    public bool isMoving = false;

    private void Start()
    {
        TilemapController = Tilemap.GetComponent<TilemapController>();
        nowPosGrid = new Pos2D(firstPosGridX, firstPosGridY);
        Vector3 tempPosWorld = Vector3.zero;
        (tempPosWorld.x, tempPosWorld.y) = (TilemapController.ToWorldX(nowPosGrid.x), TilemapController.ToWorldY(nowPosGrid.y));
        transform.position = tempPosWorld;
        dir = eDir.Up;

        targetPosWorld = Vector3.zero;
    }
    private void Update()
    {
        if (isMoving)
        {
            Move();
        }
    }

    public void SetMove(Vec2D toVec)
    {
        var (isCollide, newVec, targetPosGrid) = TilemapController.CoordinateMoveTo(nowPosGrid, toVec);
        (nowPosGrid.x, nowPosGrid.y) = (targetPosGrid.x, targetPosGrid.y);
        dir = newVec.dir;
        (targetPosWorld.x, targetPosWorld.y) = (TilemapController.ToWorldX(targetPosGrid.x), TilemapController.ToWorldX(targetPosGrid.y));
        Vector3 amountMove = targetPosWorld - transform.position;
        if (doConstantVMotion)
        {
            acceleration = Vector3.zero;
            nowVelocity = moveStartVelocity = amountMove / moveTime;
        }
        else
        {
            acceleration = -2f * amountMove / (float)Math.Pow(moveTime, 2);
            nowVelocity = moveStartVelocity = 2f * amountMove / moveTime;
        }
        Debug.Log("MoveStart amountMove:" + amountMove + " nowV:" + nowVelocity);
        moveStartTime = Time.time;
        isMoving = true;
    }

    private void Move()
    {
        Vector3 meanVelocityThisFrame = nowVelocity - acceleration * Time.deltaTime / 2f;
        transform.position = Vector3.MoveTowards(transform.position, targetPosWorld, meanVelocityThisFrame.magnitude * Time.deltaTime);
        float elapsedTime = Time.time - moveStartTime;
        nowVelocity = moveStartVelocity + acceleration * elapsedTime;
        if(elapsedTime >= moveTime)
        {
            transform.position = targetPosWorld;
            Debug.Log("MoveEnd acceleration:"+ acceleration + " nowV:" + nowVelocity + " elapsedTime:" + elapsedTime + " moveStartVelocity:" + moveStartVelocity);
            isMoving = false;
        }
    }

    public (Pos2D actorPos, eDir actorDir) GetStatus()
    {
        return (nowPosGrid, dir);
    }

    public void SetStatus(Pos2D actorPos, eDir actorDir)
    {
        (nowPosGrid.x, nowPosGrid.y) = (actorPos.x, actorPos.y);
        Vector3 nowPosWorld = Vector3.zero;
        (nowPosWorld.x, nowPosWorld.y) = (TilemapController.ToWorldX(actorPos.x), TilemapController.ToWorldY(actorPos.y));
        transform.position = nowPosWorld;
        dir = actorDir;
    }
}
