using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using Unity.VisualScripting;

// NOTE: I will be implementing a host sand client based system. This means that the host will perform all calcuations and processes
// and the remote players will be basically input machines that send their requested action to the host. The host will do the stuff, and then tell the remote
// players how to update their board.



public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    public HumanController humanPrefab;
    public AIController aiPrefab;
    public ButtonBar currentTurnButtonBar;
    public DigButton digButton; 

    public BoardSpace startingSpace;

    public float waitTime = 5f;
    public bool waitInterrupted = false;
    public List<PlayerController> interruptingPlayers = new List<PlayerController>();
    public WaitIndicator waitIndicator;
    public DiceTray floodThreatDiceTray;
    public DiceTray floodReachDiceTray;

    public Dice die1;
    public Dice die2;

    // Delete ?
    public bool setLocalPlayerMoveEnabled(bool value)
    {
        if (StateManager.Instance.localPlayer == null)
        {
            return false;
        }
        StateManager.Instance.localPlayer.moveInputEnabled = value;
        return true;
    }

    public PlayerController currentPlayer
    {
        get
        {
            return StateManager.Instance.players[StateManager.Instance.currentPlayerIndex];
        }
    }

    #region Initialize
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeGame();
    }


    public void InitializeGame()
    {
        // TODO: Add some menu or something here for deciding players and hosting?

        InitializePlayers(1, 2);

        // TODO: Set flood values to default


        // Start first player's turn
        StateManager.Instance.currentPlayerIndex = 0;
        StartTurn();
    }

    public void InitializePlayers(int numHumans, int numAI)
    {
        int playerNumIncrement = 0;
        if (numHumans < 1 || numAI < 0)
        {
            Debug.LogError("Invalid number of players");
            return;
        }

        // Add human players
        for (int i = 0; i < numHumans; i++)
        {
            string name = "Human " + (i + 1);
            HumanController human = Instantiate(humanPrefab);
            human.InitializePlayer(name, startingSpace, playerNumIncrement++);

            StateManager.Instance.players.Add(human);
            

            // TODO: For now just setting the first player as the local player
            if (i == 0)
            {
                StateManager.Instance.localPlayer = human;
            }

        }

        // Add ai players
        for (int i = 0; i < numAI; i++)
        {
            string name = "CPU " + (i + 1);
            AIController ai = Instantiate(aiPrefab);
            ai.InitializePlayer(name, startingSpace, playerNumIncrement++);
            StateManager.Instance.players.Add(ai);

        }

    }
    #endregion

    #region PlayerTurn
    // This function start the given player's turn
    public void StartTurn()
    {
        if (currentPlayer == StateManager.Instance.localPlayer)
        {
            currentTurnButtonBar.Show();
        }
        StateManager.Instance.players[StateManager.Instance.currentPlayerIndex].StartTurn();
    }
    public bool RequestMove(PlayerController player, BoardSpace targetSpace)
    {
        // Check if it is player's turn
        if (player != currentPlayer)
        {
            Debug.LogError("It is not player's turn");
            return false;
        }

        // Check if it is player's phase
        if (StateManager.Instance.currentPhase != TurnPhase.WaitingForPlayerInput)
        {
            Debug.LogError("It is not player's phase");
            return false;
        }

        // Check to see if player is already on the targetSpace
        if (player.currentSpace == targetSpace)
        {
            Debug.LogError("Player is already on target space");
            return false;
        }

        List<BoardSpace> path = MapManager.Instance.FindShortestPath(player.currentSpace, targetSpace);

        // Check if player has enough actions left to reach the target space
        if (path.Count - 1 > player.actionsLeft)
        {
            Debug.LogError("Not enough actions to reach target space");
            return false;
        }

        // Move the player to space TODO: Also update remote machine
        player.ExecuteMove(path);


        return true;
    }

    public bool RequestToEndTurn(PlayerController player)
    {
        if (currentPlayer != player)
        {
            Debug.LogError("It is not player's turn");
            return false;
        }

        if (StateManager.Instance.currentPhase != TurnPhase.WaitingForPlayerInput)
        {
            Debug.LogError("It is not player's phase");
            return false;
        }

        EndTurn();

        return true;
    }

    public bool RequestToDig(PlayerController player)
    {
        // Check if player is allowed right now
        if (currentPlayer != player)
        {
            Debug.LogError("It is not player's turn");
            return false;
        }

        if (StateManager.Instance.currentPhase != TurnPhase.WaitingForPlayerInput)
        {
            Debug.LogError("It is not player's phase");
            //return false;
        }

        if(player.actionsLeft < 1)
        {
            Debug.LogError("Not enough actions to dig");
            return false;
        }
        
        player.ExecuteDig();

        return true;
    }

    // This function will be called by the player scripts
    private void EndTurn()
    {
        currentTurnButtonBar.Hide();
        StateManager.Instance.currentPhase = TurnPhase.Ended;

        // Update local player specific stuff
        if (currentPlayer == StateManager.Instance.localPlayer)
        {
            MoveButton.Instance.DisableMove();
        }

        // Increment the current turn index, if it is at the end of the round trigger flood phase, otherwise start next player's turn
        if (StateManager.Instance.currentPlayerIndex < StateManager.Instance.players.Count - 1)
        {
            StateManager.Instance.currentPlayerIndex++;
            StartTurn();
        }
        else
        {
            // At end of round, go through the flood phase
            StartCoroutine(FullFloodPhaseCoroutine());
        }
    }
    #endregion



    #region FloodPhase


    IEnumerator FullFloodPhaseCoroutine()
    {
        yield return new WaitForSeconds(1.0f);

        Debug.Log("Starting Flood Phase");

        floodThreatDiceTray.Show();

        // Calculate Flood Threat
        List<int> results = floodThreatDiceTray.RollDice(2, new List<int>{});
        
        yield return new WaitForSeconds(1.0f);
        Debug.Log("Flood Threat Roll: " + results[0] + " + " + results[1] + " + " + FloodThreatScale.Instance.getFloodThreatModifier() +
         " = " + (results[0] + results[1] + FloodThreatScale.Instance.getFloodThreatModifier()));

        // Wait for Interrupts
        yield return StartCoroutine(WaitForInteruptsCoroutine(waitTime, null));

        // React to threat result
        // Flood triggers
        if (results[0] + results[1] + FloodThreatScale.Instance.getFloodThreatModifier() >= 12)
        {
            // Trigger Flood
            floodReachDiceTray.Show();
            // Roll flood dice
            results = floodReachDiceTray.RollDice(2, new List<int>{});
            yield return new WaitForSeconds(1.0f);

            BoardSpace.SpaceType floodReach = FloodThreatScale.getFloodReach(results[0] + results[1]);
            Debug.Log("Flood Reach Roll: " + results[0] + " + " + results[1] + " = " + floodReach + " spaces reached");

            // Highlight flooded spaces
            MapManager.Instance.HighlightFloodedSpaces(floodReach, 0.1f);

            // Allow players to use items
            yield return StartCoroutine(WaitForInteruptsCoroutine(waitTime, null));

            // Flood the spaces
            MapManager.Instance.HighlightFloodedSpaces(floodReach, 0);

            // For each player, check if they are on a space that is flooded
            foreach (PlayerController player in StateManager.Instance.players)
            {
                if ((int)player.currentSpace.spaceType >= (int)floodReach)
                {
                    Debug.Log(player.name + " is on a space that is flooded");
                    // Flood the player
                    FloodOut(player);
                }
                player.aiSelectNewGoal = true;
            }

            FloodThreatScale.Instance.SetThreatModifier(0);
            yield return new WaitForSeconds(2.0f);

        }
        else
        {
            FloodThreatScale.Instance.IncrementThreatModifier();
            
        }
        // End the flood phase
        MapManager.Instance.ClearHighlights();
        StateManager.Instance.currentPlayerIndex = 0;

        floodReachDiceTray.Hide();
        floodThreatDiceTray.Hide();

        // For now, just start the first player's turn
        StartTurn();
    }

    // Old prototype, seems bad
    // public void StartFloodPhase()
    // {
    //     Debug.Log("Starting Flood Phase");
    //     // TODO: Implement flood phase logic, possibly in its own gameObject

    //     // Calculate Flood Threat
    //     int roll1 = die1.rollDie(0);
    //     int roll2 = die2.rollDie(0);
    //     Debug.Log("Flood Threat Roll: " + roll1 + " + " + roll2 + " + " + FloodThreatScale.Instance.getFloodThreatModifier() + " = " + (roll1 + roll2 + FloodThreatScale.Instance.getFloodThreatModifier()));
    //     currentTotalDiceRoll = roll1 + roll2 + FloodThreatScale.Instance.getFloodThreatModifier();


    //     // Allow players to use items
    //     WaitForInterupts(waitTime, FloodThreatPhase);
    // }
    // public void FloodThreatPhase()
    // {

    //     if (currentTotalDiceRoll >= 12)
    //     {
    //         // Trigger Flood
    //         int roll1 = die1.rollDie(0);
    //         int roll2 = die2.rollDie(0);
    //         currentTotalDiceRoll = roll1 + roll2;
    //         BoardSpace.SpaceType floodReach = FloodThreatScale.getFloodReach(currentTotalDiceRoll);
    //         Debug.Log("Flood Reach Roll: " + roll1 + " + " + roll2 + " = " + floodReach + " spaces reached");

    //         // Highlight flooded spaces
    //         MapManager.Instance.HighlightFloodedSpaces(floodReach, 0.1f);

    //         // Allow players to use items
    //         StartCoroutine(WaitForInteruptsCoroutine(waitTime, FloodIslandPhase));
    //     }
    //     else
    //     {
    //         FloodThreatScale.Instance.IncrementThreatModifier();
    //         EndFloodPhase();
    //     }



    // }
    // void FloodIslandPhase()
    // {

    //     BoardSpace.SpaceType floodReach = FloodThreatScale.getFloodReach(currentTotalDiceRoll);
    //     MapManager.Instance.HighlightFloodedSpaces(floodReach, 0);

    //     // For each player, check if they are on a space that is flooded
    //     foreach (PlayerController player in StateManager.Instance.players)
    //     {
    //         if ((int)player.currentSpace.spaceType >= (int)floodReach)
    //         {
    //             Debug.Log(player.name + " is on a space that is flooded");
    //             // Flood the player
    //             FloodOut(player);
    //         }
    //     }

    //     FloodThreatScale.Instance.SetThreatModifier(0);
    //     StartCoroutine(WaitForInteruptsCoroutine(waitTime, EndFloodPhase));
    // }

    // private void EndFloodPhase()
    // {
    //     MapManager.Instance.ClearHighlights();
    //     StateManager.Instance.currentPlayerIndex = 0;

    //     // For now, just start the first player's turn
    //     StartTurn();
    // }

    public void FloodOut(PlayerController player)
    {
        // Find a random space that is not flooded
        BoardSpace space = MapManager.Instance.GetRandomBoardSpaceOfType(BoardSpace.SpaceType.Black);

        // "Despawn" player
        Vector3 direction = (player.currentSpace.transform.position - MapManager.Instance.transform.position).normalized;
        Vector3 despawnPosition = MapManager.Instance.transform.position + direction * 20f;
        player.targetPosition = despawnPosition;
        player.targetAlpha = 0f;

        // Move the player to that space
        StartCoroutine(Respawn(2, player, space));

        // TODO: OBLITERATE THEIR GEMS

        // TODO: Make cool animation

    }

    IEnumerator Respawn(float time, PlayerController player, BoardSpace space)
    {
        yield return new WaitForSeconds(time);
        // Take the vector from the center of the map to the desired space, normalize it, add it to the space's position, and then set the player's position to that result
        Vector3 direction = (space.transform.position - MapManager.Instance.transform.position).normalized;
        Vector3 respawnPosition = MapManager.Instance.transform.position + direction * 20f;
        player.transform.position = respawnPosition;
        player.targetAlpha = 1.0f;
        player.ExecuteJump(space);
    }
    #endregion


    #region WaitPhase
    public void WaitForInterupts(float time, Action callback)
    {
        StartCoroutine(WaitForInteruptsCoroutine(time, callback));
    }
    public IEnumerator WaitForInteruptsCoroutine(float time, Action callback)
    {
        float timeLeft = time;
        while (timeLeft > 0)
        {
            if (waitInterrupted)
            {
                timeLeft = time;
                yield return new WaitUntil(() => !waitInterrupted);
            }

            yield return null;
            timeLeft -= Time.deltaTime;
            waitIndicator.SetPercentage(timeLeft / time);
        }
        waitIndicator.SetPercentage(0);
        callback?.Invoke();
    }

    private IEnumerator WaitForInteruptsCoroutineInline(float time)
    {
        Debug.Log("Waiting for interrupts");
        float timeLeft = time;
        while (timeLeft > 0)
        {
            if (waitInterrupted)
            {
                timeLeft = time;
                yield return new WaitUntil(() => !waitInterrupted);
            }

            yield return null;
            timeLeft -= Time.deltaTime;
            waitIndicator.SetPercentage(timeLeft / time);
        }
        waitIndicator.SetPercentage(0);
    }
    public void AddPlayerToInterruptList(PlayerController player)
    {
        // Add playe to list if not already
        if (!interruptingPlayers.Contains(player))
        {
            interruptingPlayers.Add(player);
        }
        // If any players are in the list, set the waitInterrupted to true
        if (interruptingPlayers.Count > 0)
        {
            waitInterrupted = true;
        }
    }
    public void RemovePlayerFromInterruptList(PlayerController player)
    {
        // Remove player from list if it is in the list
        if (interruptingPlayers.Contains(player))
        {
            interruptingPlayers.Remove(player);
        }
        // If no players are in the list, set the waitInterrupted to false
        if (interruptingPlayers.Count == 0)
        {
            waitInterrupted = false;
        }
    }
    #endregion
}

