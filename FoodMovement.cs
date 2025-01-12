using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class FoodMovement : MonoBehaviour
{
    [SerializeField] private GameDataScript GameData;

    [Header("Playthrough Info")]
    public GameObject startPoint;
    public GameObject endPoint;
    public GameObject goalPoint;

    [Header("Food Info")]
    public float walkSpeed;
    public float jogSpeed;
    public float runSpeed;
    public float walking;
    public bool jogging;
    public bool running;
    public bool checkSpeed;
    public bool KeepGoing;
    public bool runningAway;
    private float currentSpeed;
    private float distanceCovered = 0f;
    public int FoodNum;

    [Header("Misc Info")]
    public WolfGuessObj WolfGuessObj;
    public WolfAIScriptMk2 WolfScript;
    public Renderer render;
    public Material jogMat;
    public Material walkMat;
    private Vector3 ResetPos;

    void Start()
    {
        walkSpeed = GameData.walkSpeed;
        jogSpeed = GameData.jogSpeed;
        runSpeed = GameData.RunSpeed;
        ResetPos = transform.position;
        currentSpeed = walkSpeed;
        checkSpeed = true;
        goalPoint = endPoint;
    }

    void Update()
    {
        float delta = currentSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(goalPoint.transform.position.x, transform.position.y, goalPoint.transform.position.z), delta);
        distanceCovered += delta;

        if(WolfScript.dinnerTime && !KeepGoing)
        {   
            runningAway = true;
            goalPoint = startPoint; 
        }

        // Stop when reaching the end point
        if (transform.position.x == goalPoint.transform.position.x)
        {
            GameData.FoodSafe(this.FoodNum);
            enabled = false;

            if(!runningAway)
            {
                if (!GameData.AnyFoodEaten())
                {
                    WolfScript.ActionChase();
                    WolfScript.stateUiScript.dinnerTimeselected((int)WolfScript.guessedTimeIndex);
                }
                GameData.wolfLoss = true;
                GameData.ResetRound();
            }

            if (GameData.roundNum < GameData.numberOfRounds)
            {                
                if (GameData.IsFoodAllSafe())
                {
                    GameData.wolfFail = true;
                    GameData.ResetRound();                    
                }
                if (GameData.IsOneFoodSafeAndOneEaten())
                {
                    GameData.wolfpartialSucess = true;
                    GameData.ResetRound();
                }
            } else Time.timeScale = 0f;

        }

        if (jogging) { render.material = jogMat; }
        else { render.material = walkMat; }
    }

    public void KeepGoingChoice()
    {
        float checkStartDistance = Vector3.Distance(transform.position, startPoint.transform.position);
        float checkendDistance = Vector3.Distance(transform.position, endPoint.transform.position);
        int ran = Random.Range(0, 11);
        if (checkendDistance < checkStartDistance)
        {
            if (ran < 6)
            {
                KeepGoing = true;
                runningAway = false;
                goalPoint = endPoint;
            }
            //Debug.Log("End closer and keepgoing is" + KeepGoing);
        }
        else { }//Debug.Log("start is closer");

    }

    public float GetDistanceCovered()
    {
        return distanceCovered;
    }

    public float GetTotalDistance()
    {
        return endPoint.transform.position.x - startPoint.transform.position.x;
    }

    public void ChangeSpeed()
    {
        int ran = Random.Range(1, 4);

        if (ran == 1)
        {
            currentSpeed = walkSpeed;
        } else if (ran == 2)
        { 
            currentSpeed = jogSpeed;
        }
        else if (ran == 3 )
        {
            currentSpeed = runSpeed;
        }
    }

    public void ResetFood()
    {
        goalPoint = endPoint;
        transform.position = ResetPos;
        distanceCovered = 0;
        ChangeSpeed();
        checkSpeed = true;
        enabled = true;   
        runningAway = false;
        KeepGoing = false;
    }
}