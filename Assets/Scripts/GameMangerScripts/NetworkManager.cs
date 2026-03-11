using UnityEngine;





// This class is responsible for all communication to the HostGameManager, either from the host's own LocalGameManager or from other client's LocalGameManagers. 
// All requests will be sent to this class and privately either just sent to the local HostGameManager if its on the same machine, or sent to the machine that has it
// Requests across machines will be sent to the other machine's NetworkingManager first before being sent to the destination, 
//  this means most actions will have a send and recieve function in this class.

public class NetworkManager : MonoBehaviour
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
