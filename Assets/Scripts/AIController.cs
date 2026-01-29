using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NUnit.Framework.Constraints;

public class AIController : PlayerController
{   
    
    public BoardSpace destinationSpace = null;
    int focusValue = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void StartTurn()
    {
        base.StartTurn();

        Debug.Log("Starting AI Turn");
        StartCoroutine(AiControllerTurnCoroutine());

    }
    
    public override void EndTurn()
    {

        base.EndTurn();
    }

    IEnumerator AiControllerTurnCoroutine()
    {
        // For now just have the ai choose a new space to move towards each round randomly.
        while (actionsLeft > 0){
            yield return new WaitForSeconds(1.0f);
            if(destinationSpace == null)
            {
                Debug.Log(name + "'s destination was null, so it choose one randomly without checks.");
                List<BoardSpace> spaces = MapManager.Instance.GetAllDigSpotSpaces();
                destinationSpace = spaces[UnityEngine.Random.Range(0, spaces.Count)];
                focusValue = 3;
            }

            if(focusValue <= 0 || aiSelectNewGoal || destinationSpace == null)
            {
                selectnewTargetSpace();
            }

            if(currentSpace == destinationSpace)
            {
                // Allows for waiting the flood out. The flood will trigger a new target space, which will break the target lock.
                if(currentSpace == MapManager.Instance.greenSpace)
                {
                    EndTurn();
                }

                if(currentSpace.digSpot != null)
                {
                    if (TurnManager.Instance.RequestToDig(this))
                    {
                        Debug.Log("Player " + playerName + " dug at " + currentSpace.name);
                    }
                }
            }else{
                moveTowardsDestination();
            }
        }
        

        EndTurn();
    }

    public override void ExecuteDig()
    {
        StartCoroutine(ExecuteDigCoroutine());
        focusValue -= 1;
        actionsLeft--;
    }

    protected override IEnumerator ExecuteDigCoroutine()
    {
        yield return base.ExecuteDigCoroutine();
    }

    // Logic for the ai to choose a new space it wants to move towards
    public void selectnewTargetSpace()
    {
        aiSelectNewGoal = false;

        // Place holder logic that will just wander
        // Pick a random space on the board
        //destinationSpace = MapManager.Instance.boardSpaces[UnityEngine.Random.Range(0, MapManager.Instance.boardSpaces.Count)];



        List<BoardSpace> digSpotSpaces = MapManager.Instance.GetAllDigSpotSpaces();
        List<BoardSpace> sortedList = digSpotSpaces
            .OrderBy(space => MapManager.Instance.GetDistanceToSpace(currentSpace, space)) // sort by path length
            .ToList();

        // A bunch of variables to keep track of priority values and important references
        BoardSpace greenSpace = MapManager.Instance.greenSpace;
        int? greenDistance = MapManager.Instance.GetDistanceToSpace(currentSpace, greenSpace);
        float greenPriority = 10;

        BoardSpace closestGemDigSpace = null;
        int? gemDistance = null;
        float gemPriority = 1;
        int gemDieGoal = 7;

        BoardSpace closestItemDigSpace = null;
        int? itemDistance = null;
        float itemPriority = 1;
        int itemDieGoal = 7;

        BoardSpace closestStashDigSpace = null;
        int? stashDistance = null;
        float stashPriority = 1;
        int stashValue = 0; // A value to set how good of a stash it is strategically to loot, i.e. if that player is close to winning or if it has gems the ai needs

        Dictionary<BoardSpace, float> spaceWeights = new Dictionary<BoardSpace, float>();

        // Find the closest of each type of dig spot, these are the only spaces considered + the green space
        foreach(BoardSpace space in sortedList){
            if(space.digSpot.digSpotType == DigSpot.DigSpotType.GemSpot && closestGemDigSpace == null &&
             space.digSpot != lastSuccessfulDigSpot && !inventory.HasGem(space.digSpot.gemType))
            {
                closestGemDigSpace = space;
                gemDistance = MapManager.Instance.GetDistanceToSpace(currentSpace, space);
                Debug.Log("Closest Gem Space found at distance " + gemDistance.Value);
            }else if (space.digSpot.digSpotType == DigSpot.DigSpotType.ItemSpot && closestItemDigSpace == null && space.digSpot != lastSuccessfulDigSpot)
            {
                closestItemDigSpace = space;
                itemDistance = MapManager.Instance.GetDistanceToSpace(currentSpace, space);
                Debug.Log("Closest Item Space found at distance " + itemDistance.Value);

            }else if(space.digSpot.digSpotType == DigSpot.DigSpotType.StashSpot && closestStashDigSpace == null && space.digSpot != lastSuccessfulDigSpot)
            {
                closestStashDigSpace = space;
                stashDistance = MapManager.Instance.GetDistanceToSpace(currentSpace, space);
                Debug.Log("Closest Stash Space found at distance " + stashDistance.Value);
            }
        }

        Debug.Log("Gem Priority: " + gemPriority + " | Item Priority: " + itemPriority + " | Stash Priority: " + stashPriority + " | Green Priority: " + greenPriority);

        // Start checking for specific things that impact priority

        // Assign a value to the closest stash to be used for priority calculations
        if (closestStashDigSpace)
        {
            // TODO: Calculate a value based on the contents of the stash, how far ahead the owner of the stash is, and if this ai has any items to help with it
            // Temp value:
            stashValue = 10; // Currently measured with how many spaces it is worth traveling for, more or less

        }

        // Look at raw distance
        if(closestGemDigSpace && gemDistance.HasValue)
        {
            gemDieGoal = closestGemDigSpace.digSpot.rollGoal;
            gemPriority += (4 - gemDistance.Value) * 10;
        }
        if(closestItemDigSpace && itemDistance.HasValue)
        {
            itemDieGoal = closestItemDigSpace.digSpot.rollGoal;
            itemPriority += (3 - itemDistance.Value) * 10 * (5 / itemDieGoal);
        }
        if(stashDistance.HasValue)
        {
            stashPriority += (4 - stashDistance.Value) * 10 * (10 / stashValue);
        }

        // Check if the itemspot is on the way to the gemspot
        if(gemDistance.HasValue && itemDistance.HasValue)
        {
            int distanceBetweenGemAndItem = MapManager.Instance.GetDistanceToSpace(closestGemDigSpace, closestItemDigSpace);
            // If not too far, give it priority
            if (distanceBetweenGemAndItem + itemDistance.Value < gemDistance.Value * 1.4)
            {
                itemPriority += 40 * (5 / itemDieGoal);
            }
            // If directly on the path, give it extra priority
            if(distanceBetweenGemAndItem + itemDistance.Value == gemDistance.Value)
            {
                itemPriority += 40 * (5 / itemDieGoal);
            }
        }

        int[] gemArray = inventory.GetGemArray();
        float x = (float)(gemArray[0] * 4f + gemArray[1] * 5f + gemArray[2] * 6.5f + gemArray[3] * 6.5f + gemArray[4] * 8 + gemArray[5] * 10f);

        // Check if able to win the game with current gems
        if (inventory.HasEveryGem())
        {
            greenPriority += 10000;
        }

        Debug.Log("Gem Priority: " + gemPriority + " | Item Priority: " + itemPriority + " | Stash Priority: " + stashPriority + " | Green Priority: " + greenPriority);

        // Assess flood risk and store each valid space into a dictionary for random weighted selection
        if(closestGemDigSpace && gemDistance.HasValue)
        {
            gemPriority *= AssessRiskOfSpace(closestGemDigSpace);
            spaceWeights.Add(closestGemDigSpace, gemPriority);
        }
        if(closestItemDigSpace && itemDistance.HasValue)
        {
            itemPriority *= AssessRiskOfSpace(closestItemDigSpace);
            spaceWeights.Add(closestItemDigSpace, itemPriority);
        }
        if(closestStashDigSpace && stashDistance.HasValue)
        {
            stashPriority *= AssessRiskOfSpace(closestStashDigSpace);
            spaceWeights.Add(closestStashDigSpace, stashPriority);
        }
        greenPriority *= AssessRiskOfSpace(greenSpace);
        spaceWeights.Add(greenSpace, greenPriority);

        Debug.Log("Gem Priority: " + gemPriority + " | Item Priority: " + itemPriority + " | Stash Priority: " + stashPriority + " | Green Priority: " + greenPriority);


        // Randomly select based on final weights
        float totalWeight = spaceWeights.Values.Sum();
        float r = UnityEngine.Random.value * totalWeight;

        float cumulative = 0f;
        foreach (var kvp in spaceWeights)
        {
            cumulative += kvp.Value;
            if (r <= cumulative)
            {
                destinationSpace = kvp.Key;
                break;
            }
        }
        
        Debug.Log("New Destination Space set for" + playerName + " : " + destinationSpace.name);

        // Set how many actions the ai wants to keep attempting this goal
        if(closestGemDigSpace == destinationSpace){
            focusValue = 15;
        }
        if(closestItemDigSpace == destinationSpace){
            focusValue = 3;
        }
        if(closestStashDigSpace == destinationSpace){
            focusValue = (int)Math.Floor(0.5 * stashValue);
        }
        if(greenSpace == destinationSpace){
            focusValue = 15;
        }
    }

    // Caluculates a value modifier for a space's priority based on risk and immenint flooding, outputs 0.0 - 1.0
    float AssessRiskOfSpace(BoardSpace space)
    {
        float T = (int)space.spaceType;
        float M = FloodThreatScale.Instance.getFloodThreatModifier();

        float urgency = M / 10;
        float y =  (1f - urgency) * T + urgency * (6 - T); //<-- ChatGPT Math
        return y / 5;
    }

    public bool moveTowardsDestination()
    {
        // Get spaces from neighbors that have the smallest distance to the target space
            BoardSpace targetNeighbor = null;
            int shortestDistance = 100;
            // Add all neighbors with shortest path to a list
            foreach (BoardSpace space in currentSpace.neighbors)
            {
                List<BoardSpace> path = MapManager.Instance.FindShortestPath(space, destinationSpace);
                if (path.Count < shortestDistance || path.Count == shortestDistance && space.spaceType < targetNeighbor.spaceType)
                {
                    shortestDistance = path.Count;
                    targetNeighbor = space;
                }
            }

        // Put in the request to move
        return TurnManager.Instance.RequestMove(this, targetNeighbor);
    }



}
