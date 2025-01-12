using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataScript : MonoBehaviour
{
    [Header("Set for speed of game")]
    public int GameSpeed = 1;

    [Header("analyse data")]
    public int winCount = 0;
    public int lossCount = 0;
    public int exploitedWinCount = 0;
    public int exploitedlossCount = 0;
    public int ExplorationWinCount = 0;
    public int ExplorationlossCount = 0;



    [Header("other data")]
    public FoodMovement foodOne;
    public FoodMovement foodTwo;
    public WolfAIScriptMk2 wolfAIScript;
    public int roundNum = 0;
    public int numberOfRounds = 100;
    public int numberOfStates = 50;
    public float TimeToPass = 0.25f;
    public float walkSpeed = 2f;
    public float jogSpeed = 4f;
    public float RunSpeed = 8f;

    public bool wolfLoss = false;
    public bool wolfFail = false;
    public bool wolfpartialSucess = false;

    public bool foodOneEaten;
    public bool foodTwoEaten;

    public bool foodOneSafe;
    public bool foodTwoSafe;

    public bool foodOneKeepGoing;
    public bool foodTwoKeepGoing;

    public void Start()
    {
        Time.timeScale = GameSpeed;
    }
    public void ResetRound()
    {
       roundNum++;
       foodOne.gameObject.SetActive(true);
       foodTwo.gameObject.SetActive(true);
       wolfAIScript.ResetWolf(wolfLoss, wolfFail, wolfpartialSucess);
       foodOne.ResetFood();
       foodTwo.ResetFood();
       
       ResetData();
    }

    public void FoodEaten(int foodNum)
    {
        if (foodNum == 1)
        {
            foodOneEaten = true;
        }
        if (foodNum == 2)
        {
            foodTwoEaten = true;
        }
    }

    public bool AnyFoodEaten()
    {
        if(foodOneEaten || foodTwoEaten) return true;
        else return false;
    }

    public void FoodSafe(int foodNum)
    {
        if (foodNum == 1) foodOneSafe = true;
        if (foodNum == 2) foodTwoSafe = true;
    }

    public bool OneFoodSafe()
    {
        if (foodOneSafe) return true;
        else if (foodTwoSafe) return true;
        else return false;
    }

    public bool IsFoodAllEaten()
    {
        if (foodOneEaten && foodTwoEaten)
        {
            return true;
        } else return false;
    }

    public bool IsFoodAllSafe()
    {
        if (foodOneSafe && foodTwoSafe)
        {
            return true;
        }
        else return false;
    }

    public bool IsOneFoodSafeAndOneEaten()
    {
        if ((foodOneEaten && foodTwoSafe) || (foodTwoEaten && foodOneSafe))
        { return true; }
        else return false;
    }

    public void WaitAction()
    {
        foodOne.ChangeSpeed();
        foodTwo.ChangeSpeed();
    }

    public void ResetData()
    {
        
        foodOneSafe = false;
        foodTwoSafe = false;
        foodOneEaten = false;
        foodTwoEaten = false;
        wolfLoss = false;
        wolfFail = false;
        wolfpartialSucess = false;
        foodOneKeepGoing = false;
        foodTwoKeepGoing = false;

    }


}
