using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class EnemyBaseClass : MonoBehaviour
{
    // Public
    public GridSystemManager gridManager;
    public Slider enemyHealthSlider;
    public GameObject canvas;

    // Protected
    protected GameObject playerCharacter;
    protected GameObject groundManager;

    // Melee Enemy Attributes
    protected int attackRange = 0;
    protected int sightRange = 0;
    protected int actionsPerTurn = 0;
    protected int attackDamage = 0;
    protected int enemyHealth = 3;

    protected bool finishedTurn = true;
    protected int thisTurnActions = 0;
    protected Vector3 prevPos;

    protected int gridXWidth;
    protected int gridYHeight;
    protected int gridTotalSize;
     
    protected Vector3 playerPos;
    protected Vector3 enemyPos;

    // Start is called before the first frame update
    protected void Start()
    {
        enemyHealthSlider.value = enemyHealth;

        playerCharacter = GameObject.FindGameObjectWithTag("Player");

        groundManager = gridManager.groundManager;

        // Grid data
        gridXWidth = gridManager.GameBoardData().gridXWidth;
        gridYHeight = gridManager.GameBoardData().gridYHeight;
        gridTotalSize = gridManager.GameBoardData().gridTotalSize;

        playerPos = gridManager.PlayerPosition();

        prevPos = transform.position;
        enemyPos = transform.position;
    }

    protected void Update()
    {
        playerPos = gridManager.PlayerPosition();
        finishedTurn = (thisTurnActions <= 0 && !gridManager.GetPlayersTurn()) ? true : false;
        MoveHealthUI();
    }

    protected void MoveTowardsPlayer(List<GameObject> touchingObjects)
    {
        GameObject[] allEnemiesInScene = GameObject.FindGameObjectsWithTag("Enemy");

        // Find the shortest path to player
        GameObject closest = touchingObjects[0];
        for (int i = 0; i < touchingObjects.Count; i++)
        {
            if (DistanceBetweenObject(playerPos, touchingObjects[i].transform.position) < DistanceBetweenObject(playerPos, closest.transform.position)
                && touchingObjects[i].transform.position != prevPos)
            {
                bool hasEnemyAlready = false;
                // Checks there's not already an enemy on the tile
                for (int j = 0; j < allEnemiesInScene.Length; j++)
                {
                    if(allEnemiesInScene[j].transform.position == touchingObjects[i].transform.position)
                    {
                        hasEnemyAlready = true;
                    }
                    if(!hasEnemyAlready)
                    {
                        closest = touchingObjects[i];
                    }
                }
            }
        }
        prevPos = transform.position; // To prevent it from back-tracking
        transform.LookAt(closest.transform.localPosition);
        transform.position = closest.transform.position;
        enemyPos = transform.position;
    }

    protected void MoveAwayFromPlayer(List<GameObject> touchingObjects)
    {
        // Find the longest path to player
        GameObject furthest = touchingObjects[0];
        for (int i = 0; i < touchingObjects.Count; i++)
        {
            if (DistanceBetweenObject(playerPos, touchingObjects[i].transform.position) > DistanceBetweenObject(playerPos, furthest.transform.position)
                && touchingObjects[i].transform.position != prevPos)
            {
                furthest = touchingObjects[i];
            }
        }

        prevPos = transform.position; // To prevent bing-pong effect
        transform.LookAt(furthest.transform.localPosition);
        transform.position = furthest.transform.position;
        enemyPos = transform.position;
    }

    void MoveHealthUI()
    {
        float offsetPosY = gameObject.transform.position.y + 2.0f;
        Vector3 offsetPos = new Vector3(gameObject.transform.position.x, offsetPosY, gameObject.transform.position.z);

        Vector2 canvasPos;
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(offsetPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPoint, null, out canvasPos);

        enemyHealthSlider.transform.localPosition = canvasPos;
    }

    protected List<GameObject> GetSurroundingGround()
    {
        List<GameObject> touchingObjects = new List<GameObject>();

        // Player Move highlight changes the tag thus needs to check for more tags
        List<GameObject> temp = new List<GameObject>(GameObject.FindGameObjectsWithTag("Movable"));
        temp.AddRange(new List<GameObject>(GameObject.FindGameObjectsWithTag("Highlighted")));

        // Find objects that are touching the enemy
        for (int i = 0; i < temp.Count; i++)
        {
            if (temp[i].transform.position.z == transform.position.z + 1 && temp[i].transform.position.x == transform.position.x - 1 || // Top left
                temp[i].transform.position.z == transform.position.z + 1 && temp[i].transform.position.x == transform.position.x + 1 || // Top Right

                temp[i].transform.position.z == transform.position.z - 1 && temp[i].transform.position.x == transform.position.x - 1 || // Bot Left
                temp[i].transform.position.z == transform.position.z - 1 && temp[i].transform.position.x == transform.position.x + 1 || // Bot Right

                temp[i].transform.position.x == transform.position.x + 1 && temp[i].transform.position.z == transform.position.z ||     // Right
                temp[i].transform.position.x == transform.position.x - 1 && temp[i].transform.position.z == transform.position.z ||     // Left

                temp[i].transform.position.z == transform.position.z + 1 && temp[i].transform.position.x == transform.position.x ||     // Up
                temp[i].transform.position.z == transform.position.z - 1 && temp[i].transform.position.z == transform.position.z)       // Down
            {
                touchingObjects.Add(temp[i]);
            }
        }
        return (touchingObjects);
    }

    protected bool InAttackRangeToPlayer(int _range)
    {
        if ((Mathf.Abs(playerCharacter.transform.position.x - transform.position.x) <= _range) &&
                (Mathf.Abs(playerCharacter.transform.position.z - transform.position.z) <= _range))
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }

    protected int DistanceBetweenObject(Vector3 _objOne, Vector3 _objTwo)
    {
        int x = (int)Mathf.Abs(_objOne.x - _objTwo.x);
        int z = (int)Mathf.Abs(_objOne.z - _objTwo.z);

        int distance = Mathf.Max(x, z);
        return (distance);
    }

    protected bool IsNotMovable(Vector3 _vec)
    {
        GameObject[] allNonMovealeObjs = GameObject.FindGameObjectsWithTag("NonMovable");
        for(int i = 0; i < allNonMovealeObjs.Length; i++)
        {
            if (allNonMovealeObjs[i].transform.position.x == _vec.x &&
                allNonMovealeObjs[i].transform.position.z == _vec.z)
            {
                return (true);
            }
        }
        return (false);
    }

    protected void AttackPlayer(int _damage)
    {
        //change to find the player that the enemy is attacking
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>().IncrHealth(-_damage);
    }

    public bool HasFinishedTurn()
    {
        return (finishedTurn);
    }

    public void StartEnemiesTurn()
    {
        thisTurnActions = actionsPerTurn;
        finishedTurn = false;
    }

    public Vector3 GetPos()
    {
        return enemyPos;
    }

    public void DamageEnemy(int _damage)
    {
        print("Ouch that hurt ;-;");
        enemyHealth -= _damage;
        enemyHealthSlider.value = enemyHealth;

        if(enemyHealth <= 0)
        {
            print("I am ded - " + gameObject.name);
            Destroy(gameObject);
            Destroy(enemyHealthSlider);
            GameObject.Find("EnemyManager").GetComponent<EnemyManager>().EnemyDead(gameObject.name);
            Destroy(gameObject);
        }
    }
}