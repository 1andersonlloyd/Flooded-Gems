using UnityEngine;
using System.Collections.Generic;
public enum TurnPhase
{
    WaitingForPlayerInput, // Phase for player input/AI waiting. Possible time to interrupt
    Cutscene, // Phase that is not possible to be interrupted
    Interrupted, // Suspended gameplay for an interrupt
    Ended, // Current turn is over, waiting to initiate the next turn/phase
    FloodWaitingForPlayerInput, // TODO: Might just have a flag instead of seperate enum values for flood
    FloodCutscene,
    FloodInterrupted
}
public class StateManager : MonoBehaviour
{
    public static StateManager Instance;
    
    // A predicted threat roll that will override end of round roll
    public (int, int) predictedFloodThreatRolls = (0, 0);
    public int currentThreatIncrement = 0;

    public List<PlayerController> players = new List<PlayerController>();
    public HumanController localPlayer;
    public int currentPlayerIndex = 0;
    public TurnPhase currentPhase = TurnPhase.WaitingForPlayerInput;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
        
    }

  
}
