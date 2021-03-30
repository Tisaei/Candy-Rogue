using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyRogueBase;

public class EnemyController : ActorController
{
    [SerializeField]
    private EnemyData enemyData;

    public Behavior decideBehavior(Behavior playerBehavior)
    {
        if (playerBehavior.isMove)
        {
            if(playerBehavior.move.dir == eDir.Up || playerBehavior.move.dir == eDir.Down)
            {
                return new Behavior(true, new Vec2D(eDir.Up, eLen.One));
            }
            else
            {
                return new Behavior(true, new Vec2D(eDir.Down, eLen.One));
            }
        }
        else
        {
            return new Behavior(false);
        }
    }
}
