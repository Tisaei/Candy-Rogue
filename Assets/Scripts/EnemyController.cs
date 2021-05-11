using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyRogueBase;

public class EnemyController : ActorController
{
    private PlayerController playerController;

    protected override void Awake()
    {
        base.Awake();
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    public override string ToString() { return base.ToString() + " Enemy(" + actorData.actorName + ")"; }

    public Behavior decideBehavior(Behavior playerBehavior) // EnemyBehaviourToConsistencyでは目標座標が壁かどうか検証しないのでここで壁を目標座標にしないようにする.
    {
        Vec2D moveVec;
        (int dx, int dy) = (playerController.GetNowPosGrid().x - GetNowPosGrid().x, playerController.GetNowPosGrid().y - GetNowPosGrid().y);
        if(Math.Abs(dx) > Math.Abs(dy))
        {
            if(dx < 0) { moveVec = new Vec2D(eDir.Left, eLen.One); } else { moveVec = new Vec2D(eDir.Right, eLen.One); }
        }
        else
        {
            if (dy < 0) { moveVec = new Vec2D(eDir.Down, eLen.One); } else { moveVec = new Vec2D(eDir.Up, eLen.One); }
        }
        //if (playerBehavior.isMove)
        //{
        //    if (GetNowPosGrid().y % 2 == 0)
        //    {
        //        if (playerBehavior.move.dir == eDir.Up || playerBehavior.move.dir == eDir.Down)
        //        {
        //            moveVec = new Vec2D(eDir.Up, eLen.One);
        //        }
        //        else
        //        {
        //            moveVec = new Vec2D(eDir.Down, eLen.One);
        //        }
                    
        //    }
        //    else
        //    {
        //        return new Behavior(false);
        //    }
        //}
        //else
        //{
        //    if(GetNowPosGrid().y % 2 == 0)
        //    {
        //        moveVec = new Vec2D(eDir.Up, eLen.One);
        //    }
        //    else
        //    {
        //        moveVec = new Vec2D(eDir.Down, eLen.One);
        //    }
        //}
        (_, moveVec, _) = TilemapController.CoordinateMoveTo(GetNowPosGrid(), moveVec, true);
        return new Behavior(true, moveVec);
    }
}
