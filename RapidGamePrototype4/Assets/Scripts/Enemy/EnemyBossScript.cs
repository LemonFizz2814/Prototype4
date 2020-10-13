using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBossScript : EnemyBaseClass
{
    public Material enemyTargetMtrl;

    bool startedThisTurn = false;
    Vector3 attackPoint;

    // Stage One Attack
    int stageOneAtkRnage = 2;
    int stageOneDamage = 10;

    // Stage Two Attack
    int stageTwoAtkRange = 1;
    int stageTwoDamage = 10;

    // Stage Three Attack
    int stageThreeAtkRange = 1;
    int stageThreeDamage = 20;

    enum EState
    {
        ENone,
        EStageOnePhaseOne,
        EStageOnePhaseTwo,
        EStageTwoPhaseOne,
        EStageTwoPhaseTwo,
        EStageThreePhaseOne,
        EStageThreePhaseTwo,
    };

    enum EStage
    {
        EStageOne,
        EStageTwo,
        EStageThree,
    };

    EStage currentStage = EStage.EStageOne;

    EState currentState = EState.ENone;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        attackRange = 10;
        sightRange = 10;
        actionsPerTurn = 1;
        attackDamage = 2;
        enemyHealth = 15;

        finishedTurn = true;

        currentState = EState.ENone;
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        StageManager();

        // Is enemies turn and hasn't already moved
        if (!gridManager.GetPlayersTurn() && !finishedTurn)
        {
            if (InAttackRangeToPlayer(attackRange))
            {
                // Does the first stage attacks
                if (currentStage == EStage.EStageOne)
                {
                    currentState = currentState == EState.ENone ? EState.EStageOnePhaseOne : currentState;
                    StageOneAttack();
                    thisTurnActions--;
                }
                // Does the second stage attacks
                else if (currentStage == EStage.EStageTwo)
                {
                    currentState = currentState == EState.ENone ? EState.EStageTwoPhaseOne : currentState;
                    StageTwoAttack();
                    thisTurnActions--;
                }
                // Does the third stage attacks
                else if (currentStage == EStage.EStageThree)
                {
                    currentState = currentState == EState.ENone ? EState.EStageThreePhaseOne : currentState;
                    StageThreeAttack();
                    thisTurnActions--;
                }
            }
            // Skip enemy turn due to player outside sight range
            else
            {
                currentState = EState.ENone;
                thisTurnActions = 0;
                finishedTurn = true;
            }
        }
    }

    List<GameObject> SurroundingObjects(Vector3 objPosition, int range)
    {
        // Gets all ground objects
        List<GameObject> allGroundObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("Movable"));
        allGroundObjects.AddRange(new List<GameObject>(GameObject.FindGameObjectsWithTag("Highlighted")));

        // Gets all the surrounding ground objects
        List<GameObject> surroundingGround = new List<GameObject>();

        // Finds all objects surrounding enemy
        for (int i = 0; i < allGroundObjects.Count; i++)
        {
            if (allGroundObjects[i].transform.position.z <= objPosition.z + range && // Top
                allGroundObjects[i].transform.position.z >= objPosition.z - range && // Bot
                allGroundObjects[i].transform.position.x <= objPosition.x + range && // Right
                allGroundObjects[i].transform.position.x >= objPosition.x - range)   // Left
            {
                surroundingGround.Add(allGroundObjects[i]);
            }
        }

        return (surroundingGround);
    }

    void HighlightArea(List<GameObject> gameObjects)
    {
        for (int i = 0; i < gameObjects.Count; i++)
        {
            gameObjects[i].GetComponent<Renderer>().material = enemyTargetMtrl;
        }
    }

    void StageOneAttack()
    {
        // First Phase of Attack
        if (currentState == EState.EStageOnePhaseOne)
        {
            List<GameObject> surroundingGround = SurroundingObjects(transform.position, stageOneAtkRnage);
            HighlightArea(surroundingGround);

            currentState = EState.EStageOnePhaseTwo;
            startedThisTurn = true;
        }
        // Second Phase of Attack (The Damage Phase)
        else if (currentState == EState.EStageOnePhaseTwo && !startedThisTurn)
        {
            if (DistanceBetweenObject(transform.position, playerPos) <= stageOneAtkRnage)
            {
                AttackPlayer(stageOneDamage);
            }

            currentState = EState.EStageOnePhaseOne;
        }

        // Resets for next turn
        startedThisTurn = false;
    }

    void StageTwoAttack()
    {
        // First Phase of Attack
        if(currentState == EState.EStageTwoPhaseOne)
        {
            attackPoint = playerPos;

            List<GameObject> surroundingGround = SurroundingObjects(playerPos, stageTwoAtkRange);
            HighlightArea(surroundingGround);

            currentState = EState.EStageTwoPhaseTwo;
            startedThisTurn = true;
        }
        // Second Phase of attack
        else if (currentState == EState.EStageTwoPhaseTwo && !startedThisTurn)
        {
            if (DistanceBetweenObject(transform.position, attackPoint) <= stageTwoAtkRange)
            {
                AttackPlayer(stageTwoDamage);
            }

            currentState = EState.EStageTwoPhaseOne;
        }

        // Resets for next turn
        startedThisTurn = false;
    }

    void StageThreeAttack()
    {
        if(currentState == EState.EStageThreePhaseOne)
        {
            // Like Phase One
            List<GameObject> surroundingEnemy = SurroundingObjects(this.transform.position, stageThreeAtkRange);
            HighlightArea(surroundingEnemy);

            // Like Phase Two
            attackPoint = playerPos;

            List<GameObject> surroundingPlayer = SurroundingObjects(attackPoint, stageThreeAtkRange);
            HighlightArea(surroundingPlayer);

            // Switching States
            currentState = EState.EStageThreePhaseTwo;
            startedThisTurn = true;
        }
        else if (currentState == EState.EStageThreePhaseTwo && !startedThisTurn)
        {
            // Deals Damage
            if (DistanceBetweenObject(transform.position, attackPoint) <= stageThreeAtkRange || 
                (DistanceBetweenObject(transform.position, playerPos) <= stageThreeAtkRange))
            {
                AttackPlayer(stageThreeDamage);
            }

            // Switching state
            currentState = EState.EStageThreePhaseOne;
        }

        // Resets for next turn
        startedThisTurn = false;
    }

    void StageManager()
    {
        // Switches stages based on health lost
        if (enemyHealth >= 10 && currentStage != EStage.EStageOne)
        {
            currentStage = EStage.EStageOne;
            currentState = EState.ENone;
        }
        else if (enemyHealth < 10 && enemyHealth > 5 && currentStage != EStage.EStageTwo)
        {
            print("Stage Two");
            currentStage = EStage.EStageTwo;
            currentState = EState.ENone;
        }
        else if (enemyHealth <= 5 && currentStage != EStage.EStageThree)
        {
            print("Stage Three");
            currentStage = EStage.EStageThree;
            currentState = EState.ENone;
        }
    }
}