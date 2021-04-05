using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyRogueBase;

public class EnemyController : ActorController
{
    [SerializeField]
    public EnemyData enemyData;

    public Behavior decideBehavior(Behavior playerBehavior) // EnemyBehaviourToConsistencyでは目標座標が壁かどうか検証しないのでここで壁を目標座標にしないようにする.
    {
        Vec2D moveVec;
        if (playerBehavior.isMove)
        {
            if (GetNowPosGrid().y % 2 == 0)
            {
                if (playerBehavior.move.dir == eDir.Up || playerBehavior.move.dir == eDir.Down)
                {
                    moveVec = new Vec2D(eDir.Up, eLen.One);
                }
                else
                {
                    moveVec = new Vec2D(eDir.Down, eLen.One);
                }
                    
            }
            else
            {
                return new Behavior(false);
            }
        }
        else
        {
            if(GetNowPosGrid().y % 2 == 0)
            {
                moveVec = new Vec2D(eDir.Up, eLen.One);
            }
            else
            {
                moveVec = new Vec2D(eDir.Down, eLen.One);
            }
        }
        (_, moveVec, _) = TilemapController.CoordinateMoveTo(GetNowPosGrid(), moveVec);
        return new Behavior(true, moveVec);
    }
}
