using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public GridSystemManager gridManager;
    public EnemyManager enemyManager;

    // Highlights
    public GameObject canAttackSPR;
    public GameObject canNotAttackSPR;
    public GameObject notPlayerTurnSPR;
    public GameObject playerMoveSPR;
    public GameObject canvas;

    public Slider healthSlider;

    int playerMeleeRange = 1;
    int playerRangedRange = 3;
    int playerMoveRange = 1;

    int thisTurnActions = 0;

    int totemAdd1 = 20;
    int totemAdd2 = 45;
    int totemAdd3 = 70;

    public static int actionsPerTurn;
    public const int maxActionsPerTurn = 4;

    public int health = 3;

    int gridXWidth;
    int gridYHeight;
    int gridTotalSize;

    RaycastHit lastCast;

    public enum EAttackType
    {
        ENone,
        EMelee,
        ERanged,
    };

    public EAttackType currentAttack = EAttackType.EMelee;

    private void Start()
    {
        // Gameboard data
        gridXWidth = gridManager.GameBoardData().gridXWidth;
        gridYHeight = gridManager.GameBoardData().gridYHeight;
        gridTotalSize = gridManager.GameBoardData().gridTotalSize;

        healthSlider.value = health;

        actionsPerTurn = maxActionsPerTurn;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Totem1"))
        {
            gridManager.IncrPower(totemAdd1);
            Destroy(other.gameObject);
        }
        if (other.gameObject.CompareTag("Totem2"))
        {
            gridManager.IncrPower(totemAdd2);
            Destroy(other.gameObject);
        }
        if (other.gameObject.CompareTag("Totem3"))
        {
            gridManager.IncrPower(totemAdd3);
            Destroy(other.gameObject);
        }
    }

    private void Update()
    {
        lastCast = gridManager.ClickCast();

        // Hightlights ground based on game state and attack type
        HightlightGround();

        MoveHealthUI();
    }

    void MoveHealthUI()
    {
        float offsetPosY = gameObject.transform.position.y + 2.0f;
        Vector3 offsetPos = new Vector3(gameObject.transform.position.x, offsetPosY, gameObject.transform.position.z);

        Vector2 canvasPos;
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(offsetPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPoint, null, out canvasPos);

        healthSlider.transform.localPosition = canvasPos;
    }

    private void HightlightGround()
    {
        if (lastCast.transform?? false)
        {            
            if (gridManager.CurrentGameState() == GridSystemManager.EGameState.EEnemyAttack ||
                gridManager.CurrentGameState() == GridSystemManager.EGameState.EEnemyMove)
            {
                // Enemy Turn
                notPlayerTurnSPR.SetActive(true);
                notPlayerTurnSPR.transform.position = lastCast.transform.position;

                canAttackSPR.SetActive(false);
                canNotAttackSPR.SetActive(false);
                playerMoveSPR.SetActive(false);
            }
            else
            {
                notPlayerTurnSPR.SetActive(false);

                // Player turn
                if (gridManager.CurrentGameState() == GridSystemManager.EGameState.EPlayerMove)
                {
                    if (lastCast.transform.CompareTag("Highlighted") || lastCast.transform.CompareTag("Bushes"))
                    {
                        playerMoveSPR.SetActive(true);
                        playerMoveSPR.transform.position = lastCast.transform.position;

                        // Deactivate other sprites
                        canNotAttackSPR.SetActive(false);
                        canAttackSPR.SetActive(false);
                        notPlayerTurnSPR.SetActive(false);
                    }
                    else
                    {
                        canNotAttackSPR.SetActive(true);
                        canNotAttackSPR.transform.position = lastCast.transform.position;

                        // Deactivate other sprites
                        playerMoveSPR.SetActive(false);
                        canAttackSPR.SetActive(false);
                        notPlayerTurnSPR.SetActive(false);
                    }
                }
                else if (currentAttack == EAttackType.EMelee)
                {
                    playerMoveSPR.SetActive(false);

                    if (lastCast.transform.CompareTag("Highlighted") || lastCast.transform.CompareTag("Bushes"))
                    {
                        canAttackSPR.SetActive(true);
                        canAttackSPR.transform.position = lastCast.transform.position;
                        canNotAttackSPR.SetActive(false);
                    }
                    else
                    {
                        canNotAttackSPR.SetActive(true);
                        canNotAttackSPR.transform.position = lastCast.transform.position;
                        canAttackSPR.SetActive(false);
                    }
                }
                else if (currentAttack == EAttackType.ERanged)
                {
                    playerMoveSPR.SetActive(false);

                    if (lastCast.transform.CompareTag("Highlighted") || lastCast.transform.CompareTag("Bushes"))
                    {
                        canAttackSPR.SetActive(true);
                        canAttackSPR.transform.position = lastCast.transform.position;
                        canNotAttackSPR.SetActive(false);
                    }
                    else
                    {
                        canNotAttackSPR.SetActive(true);
                        canNotAttackSPR.transform.position = lastCast.transform.position;
                        canAttackSPR.SetActive(false);
                    }
                }
            }
        }
    }

    public void IncrHealth(int _i)
    {
        health += _i;
        healthSlider.value = health;

        if (health <= 0)
        {
            //player dead
            print("you are the big ded");
          
            gridManager.PlayerDeathScreen();
        }
    }

    public Vector3 GetPos()
    {
        return gameObject.transform.localPosition;
    }

    public EAttackType CurrentActiveAttack()
    {
        return (currentAttack);
    }

    public void SetCurrentAttack(EAttackType _newAttack)
    {
        currentAttack = _newAttack;
    }

    public void MeleeAttack()
    {
        gameObject.transform.GetChild(0).GetComponent<Animator>().SetTrigger("Attack");
    }

    public void SetPlayerTurns(int _i)
    {
        actionsPerTurn += _i;
    }

    public int PlayerTurns()
    {
        return (actionsPerTurn);
    }

    public void EndPlayersTurn()
    {
        StartCoroutine(enemyManager.SetEnemiesTurn());
    }
}
