using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using Unity.VisualScripting;


// This is the class responsible for managing the highest level of logic and control on a specific instance of the game.
// This class however will not be allowed to update most of its data directly, merely acting as a form of UIManager.
// It is not however responsible for making logic decisions for the state and must instead be the one to make requests to the HostGameManager through the NetworkManager
// This class should be prepared to send requests to the HostGameManager through the NetworkManager and recieve game state updates through the same path.
// This class will not know if the machine it is on is the host or not, and it must still communicate with the HostGameManager as if it was on a different system.
// If this instance is on the host's machine, it will be the source of game state data for the HostGameManager directly and must be prepared to share all relevant data.
// May need to implement a lock system to prevent the host from reading data at the same time it's being updated,
//  or consider making a GameState struct that has ALL important data and sending when the host asks
// Also, keep in mind that object references will not work across network, so IDs will need to be used to refer to specific players and such.

public enum TurnPhase
{
    Interruptable, // Phase for player input/AI waiting. Possible time to interrupt
    NonInterruptable, // Phase where actions are being executed, no interrupts
    Interrupted, // Suspended gameplay for an interrupt
    Ended, // Current turn is over, waiting to initiate the next turn/phase

}
public class LocalGameManager : MonoBehaviour
{
    public static LocalGameManager Instance { get; private set; }
    public HumanController localPlayer;
    public HumanController humanPrefab;
    public AIController aiPrefab;
    public BoardSpace startingSpace;
    public int actionsPerTurn = 3;
    public List<PlayerController> players = new List<PlayerController>();
    public int currentPlayerIndex = 0;

    // Interrupt/Phases System Variables
    public float waitTime = 5f;
    public bool waitInterrupted = false;
    public List<PlayerController> interruptingPlayers = new List<PlayerController>();
    public static Action<PlayerController> AddingPlayerToInterruptList;
    public static Action<PlayerController> RemovingPlayerFromInterruptList;
    public TurnPhase currentPhase // currentPhase is a wrapper for _currentPhase that allows all updates to be broadcasted through the event
    {
        get => _currentPhase;
        set
        {
            _currentPhase = value;
            OnCurrentPhaseChanged?.Invoke(_currentPhase);
        }
    }
    [SerializeField]
    private TurnPhase _currentPhase = TurnPhase.Interruptable;
    public static Action<TurnPhase> OnCurrentPhaseChanged;
    public static Action<PlayerController> StartingPlayerTurn;
    public static Action<PlayerController> EndingPlayerTurn;

    // Flood System Variables
    public (int, int) predictedFloodThreatRolls = (0, 0);
    public int currentThreatIncrement = 0;

    public PlayerController currentPlayer
    {
        get
        {
            return players[currentPlayerIndex]; // Probably change to host check
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

        InitializePlayers(1, 1);

        // TODO: Set flood values to default


        // Start first player's turn
        currentPlayerIndex = 0;

        
        UIManager.Instance.Initialize();



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
            players.Add(human);
            UIManager.Instance.AddPlayerPlaque(human, numHumans + numAI,i);


            // TODO: For now just setting the first player as the local player
            if (i == 0)
            {
                localPlayer = human;
            }

        }

        // Add ai players
        for (int i = numHumans; i < numHumans + numAI; i++)
        {
            string name = "CPU " + (i + 1 - numHumans);
            AIController ai = Instantiate(aiPrefab);
            ai.InitializePlayer(name, startingSpace, playerNumIncrement++);
            players.Add(ai);
            UIManager.Instance.AddPlayerPlaque(ai, numHumans + numAI,i);
        }
    }

    #endregion

    #region PlayerTurn
    // This function start the given player's turn
    public void StartTurn()
    {
        currentPhase = TurnPhase.Interruptable;
        // Trigger the start of the turn for the player directly
        players[currentPlayerIndex].StartTurn();
        // Send out event for turn start
        StartingPlayerTurn?.Invoke(currentPlayer);
    }
    public bool RequestMove(PlayerController player, BoardSpace targetSpace)
    {
        // Check if it is player's turn
        if (player != currentPlayer)
        {
            Debug.LogError("It is not player's turn");
            return false;
        }

        // // Check if it is player's phase (OUTDATED? THE AI WOULD HAVE CLAIMED PRIORITY AND POSSIBLY THE HUMANS TOO BY THIS POINT)
        // if (currentPhase != TurnPhase.Interruptable)
        // {
        //     Debug.LogError("It is not player's phase");
        //     return false;
        // }

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
            Debug.LogError("Request to end turn denied for " + player.playerName + ". It is not their turn.");
            return false;
        }

        if (currentPhase != TurnPhase.Interruptable)
        {
            Debug.LogError("Request to end turn denied for " + player.playerName + ". It is not the correct phase.");
            // return false;
        }

        Debug.Log("Request to end turn accepted for " + player.playerName);
        EndCurrrentPlayersTurn();

        return true;
    }

    public bool RequestToDig(PlayerController player)
    {
        // Check if player is allowed right now
        if (currentPlayer != player)
        {
            Debug.LogError("Request to dig denied for " + player.playerName + ". It is not their turn.");
            return false;
        }

        if (currentPhase != TurnPhase.Interruptable)
        {
            Debug.LogError("Request to dig denied for " + player.playerName + ". It is not the correct phase.");
            //return false;
        }

        if(player.actionsLeft < 1)
        {
            Debug.LogError("Request to dig denied for " + player.playerName + ". Not enough actions left to dig.");
            return false;
        }

        if(player.currentSpace.digSpot == null)
        {
            Debug.LogError("Request to dig denied for " + player.playerName + ". No digspot on current space.");
            return false;
        }
        Debug.Log("Request to dig accepted for " + player.playerName);




        return true;
    }

    public bool RequestToStash(PlayerController player)
    {
        if (currentPlayer != player)
        {
            Debug.LogError("Request to stash denied for " + player.playerName + ". It is not their turn.");
            return false;
        }

        if (currentPhase != TurnPhase.Interruptable)
        {
            Debug.LogError("Request to stash denied for " + player.playerName + ". It is not the correct phase.");
            //return false;
        }

        if(player.actionsLeft < 1)
        {
            Debug.LogError("Request to stash denied for " + player.playerName + ". Not enough actions left to dig.");
            return false;
        }

        if(player.stashSpot != null)
        {
            Debug.LogError("Request to stash denied for " + player.playerName + ". They already have a stash placed.");
            return false;
        }


        if(player.currentSpace.digSpot != null)
        {
            Debug.LogError("Request to stash denied for " + player.playerName + ". Already a digspot on current space.");
            return false;
        }
        Debug.Log("Request to stash accepted for " + player.playerName);
        return true;
    }



    // This function will be called by the player scripts
    private void EndCurrrentPlayersTurn()
    {
        players[currentPlayerIndex].actionsLeft = 0;
        currentPhase = TurnPhase.Ended;
        EndingPlayerTurn?.Invoke(players[currentPlayerIndex]);

        // Update local player specific stuff
        if (currentPlayer == localPlayer)
        {
            MoveButton.Instance.DisableMove();
        }

        // Increment the current turn index, if it is at the end of the round trigger flood phase, otherwise start next player's turn
        if (currentPlayerIndex < players.Count - 1)
        {
            currentPlayerIndex++;
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
        currentPhase = TurnPhase.Interruptable;
        yield return StartCoroutine(WaitForInteruptsCoroutineInline(1.0f));
        currentPhase = TurnPhase.NonInterruptable;

        Debug.Log("Starting Flood Phase");

        // Calculate Flood Threat
        List<int> threatResults = new List<int>{UnityEngine.Random.Range(1, 7), UnityEngine.Random.Range(1, 7)};
        UIManager.Instance.RollFloodThreatDice(threatResults);
        
        currentPhase = TurnPhase.Interruptable;
        yield return StartCoroutine(WaitForInteruptsCoroutineInline(1.0f));
        currentPhase = TurnPhase.NonInterruptable;

        Debug.Log("Flood Threat Roll: " + threatResults[0] + " + " + threatResults[1] + " + " + FloodThreatScale.Instance.getFloodThreatModifier() +
         " = " + (threatResults[0] + threatResults[1] + FloodThreatScale.Instance.getFloodThreatModifier()));

        // Wait for Interrupts
        currentPhase = TurnPhase.Interruptable;
        yield return StartCoroutine(WaitForInteruptsCoroutineInline(1.0f));
        currentPhase = TurnPhase.NonInterruptable;
        
        // React to threat result
        // Flood triggers
        if (threatResults[0] + threatResults[1] + FloodThreatScale.Instance.getFloodThreatModifier() >= 12)
        {            
            // Roll flood dice
            List<int> reachResults = new List<int>{UnityEngine.Random.Range(1, 7), UnityEngine.Random.Range(1, 7)};
            UIManager.Instance.RollFloodReachDice(reachResults);

            yield return new WaitForSeconds(1.0f);

            BoardSpace.SpaceType floodReach = FloodThreatScale.getFloodReach(reachResults[0] + reachResults[1]);
            Debug.Log("Flood Reach Roll: " + reachResults[0] + " + " + reachResults[1] + " = " + floodReach + " spaces reached");

            // Highlight flooded spaces
            MapManager.Instance.HighlightFloodedSpaces(floodReach, 0.1f);

            // Allow players to use items
            currentPhase = TurnPhase.Interruptable;
            yield return StartCoroutine(WaitForInteruptsCoroutineInline(1.0f));
            currentPhase = TurnPhase.NonInterruptable;

            // Flood the spaces
            MapManager.Instance.HighlightFloodedSpaces(floodReach, 0);

            // For each player, check if they are on a space that is flooded
            foreach (PlayerController player in players)
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
        currentPlayerIndex = 0;

        UIManager.Instance.HideFloodDiceTrays();

        // For now, just start the first player's turn
        StartTurn();
    }

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
    // A coroutine that waits for a specified amount of time, but can be interrupted. A callback function can be called at the end of the wait period.
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
            UIManager.Instance.SetWaitIndicatorPercentage(timeLeft / time);
        }
        UIManager.Instance.SetWaitIndicatorPercentage(0);
        callback?.Invoke();
    }

    public IEnumerator WaitForInteruptsCoroutineInline(float time)
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
            UIManager.Instance.SetWaitIndicatorPercentage(timeLeft / time);
        }
        UIManager.Instance.SetWaitIndicatorPercentage(0);
    }
    public void AddPlayerToInterruptList(PlayerController player)
    {
        // Add playe to list if not already
        if (!interruptingPlayers.Contains(player))
        {
            interruptingPlayers.Add(player);
            AddingPlayerToInterruptList?.Invoke(player);
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
            RemovingPlayerFromInterruptList?.Invoke(player);
        }
        // If no players are in the list, set the waitInterrupted to false
        if (interruptingPlayers.Count == 0)
        {
            waitInterrupted = false;
        }
    }

    public List<PlayerController> GetPlayerInterruptList()
    {
        return interruptingPlayers;
    }

    #endregion
}

