using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class GridSystemManager : MonoBehaviour
{
    //public
    public Camera mainCamera;

    public EnemyManager enemyManager;

    public GameObject playerCharacter;
    public GameObject groundManager;

    // UI
    public GameObject deathScreenCanvas;
    public GameObject winScreenCanvas;
    public GameObject canvas;

    public Material selectedMtrl;
    public Material defaultMtrl1;
    public Material defaultMtrl2;

    public Slider powerSlider;
    
    public int power = 30;
    public int meleeRequirement = 10;
    public int rangeRequirement = 15;
    public int healRequirement = 20;

    //private
    int groundPos = 0;

    int maxPower = 100;
    int rangeDirection = 1;

    int[,] gridArray1 = new int[10, 7];
    int[,] gridArray2 = new int[14, 13];
    int groundLayerMask = 1 << 8;

    bool rangeIsHorizontal;
    bool isPlayersTurn = true;
    bool gameOver = false;

    public enum EGameState
    {
        EEnemyMove,
        EEnemyAttack,
        EPlayerMove,
        EPlayerAttack,
    };
    EGameState currentState = EGameState.EPlayerMove;

    public struct GameGrid
    {
        public int gridXWidth;
        public int gridYHeight;
        public int gridTotalSize;
    };
    GameGrid gameGrid = new GameGrid();

    Vector3 playerPos = new Vector3(0, 0, 0);

    // Ground data
    RaycastHit cast;
    GameObject currentGroundObject;

    private void Start()
    {
        //IncrPower(20);

        winScreenCanvas.SetActive(false);
        deathScreenCanvas.SetActive(false);

        currentGroundObject = groundManager.transform.GetChild(0).gameObject;

        powerSlider.maxValue = maxPower;
        powerSlider.value = power;
        if (SceneManager.GetActiveScene().name == "MainGameScene")
        {
            gameGrid.gridXWidth = gridArray1.GetLength(0);
            gameGrid.gridYHeight = gridArray1.GetLength(1);
            gameGrid.gridTotalSize = gameGrid.gridXWidth * gameGrid.gridYHeight;
        }
        else if (SceneManager.GetActiveScene().name == "Level1Scene")
        {
            gameGrid.gridXWidth = gridArray2.GetLength(0);
            gameGrid.gridYHeight = gridArray2.GetLength(1);
            gameGrid.gridTotalSize = gameGrid.gridXWidth * gameGrid.gridYHeight;
        } 

        SetGroundMaterial();
    }

    void Update()
    {
        if (!gameOver)
        {
            // Changes to enemies turn when playe has no actions
            if (PlayerScript.actionsPerTurn <= 0 && isPlayersTurn)
            {
                WipeBoard();
                ChangeTurn(false);
                playerCharacter.GetComponent<PlayerScript>().EndPlayersTurn();
            }

            // Updates the ground move material
            /*if (PlayerScript.actionsPerTurn <= 0 || currentState != EGameState.EPlayerMove)
            {
                //SetGroundMaterial(defaultMtrl);
            }
            else
            {
                SetGroundMaterial(selectedMtrl);
            }*/

            RaycastHit hit;
            //check for mouse click on ground detection
            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, groundLayerMask))
            {
                cast = hit;

                if (isPlayersTurn)
                {
                    //move action
                    if (currentState == EGameState.EPlayerMove)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            MoveAction();
                        }
                    }
                    //ranged action
                    else if (playerCharacter.GetComponent<PlayerScript>().currentAttack == PlayerScript.EAttackType.ERanged)
                    { 
                        RangedAction();
                        //on click on selected ground
                        if (Input.GetMouseButtonDown(0) && cast.transform.CompareTag("Highlighted") && power >= rangeRequirement)
                        {
                            PlayerScript.actionsPerTurn--;
                            WipeBoard();
                            enemyManager.DamageEnemies(cast.transform.position.x, cast.transform.position.z);
                            //playerCharacter.GetComponent<PlayerScript>().RangedAttack(cast.transform.position.x, cast.transform.position.z);
                            IncrPower(-rangeRequirement);

                            //end players turn here
                        }
                    }
                    //melee action
                    else if (playerCharacter.GetComponent<PlayerScript>().currentAttack == PlayerScript.EAttackType.EMelee)
                    {
                        MeleeAction();
                        if (Input.GetMouseButtonDown(0) && cast.transform.CompareTag("Highlighted") && power >= meleeRequirement)
                        {
                            //MeleeAttack
                            PlayerScript.actionsPerTurn--;
                            WipeBoard();
                            enemyManager.DamageEnemies(cast.transform.position.x, cast.transform.position.z);
                            playerCharacter.GetComponent<PlayerScript>().MeleeAttack();
                            //playerCharacter.GetComponent<PlayerScript>().RangedAttack(cast.transform.position.x, cast.transform.position.z);
                            IncrPower(-meleeRequirement);
                        }
                    }
                }
            }
        }
    }

    bool IsInField()
    {
        //check if in 3x3 around player
        if (cast.transform.position.x - playerPos.x <= 1 && cast.transform.position.x - playerPos.x >= -1)
        {
            if (cast.transform.position.z - playerPos.z <= 1 && cast.transform.position.z - playerPos.z >= -1)
            {
                //check if ground is able to be moved to
                if (cast.transform.CompareTag("Highlighted") || cast.transform.CompareTag("Bushes"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void SetGroundMaterial()
    {
        for (int i = -1; i < 2; ++i)
        {
            for (int j = -1; j < 2; ++j)
            {
                //get groundplate child depending on position
                groundPos = (int)((playerPos.x + i) + ((playerPos.z + j) * gameGrid.gridYHeight));

                //check if it isn't out of the groundmanagers range of children 
                if (groundPos >= 0 && groundPos < gameGrid.gridTotalSize)
                {
                    if (groundManager.transform.GetChild(groundPos).CompareTag("Movable") || groundManager.transform.GetChild(groundPos).CompareTag("Bushes"))
                    {
                        if(groundManager.transform.GetChild(groundPos).CompareTag("Movable"))
                        {
                            groundManager.transform.GetChild(groundPos).tag = "Highlighted";
                        }
                        
                        groundManager.transform.GetChild(groundPos).GetComponent<Renderer>().material = selectedMtrl;
                    }
                }
            }
        }
    }

    public void PlayerDeathScreen()
    {
        canvas.SetActive(false);
        deathScreenCanvas.SetActive(true);
        //playerCharacter.SetActive(false);
    }

    public void GameWon()
    {
        gameOver = true;
        canvas.SetActive(false);
        winScreenCanvas.SetActive(true);
        //playerCharacter.SetActive(false);
    }

    void MoveAction()
    {
        if (IsInField())
        {
            PlayerScript.actionsPerTurn--;

            //set previous groundPos to previous colour
            //SetGroundMaterial(defaultMtrl);
            WipeBoard();

            //set player to selected object
            currentGroundObject = cast.transform.gameObject;
            playerPos = currentGroundObject.transform.position;
            playerCharacter.transform.LookAt(currentGroundObject.transform.localPosition);
            playerCharacter.transform.position = playerPos;

            //set new ground to new colour
            SetGroundMaterial();
            //groundPos = (int)(playerPos.x + (playerPos.z * gridYHeight));
            //groundManager.transform.GetChild(groundPos).GetComponent<Renderer>().material = selectedMtrl;
        }
    }

    void MeleeAction()
    {
        WipeBoard();
        SetGroundMaterial();
    }

    void RangedAction()
    {
        //reset previous changes
        //RangeLimitSet(defaultMtrl);
        WipeBoard();

        //print("differnce: " + (cast.transform.position.x - playerCharacter.transform.position.x));

        if (cast.transform.position.x - playerCharacter.transform.position.x == 0)
        {
            //print("vertical");
            rangeIsHorizontal = false;
            rangeDirection = (int)Mathf.Clamp(cast.transform.position.z - playerCharacter.transform.position.z, -1, 1);
        }
        else
        {
            //print("horizontal");
            rangeIsHorizontal = true;
            rangeDirection = (int)Mathf.Clamp(cast.transform.position.x - playerCharacter.transform.position.x, -1, 1);
        }

        if(rangeDirection == 0)
        {
            rangeDirection = 1;
        }

        //print("rangeDirection :" + rangeDirection);
        //print("rangeIsHorizontal: " + rangeIsHorizontal);

        RangeLimitSet();
    }

    void RangeLimitSet()
    {
        int incr = 0;
        groundPos = (int)(playerPos.x + (playerPos.z * gameGrid.gridYHeight));

        while (RangeCheck())
        {
            groundManager.transform.GetChild(groundPos).GetComponent<Renderer>().material = selectedMtrl;
            groundManager.transform.GetChild(groundPos).tag = "Highlighted";

            incr++;

            //get groundplate child depending on position
            if (rangeIsHorizontal)
            {
                groundPos = (int)((playerPos.x + (incr * rangeDirection)) + (playerPos.z * gameGrid.gridYHeight));
            }
            else
            {
                groundPos = (int)(playerPos.x + ((playerPos.z + (incr * rangeDirection)) * gameGrid.gridYHeight));
            }
        }
    }

    bool RangeCheck()
    {
        if (groundPos >= 0 && groundPos < gameGrid.gridTotalSize && groundManager.transform.GetChild(groundPos).CompareTag("Movable"))
        {
            if (rangeIsHorizontal)
            {
                if (groundPos % gameGrid.gridXWidth != gameGrid.gridXWidth - 1)
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    public void IncrPower(int _i)
    {
        if (power < maxPower)
        {
            power += _i;
        }
        powerSlider.value = power;
    }

    public RaycastHit ClickCast()
    {
        return (cast);
    }

    public GameGrid GameBoardData()
    {
        return (gameGrid);
    }

    public Vector3 PlayerPosition()
    {
        return (playerPos);
    }

    public EGameState CurrentGameState()
    {
        return (currentState);
    }

    public void SetCurrentGameState(EGameState _newGameState)
    {
        WipeBoard();
        currentState = _newGameState;
    }

    public void ChangeTurn(bool _bool)
    {
        isPlayersTurn = _bool;
    }

    public bool GetPlayersTurn()
    {
        return (isPlayersTurn);
    }

    public bool PlayerInBushes()
    {
        groundPos = (int)(playerPos.x + (playerPos.z * gameGrid.gridYHeight));
        bool isInBushes = groundManager.transform.GetChild(groundPos).CompareTag("Bushes") ? true : false;

        return (isInBushes);
    }

    public void WipeBoard()
    {
        for(int i = 0; i < gameGrid.gridTotalSize; i++)
        {
            if (!groundManager.transform.GetChild(i).CompareTag("NonMovable"))
            {
                if (i % 2 == 0)
                {
                    groundManager.transform.GetChild(i).GetComponent<Renderer>().material = defaultMtrl2;
                }
                else
                {
                    groundManager.transform.GetChild(i).GetComponent<Renderer>().material = defaultMtrl1;
                }

                if (groundManager.transform.GetChild(i).CompareTag("Highlighted"))
                {
                    groundManager.transform.GetChild(i).tag = "Movable";
                }
            }
        }
    }
}