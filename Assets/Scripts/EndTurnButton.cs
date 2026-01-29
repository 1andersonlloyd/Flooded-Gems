using UnityEngine;
using UnityEngine.UI;

public class EndTurnButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the Button component attached to this GameObject
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }
    }

    void HandleClick()
    {
        if (TurnManager.Instance.RequestToEndTurn(StateManager.Instance.localPlayer))
        {
            //Debug.Log("Request to end turn was accepted");
        }
        else
        {
            Debug.Log("Request to end turn was rejected");
        }
    }
}
