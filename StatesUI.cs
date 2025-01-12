using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatesUI : MonoBehaviour
{
    public GameDataScript gameData;
    public List<TextMeshProUGUI> VisualDic;
    public List <TextMeshProUGUI> StateInfo;
    public List <int> StateExploiteddWithWin;
    public List<int> StateExploiteddWithLoss;
    public List<int> StateSuccsess;
    public List<int> StateFailures;
    
    public TextMeshProUGUI timeStamp;
    public TextMeshProUGUI stamp;
    public TextMeshProUGUI episilon;
    public Image ExploitingLight;
    public int previousState = 0;
    public float previousScore = 0; 
    public bool previousSuccess = false;
    private bool FirstTurn = true;

    // Start is called before the first frame update
    void Start()
    {
        int count = 0;
        while (count < StateExploiteddWithWin.Count)
        {
            StateExploiteddWithWin[count] = 0;
            StateExploiteddWithLoss[count] = 0; 
            StateSuccsess[count] = 0;
            StateFailures[count] = 0;
            count++;
        }
        

        
        count = 0;
        foreach (var state in VisualDic)
        {
            state.text = "" + count + " =     " + "0";
            StateInfo[count].text = "S" + count + " W" + StateSuccsess[count] + " L" + StateFailures[count] + "\n" + "Ew" + StateExploiteddWithWin[count] + " l" + StateExploiteddWithLoss[count];
            count++;
        }
    }

    private void Update()
    {
        episilon.text = "" + (float)Math.Round(gameData.wolfAIScript.GetEpisiolon(), 4);
    }

    public void updateTheSate(int state, float score, bool success)
    {
        Debug.Log(state);
        VisualDic[state].text = "" + state + " =     " + score;

        if (success)
        {
            StateSuccsess[state]++;            
        } else StateFailures[state]++;

        StateInfo[state].text = "S"+state + " W" + StateSuccsess[state] + " L" + StateFailures[state] + "\n" + "Ew" + StateExploiteddWithWin[state] +" l" + StateExploiteddWithLoss[state]; ;

        previousState = state;
        previousSuccess = success;
    }

    public void dinnerTimeselected(int state)
    {
        VisualDic[state].color = Color.cyan;

        if (!FirstTurn)
        {
            if (previousSuccess) VisualDic[previousState].color = Color.yellow;
            else VisualDic[previousState].color = Color.magenta;
        }else FirstTurn = false;
    }

    public void TuenOnExploitingLight(bool turnOn)
    {
        if(turnOn) ExploitingLight.color = Color.green;  
        else ExploitingLight.color = Color.red;        
    }

    public void updateTimeStamp(int time)
    {
        timeStamp.text = "Time Index: " + time;
    }

    public void GueesedTimeIndex(int time)
    {
        stamp.text = "Guessed: " + time;
    }

    public void resetUI()
    {
        
    }
}
