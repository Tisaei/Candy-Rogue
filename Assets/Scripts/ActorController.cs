﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using CandyRogueBase;

public abstract class ActorController : MonoBehaviour
{
    protected TilemapController TilemapController;

    [SerializeField]
    public ActorData actorData;

    [SerializeField]
    private int firstPosGridX = 0, firstPosGridY = 0;

    private Pos2D nowPosGrid;
    public Pos2D GetNowPosGrid() { return nowPosGrid; }
    private eDir dir;
    private int nowHp;
    public int GetNowHp() { return nowHp; }
    private int nowAtk;
    public int GetNowAtk() { return nowAtk; }
    private int nowDef;
    public int GetNowDef() { return nowDef; }

    public override string ToString() { return "Position:(" + nowPosGrid.x.ToString() + "," + nowPosGrid.y.ToString() + ") Direction:" + dir.ToString(); }

    private Vector3 targetPosWorld; //移動先のワールド座標.

    [SerializeField]
    private bool doConstantVMotion = false; //キャラを等速運動させるか否か.
    [SerializeField]
    private float moveTime = 0.25f; //キャラの1回の移動時間(秒).

    public bool isMoving = false;
    public bool isActing = false;

    public enum eTurnState
    {
        KEY_INPUT,

        // 攻撃する，アイテムを使う等，行動する.
        ACT_BEGIN,
        ACTING,

        // 攻撃せずに移動する.
        MOVE_BEGIN,
        MOVING,

        TURN_END
    }

    private eTurnState turnState;

    protected virtual void Awake() // Instantiateの直後ではまだStartは呼び出されていないので.
    {
        TilemapController = GameObject.Find("Tilemap").GetComponent<TilemapController>();
    }
    protected virtual void Start()
    {
        nowPosGrid = new Pos2D(firstPosGridX, firstPosGridY);
        Vector3 tempPosWorld = Vector3.zero;
        (tempPosWorld.x, tempPosWorld.y) = (TilemapController.ToWorldX(nowPosGrid.x), TilemapController.ToWorldY(nowPosGrid.y));
        transform.position = tempPosWorld;
        dir = eDir.Up;
        turnState = eTurnState.KEY_INPUT;
        (nowHp, nowAtk, nowDef) = (actorData.maxHp, actorData.atk, actorData.def);

        targetPosWorld = Vector3.zero;
    }
    private void Update()
    {

    }

    public async UniTask Move(Vec2D toVec, CancellationToken cancellation_token, ActorController collideActor = null)
    {
        if (isMoving) return;
        isMoving = true;
        turnState = eTurnState.MOVING;

        // 移動するベクトルを受け取り，移動先の座標を計算.
        var (dirXf, dirYf) = Vec2D.ToUnitPos2D(toVec.dir);
        Pos2D amountMoveGrid = (int)toVec.len * new Pos2D((int)dirXf, (int)dirYf);
        nowPosGrid += amountMoveGrid;

        // 移動の加速度と初速度を計算.
        dir = toVec.dir;
        (targetPosWorld.x, targetPosWorld.y) = (TilemapController.ToWorldX(nowPosGrid.x), TilemapController.ToWorldX(nowPosGrid.y));
        Vector3 vectorToGoal = targetPosWorld - transform.position;
        Vector3 amountMove = vectorToGoal + (collideActor != null ? new Vector3(dirXf*TilemapController.GetCharactorDots(), dirYf*TilemapController.GetCharactorDots()) : Vector3.zero);
        Vector3 acceleration, moveStartVelocity;
        if (doConstantVMotion)
        {
            acceleration = Vector3.zero;
            moveStartVelocity = amountMove / moveTime;
        }
        else
        {
            acceleration = -2f * amountMove / (float)Math.Pow(moveTime, 2);
            moveStartVelocity = 2f * amountMove / moveTime;
        }
        Debug.Log("MoveStart amountMove:" + amountMove + " startV:" + moveStartVelocity);

        // 移動.
        float moveStartTime = Time.time;
        Vector3 moveStartPosition = transform.position;
        bool isBounded = false;
        float elapsedTime;
        Vector3 nowVelocity;
        do
        {
            elapsedTime = Time.time - moveStartTime;
            nowVelocity = moveStartVelocity + acceleration * elapsedTime;
            Vector3 meanVelocityThisFrame = nowVelocity - acceleration * Time.deltaTime / 2f;
            if(collideActor != null)
            {
                transform.position = transform.position + meanVelocityThisFrame * Time.deltaTime;
                // もしtransform + charDots / 2 まで進んだらmoveStartVelocityとaccelerationを負にする.
                if (!isBounded && (transform.position - moveStartPosition).magnitude >= vectorToGoal.magnitude + TilemapController.GetCharactorDots() / 2)
                {
                    collideActor.Attacked(nowAtk); // 攻撃!!
                    moveStartVelocity = -moveStartVelocity;
                    acceleration = -acceleration;
                    isBounded = true;
                }
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosWorld, meanVelocityThisFrame.magnitude * Time.deltaTime);
            }
            await UniTask.Yield(PlayerLoopTiming.Update, cancellation_token);
        } while (elapsedTime < moveTime);
        transform.position = targetPosWorld;

        // 移動完了.
        Debug.Log("MoveEnd acceleration:" + acceleration + " nowV:" + nowVelocity + " elapsedTime:" + elapsedTime);
        turnState = eTurnState.TURN_END;
        isMoving = false;
    }

    public virtual async UniTask Act(eAct act, CancellationToken cancellation_token)
    {
        if (isActing) return;
        isActing = true;
        turnState = eTurnState.ACTING;

        // 回転準備.
        Vector3 initRotation = transform.rotation.eulerAngles;
        float rotateTime = moveTime * 2f;
        Vector3 angularVelocity = new Vector3(0f, 0f, 360f / rotateTime);

        // 回転.
        float moveStartTime = Time.time;
        float elapsedTime;
        do
        {
            elapsedTime = Time.time - moveStartTime;
            transform.rotation = Quaternion.Euler(initRotation + angularVelocity * elapsedTime);
            await UniTask.Yield(PlayerLoopTiming.Update, cancellation_token);
        } while (elapsedTime < rotateTime);
        transform.rotation = Quaternion.Euler(initRotation);

        // 回転完了.
        turnState = eTurnState.TURN_END;
        isActing = false;
    }

    public virtual void Attacked(int atk)
    {
        int damage = (int)Math.Round(atk * Math.Pow(0.9375, nowDef), MidpointRounding.AwayFromZero); // ダメージ計算式.
        if(nowHp - damage > 0)
        {
            nowHp -= damage;
        }
        else
        {
            nowHp = 0;
            gameObject.GetComponent<Renderer>().enabled = false;
        }
    }

    public (Pos2D actorPos, eDir actorDir, int hp, int atk, int def) GetStatus()
    {
        return (nowPosGrid, dir, nowHp, nowAtk, nowDef);
    }
    public virtual void SetStatus(Pos2D actorPos, eDir actorDir, int hp, int atk, int def)
    {
        (nowPosGrid.x, nowPosGrid.y) = (actorPos.x, actorPos.y);
        Vector3 nowPosWorld = Vector3.zero;
        (nowPosWorld.x, nowPosWorld.y) = (TilemapController.ToWorldX(actorPos.x), TilemapController.ToWorldY(actorPos.y));
        transform.position = nowPosWorld;
        dir = actorDir;
        nowHp = hp;
        (nowHp, nowAtk, nowDef) = (hp, atk, def);
    }

    public eTurnState GetEAct()
    {
        return turnState;
    }

    public void BeMoveBegin()
    {
        turnState = eTurnState.MOVE_BEGIN;
    }
    public void BeActBegin()
    {
        turnState = eTurnState.ACT_BEGIN;
    }
    public void BeKeyInput()
    {
        turnState = eTurnState.KEY_INPUT;
    }
}
