using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class WolfAIScriptMk2 : MonoBehaviour
{
    [Header("Wolf Info")]
    public int speed = 6;
    private Vector3 WolfStartPosition;
    private Quaternion WolfRotation;
    public bool exploiting = false;
    public bool exploited = false;    
    public bool exploring = false;
    public bool dinnerTime = false;

    [Header("Playthrough Info")]
    
    [SerializeField] private bool guessed = false;
    public bool gameLive = true;
    public bool timestamped;
    public int correctGuessCount = 0;
    public Vector3 Goal;
    public int guessedTimeIndex;
    private float distanceError;

    public (float, float, float) state;

    [Header("Objects")]
    [SerializeField] private GameDataScript FoodData;
    private GameObject FoodToChase;
    private GameObject OtherFoodToChase;
    public GameObject foodObjectOne;
    public GameObject foodObjectTwo;
    public GameObject wolfGuessObj;
    private GameObject GuessObject = null;

    [Header("Scripts")]
    public FoodMovement movingObject;
    public StatesUI stateUiScript;

    [Header("UI")]
    public TextMeshProUGUI ExploreCountTxt;
    public TextMeshProUGUI ExploitCountTxt;
    public TextMeshProUGUI GuessNumberCountTxt;
    private int ExploreCount = 0;
    private int ExploitCount = 0;
    private int GuessNumberCount = 0;
    public float value = 0;

    [Header("Reinforcment learning")]
    [SerializeField] private float learningRate = 0.3f;  // Alpha
    [SerializeField] private float discountFactor = 0.9f;  // Gamma
    [SerializeField] private float epsilon = 0.8f;  // Exploration probability
    [SerializeField] private float epsilonDecay = 0.995f;
    [SerializeField] private float epsilonGrowth = 0.005f;
    [SerializeField] private float ConsecutiveFailures = 0;
    [SerializeField] private int TimeIndex = 0;
    // Q-table
    private Dictionary<float, float> qTable = new Dictionary<float, float>();

    void Start()
    {
        WolfStartPosition = transform.position;
        WolfRotation = transform.rotation;
        ExploreCountTxt = GameObject.Find("ExploreCountTxt").GetComponent<TextMeshProUGUI>();
        ExploitCountTxt = GameObject.Find("ExploitCountTxt").GetComponent<TextMeshProUGUI>();
        GuessNumberCountTxt = GameObject.Find("GuessNumberCountTxt").GetComponent<TextMeshProUGUI>();
        stateUiScript = GameObject.Find("Canvas").GetComponent<StatesUI>();
        FoodData.foodOne = foodObjectOne.GetComponent<FoodMovement>();
        FoodData.foodTwo = foodObjectTwo.GetComponent<FoodMovement>();
        //totalDistance = movingObject.GetTotalDistance();
        StartCoroutine(timeCounter());
    }

    void Update()
    {
        float delta = speed * Time.deltaTime;

        if(FoodData.OneFoodSafe())
        {
            FoodToChase = OtherFoodToChase;
        }

        if (dinnerTime)
        {
            float reward;
            transform.LookAt(FoodToChase.transform);
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(FoodToChase.transform.position.x, transform.position.y, FoodToChase.transform.position.z), delta);
            if(transform.position.x == FoodToChase.transform.position.x)
            {
                FoodData.FoodEaten(FoodToChase.GetComponent<FoodMovement>().FoodNum);
                if (FoodData.IsFoodAllEaten())
                {
                    //float reward = (int)Mathf.Max(25 - distanceError, -10); // Higher reward for accuracy  
                    reward = 10f;
                    Reward(reward, true);
                    FoodData.ResetRound();
                }
                else if (FoodData.IsOneFoodSafeAndOneEaten())
                {
                    reward = 5f;
                    Reward(reward, true);
                    FoodToChase.SetActive(true);
                    FoodData.ResetRound();
                }
                else
                {                    
                    FoodToChase.SetActive(false);
                    FoodToChase = OtherFoodToChase;
                }
                                    
            }
        }

        if (guessed) return;

        if (timestamped)
        {
            timestamped = false;
            DecisionFunction();
        }       
    }

    /// <reinforcmmentLearning>
    /// /////////////////////////////////////////////////////////
    /// </summary>
    public void DecisionFunction()
    {
        // Simulate decision-making
        float currentDistance = movingObject.GetDistanceCovered();
        //float halfwayPoint = totalDistance / 2;
        value = Random.value;
        if ((value < epsilon && !exploiting) || exploring)  // Exploration
        {
            ExplorationPolicy(currentDistance);  // Random guess

        }
        else // Exploitation (use learned Q-values)
        {
            ExploitationPloicy(currentDistance);
        }
    }

    private void ExplorationPolicy(float currentDistance)
    {
        exploring = true;
        // wolf has 2 choices at a time index action 1 is to wait, action 2 is to place his guess;
        float explorationChance = Mathf.Lerp(10, 1, (float)TimeIndex / 100); // Decrease chance over time
        if (Random.Range(0, 101) < explorationChance)
        {
            
            SetUI(false);
            ActionChase();
            exploring = false; // choice has been made so reset 
        }
    }

    private void ExploitationPloicy(float currentDistance)
    {
        exploiting = true;
        stateUiScript.TuenOnExploitingLight(true);

        if (qTable.Count == 0 || !QHasKeysAbove(TimeIndex))
        {
            exploiting = false; // nothing to explot set back to explore
            stateUiScript.TuenOnExploitingLight(false);
            ExplorationPolicy(currentDistance); // call explore
        }
        else if (CheckQValue())
        {                      
            SetUI(true);
            ActionChase();
            exploited = true; // toupdate ui
            exploiting = false; // choice has been made so reset       
        }
    }

    private bool QHasKeysAbove(int threshold)
    {
        return qTable.Keys.Any(key => key > threshold);
    }

    private float GetMaxQValue()
    {
        if (qTable.Count == 0) return 0f;

        float maxQ = float.MinValue;
        foreach (var value in qTable.Values)
        {
            maxQ = Mathf.Max(maxQ, value);
        }

        return maxQ;
    }

    private bool CheckQValue()
    {
        if (qTable.Count == 0) return false;
        if (!qTable.ContainsKey(TimeIndex)) return false; // Current state not in Q-table

        // Get the current Q-value
        float currentQ = qTable[TimeIndex];

        // Normalize Q-values to prevent overflow
        float maxQ = GetMaxQValue(); // Find the maximum Q-value
        float sum = 0;

        foreach (var qValue in qTable.Values)
        {
            sum += Mathf.Exp(qValue - maxQ); // Normalize using maxQ
        }

        // Calculate the softmax probability
        float probability = Mathf.Exp(currentQ - maxQ) / sum;

        // Fallback to direct exploitation if current Q-value is close to the max
        if (Mathf.Approximately(currentQ, maxQ)) return true; // Exploit high-confidence states

        // Use probability-based selection otherwise
        return Random.value < probability;
    }


    /// <Wins>
    /// //////////////////////////////////////////////////////////
    /// <Wins>

    private void Reward(float reward, bool successful)
    {
        // Update Q-table based on learning formula
        if (!qTable.ContainsKey(guessedTimeIndex))
        {
            qTable[guessedTimeIndex] = 0; // Initialize if state is missing
        }

        // Q-Learning update rule
        qTable[guessedTimeIndex] += learningRate * (reward + discountFactor * GetMaxQValue() - qTable[guessedTimeIndex]);

        // Update UI with the new reward
        stateUiScript.updateTheSate(guessedTimeIndex, (int)qTable[guessedTimeIndex], successful);

        // Adaptive epsilon adjustment based on performance
        if (successful)
        {
            FoodData.winCount++;
            // Decay epsilon if guesses are successful
            if (exploited)
            {
                stateUiScript.StateExploiteddWithWin[guessedTimeIndex]++;
                FoodData.exploitedWinCount++;
            }else FoodData.ExplorationWinCount++;
            
            epsilon = Mathf.Max(0.1f, epsilon * epsilonDecay); // Minimum value 0.1
            ConsecutiveFailures = 0;
        }
        else 
        {
            FoodData.lossCount++;
            if (exploited)
            {
                stateUiScript.StateExploiteddWithLoss[guessedTimeIndex]++;
                FoodData.exploitedlossCount++;
            }else FoodData.ExplorationlossCount++;
            
            if (ConsecutiveFailures < 9)
            {
                epsilon = Mathf.Max(0.1f, epsilon * epsilonDecay); // Minimum value 0.1
                ConsecutiveFailures++;
            }
            else epsilon = Mathf.Min(1.0f, epsilon + epsilonGrowth); // Cap exploration at 1.0
        }

        // Debug log to track epsilon changes
        // Debug.Log($"Epsilon Updated: {epsilon}");
        Debug.Log("qtable reward is:"+ qTable[guessedTimeIndex]);
    }

    private float IncreaseReward(bool bothEaten, bool noneEaten, float rewardNum)
    {
        float rewardValue = rewardNum;
        if (bothEaten)
        {
            return rewardValue = rewardValue + rewardNum/2;
        }
        else if (noneEaten)
        {
            rewardValue = -15.0f;
        }
        else
        {
            return rewardValue = rewardNum / 2;
        }

        return rewardValue;
    }

    public void ActionChase() 
    {
        dinnerTime = true; // let the wolf know to chase the food
        guessed = true; // Mark that the Wolf has made its guess
        GetTarget();
        distanceError = Mathf.Abs(FoodToChase.transform.position.x - transform.position.x);
        guessedTimeIndex = (int)state.Item1;
        stateUiScript.GueesedTimeIndex(guessedTimeIndex);
        
    }

    private void GetTarget()
    {
        if (state.Item1 <= state.Item2)
        {
            FoodToChase = foodObjectOne;
            OtherFoodToChase = foodObjectTwo;
        }
        else
        {
            FoodToChase = foodObjectTwo;
            OtherFoodToChase = foodObjectOne;
        }
        OtherFoodToChase.GetComponent<FoodMovement>().KeepGoingChoice();
    }

    /// <UI>
    /// ////////////////////////////////////////////////////////////
    /// <UI>

    private void SetUI(bool exploit)
    {     
        if (exploit)
        {
            PlaceGuessVisualiser(false);
            ExploitCount++;
            ExploitCountTxt.text = ExploitCount.ToString();
            stateUiScript.TuenOnExploitingLight(false);
        }
        else
        {
            PlaceGuessVisualiser(true);
            ExploreCount++;
            ExploreCountTxt.text = ExploreCount.ToString();
        }
        GuessNumberCount++;
        GuessNumberCountTxt.text = GuessNumberCount.ToString();
        
        stateUiScript.dinnerTimeselected((int)state.Item1);
    }

    public float GetEpisiolon()
    {
        return epsilon;
    }

    private void PlaceGuessVisualiser(bool explore)
    {
        GuessObject = Instantiate(wolfGuessObj, new Vector3(movingObject.transform.position.x, 1, 0), Quaternion.identity);
        GuessObject.GetComponent<WolfGuessObj>().SetColour(explore);
    }

    /// <Rounds>
    /// ////////////////////////////////////////////////////////////
    /// <Rounds>

    public void ResetWolf(bool failToCatch, bool GotoDen, bool miniWin)
    {
        if (GotoDen) Reward(-8, false);
        else if (failToCatch) Reward(-4, false);
        else if(miniWin) Reward(5, true);
        
        dinnerTime = false;
        guessed = false;
        epsilon = Mathf.Max(epsilon * epsilonDecay);
        TimeIndex = 0;
        this.transform.position = WolfStartPosition;
        this.transform.rotation = WolfRotation;
        exploiting = false;
        exploited = false;
    }

    IEnumerator timeCounter()
    {
        while (gameLive)
        {
            TimeIndex++;
            state.Item1 = TimeIndex;
            state.Item2 = (int)Vector3.Distance(transform.position, foodObjectOne.transform.position);
            state.Item3 = (int)Vector3.Distance(transform.position, foodObjectOne.transform.position);
            
            FoodData.WaitAction();
            
            stateUiScript.updateTimeStamp(TimeIndex);
            yield return new WaitForSeconds(FoodData.TimeToPass);     
            timestamped = true;
        }
    }
}
