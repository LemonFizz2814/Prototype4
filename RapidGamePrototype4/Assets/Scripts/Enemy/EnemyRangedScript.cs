using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRangedScript : EnemyBaseClass
{
    int desiredRange = 2;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        attackRange = 3;
        sightRange = 5;
        actionsPerTurn = 1;
        attackDamage = 2;

        finishedTurn = true;
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        // Is enemies turn and hasn't already moved
        if (!gridManager.GetPlayersTurn() && !finishedTurn)
        {
            // Player is in sight range
            if (InAttackRangeToPlayer(sightRange) && !gridManager.PlayerInBushes())
            {
                // Player is in attacking range
                if (InAttackRangeToPlayer(attackRange))
                {
                    // If player is too close enemy will move away
                    if (DistanceBetweenObject(playerPos, transform.position) >= desiredRange)
                    {
                        AttackPlayer(attackDamage);
                        thisTurnActions--;
                    }
                    // Player is too close to enemy, move away from player
                    else
                    {
                        MoveAwayFromPlayer(GetSurroundingGround());
                        thisTurnActions--;
                    }
                }
                // Player is in sight range but not attacking range
                else
                {
                    MoveTowardsPlayer(GetSurroundingGround());
                    thisTurnActions--;
                }
            }
            // Skip enemy turn due to player outside sight range
            else
            {
                thisTurnActions = 0;
                finishedTurn = true;
            }
        }
    }
}