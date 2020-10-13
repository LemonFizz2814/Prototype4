using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeScript : EnemyBaseClass
{
    new void Start()
    {
        base.Start();

        attackRange = 1;
        sightRange = 5;
        actionsPerTurn = 1;
        attackDamage = 2;
        enemyHealth = 3;

        finishedTurn = true;
    }

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
                    AttackPlayer(attackDamage);
                    thisTurnActions--;
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