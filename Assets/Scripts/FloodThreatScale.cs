using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class FloodThreatScale : MonoBehaviour
{
    public static FloodThreatScale Instance;
    public static List<int> floodThreatModifiers = new List<int> { 0, 0, 1, 2, 3, 4, 5, 7, 9, 10 };
    public static List<(int, BoardSpace.SpaceType)> floodReach = new List<(int, BoardSpace.SpaceType)>
    { (2, BoardSpace.SpaceType.Black),
        (4, BoardSpace.SpaceType.Red),
        (7, BoardSpace.SpaceType.Yellow),
        (10, BoardSpace.SpaceType.Blue),
        (12, BoardSpace.SpaceType.Green) };

    public GameObject threatToken;
    private RectTransform threatTokenRectTransform;
    public Vector2 threatTokenTarget;
    public float moveSpeed = 5f;

    public List<int> tokenXPositions = new List<int> { -200, -150, -100, -50, 0, 50, 100, 150, 200, 250 };


    public int getFloodThreatModifier()
    {
        return floodThreatModifiers[StateManager.Instance.currentThreatIncrement];
    }

    void Awake()
    {
        Instance = this;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        threatTokenRectTransform = threatToken.GetComponent<RectTransform>();
        threatTokenTarget = threatTokenRectTransform.anchoredPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // Move the threat token towards the target position
        threatTokenRectTransform.anchoredPosition = Vector2.Lerp(
            threatTokenRectTransform.anchoredPosition, threatTokenTarget, Time.deltaTime * moveSpeed
        );
    }

    // This function also updates the state in StateManager for the flood
    public void SetThreatModifier(int threatModifier)
    {
        threatTokenTarget = new Vector2(tokenXPositions[threatModifier], threatTokenTarget.y);
        StateManager.Instance.currentThreatIncrement = threatModifier;
    }

    public void IncrementThreatModifier()
    {
        if (StateManager.Instance.currentThreatIncrement < 10)
        {
            SetThreatModifier(StateManager.Instance.currentThreatIncrement + 1);
        }
    }

    // Returns the SpaceType of the furthest reached spaces given a dice total
    public static BoardSpace.SpaceType getFloodReach(int diceTotal)
    {
        BoardSpace.SpaceType furthestReached = BoardSpace.SpaceType.Black;
        foreach (var (num, spaceType) in floodReach)
        {
            if (diceTotal >= num)
            {
                furthestReached = spaceType;
            }
        }
        return furthestReached;
    }
}
