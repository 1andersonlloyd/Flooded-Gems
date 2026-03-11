using UnityEngine;


// This class is responsible for accepting requests from all game instances, performing game logic, and then sending out updates to all instances to update game states.
// This class will only be communicated to and from through the NetworkManager class, even if it is on the same instance as the LocalGameManager it is communicating with.
// This class will however be directly querying information from the present LocalGameManager for game state when making decisions for requests, 
//  however it will not be allowed to update the LocalGameManager this way.
// A lot of this class's logic will come from what used to be the StateManager.


public class HostGameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
