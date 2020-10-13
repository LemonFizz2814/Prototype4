using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject gridManager;
    public CameraController cameraControllerSCR;

    GridSystemManager gridManagerSCR;

    GameObject[] allEnemiesInScene;

    int numberEnemies = 0;

    float enemyWaitTime = 0.5f;



    // Start is called before the first frame update
    void Start()
    {
        gridManagerSCR = gridManager.GetComponent<GridSystemManager>();
        GetAllEnemies();
    }

    // Update is called once per frame
    void Update()
    {
        /*if (!gridManagerSCR.GetPlayersTurn()) // Is the enemies turn
        {
            int hasEnemyFinised = 0;
            for (int i = 0; i < numberEnemies; i++)
            {
                // Check if all enemies have finished their turn
                EnemyBaseClass enemyBaseScript = allEnemiesInScene[i].GetComponent<EnemyBaseClass>();
                hasEnemyFinised = enemyBaseScript.HasFinishedTurn() ? hasEnemyFinised += 1 : hasEnemyFinised;
            }

            // If all enemies have finised turn then change to players turn
            if (hasEnemyFinised == numberEnemies)
            {
                gridManagerSCR.ChangeTurn(false);
            }
        }*/
    }

    void GetAllEnemies()
    {
        allEnemiesInScene = GameObject.FindGameObjectsWithTag("Enemy");
        numberEnemies = allEnemiesInScene.Length;

        //No more enemies so game is won
        if(numberEnemies == 0)
        {
            print("Game Won");
            gridManagerSCR.GameWon();
        }
    }

    public void DamageEnemies(float _x, float _z)
    {
        for (int i = 0; i < numberEnemies; i++)
        {
            if(allEnemiesInScene[i].GetComponent<EnemyBaseClass>().GetPos().x == _x && allEnemiesInScene[i].GetComponent<EnemyBaseClass>().GetPos().z == _z)
            {
                allEnemiesInScene[i].GetComponent<EnemyBaseClass>().DamageEnemy(3);
            }
        }
    }

    public void EnemyDead(string _name)
    {
        GetAllEnemies();
    }

    public IEnumerator SetEnemiesTurn()
    {
        GetAllEnemies();
        for(int i = 0; i < numberEnemies; i++)
        {
            // Starts all enemies turn
            EnemyBaseClass enemyBaseScript = allEnemiesInScene[i].GetComponent<EnemyBaseClass>();
            cameraControllerSCR.SnapToPosition(enemyBaseScript.GetPos());

            //camera shows enemy actions
            yield return new WaitForSeconds(enemyWaitTime);

            enemyBaseScript.StartEnemiesTurn();
            yield return new WaitForSeconds(0.01f);
            cameraControllerSCR.SnapToPosition(enemyBaseScript.GetPos());

            yield return new WaitForSeconds(enemyWaitTime);
        }

        cameraControllerSCR.SnapToPosition(GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>().GetPos());

        yield return new WaitForSeconds(enemyWaitTime);

        //Back to players turn
        PlayerScript.actionsPerTurn = PlayerScript.maxActionsPerTurn;
        gridManagerSCR.SetCurrentGameState(GridSystemManager.EGameState.EPlayerMove);
        gridManagerSCR.ChangeTurn(true);
        gridManagerSCR.IncrPower(1);
        gridManagerSCR.SetGroundMaterial();
    }
}
