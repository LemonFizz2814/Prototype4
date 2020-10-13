using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public GameObject playerOBJ;
    public GameObject gridSystemManager;

    public TextMeshProUGUI changeAttackButtonTMP;
    public TextMeshProUGUI changeActionButtonTMP;
    public TextMeshProUGUI endTurnButtonTMP;
    public TextMeshProUGUI turnsLeftTMP;
    public TextMeshProUGUI powerTMP;

    PlayerScript playerSCR;

    GridSystemManager gridManagerSCR;

    Image buttonAlpha;

    private void Start()
    {
        playerSCR = playerOBJ.GetComponent<PlayerScript>();
        gridManagerSCR = gridSystemManager.GetComponent<GridSystemManager>();

        buttonAlpha = changeAttackButtonTMP.transform.parent.GetComponent<Image>();
        var tempColor = buttonAlpha.color;
        tempColor.a = 0.5f;
        buttonAlpha.color = tempColor;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartPressed();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadScene("MainGameScene");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SceneManager.LoadScene("Level1Scene");
        }

        // Attack type button title update
        if(playerSCR.CurrentActiveAttack() == PlayerScript.EAttackType.EMelee)
        {
            changeAttackButtonTMP.SetText("Melee: " + gridManagerSCR.meleeRequirement);
        }
        else if(playerSCR.CurrentActiveAttack() == PlayerScript.EAttackType.ERanged)
        {
            changeAttackButtonTMP.SetText("Ranged: " + gridManagerSCR.rangeRequirement);
        }

        // Game state button title update
        if (gridManagerSCR.CurrentGameState() == GridSystemManager.EGameState.EPlayerMove)
        {
            changeActionButtonTMP.SetText("Move");
        }
        else if(gridManagerSCR.CurrentGameState() == GridSystemManager.EGameState.EPlayerAttack)
        {
            changeActionButtonTMP.SetText("Attack");
        }

        // Turn title update
        if (gridManagerSCR.GetPlayersTurn())
        {
            endTurnButtonTMP.SetText("End Turn");
        }
        else if(!gridManagerSCR.GetPlayersTurn())
        {
            endTurnButtonTMP.SetText("Enemies Turn");
        }

        // Number of players turns
        turnsLeftTMP.text = "Turns Left: " + playerSCR.PlayerTurns().ToString();
        powerTMP.text = "" + gridManagerSCR.power;
    }

    public void ChangeAttack()
    {
        if (playerSCR.CurrentActiveAttack() == PlayerScript.EAttackType.EMelee)
        {
            playerSCR.SetCurrentAttack(PlayerScript.EAttackType.ERanged);
        }
        else if (playerSCR.CurrentActiveAttack() == PlayerScript.EAttackType.ERanged)
        {
            playerSCR.SetCurrentAttack(PlayerScript.EAttackType.EMelee);
        }
    }

    public void ChangeAction()
    {
        if (gridManagerSCR.CurrentGameState() == GridSystemManager.EGameState.EPlayerMove)
        {
            var tempColor = buttonAlpha.color;
            tempColor.a = 1.0f;
            buttonAlpha.color = tempColor;
            gridManagerSCR.SetCurrentGameState(GridSystemManager.EGameState.EPlayerAttack);
        }
        else if (gridManagerSCR.CurrentGameState() == GridSystemManager.EGameState.EPlayerAttack)
        {
            var tempColor = buttonAlpha.color;
            tempColor.a = 0.5f;
            buttonAlpha.color = tempColor;
            gridManagerSCR.SetCurrentGameState(GridSystemManager.EGameState.EPlayerMove);
            gridManagerSCR.SetGroundMaterial();
        }
    }

    public void EndTurn()
    {
        if (gridManagerSCR.GetPlayersTurn())
        {
            playerSCR.EndPlayersTurn();
            gridManagerSCR.ChangeTurn(false);
        }
    }

    public void HealButton()
    {
        if (gridManagerSCR.power >= gridManagerSCR.healRequirement)
        {
            playerSCR.IncrHealth(1);
            gridManagerSCR.power -= gridManagerSCR.healRequirement;
            playerSCR.SetPlayerTurns(-1);
        }
    }

    public void RestartPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevelPressed()
    {
        //if more levels are created make an array of the level names
        SceneManager.LoadScene("MainGameScene");
    }

    public void BackToMenuPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }
}