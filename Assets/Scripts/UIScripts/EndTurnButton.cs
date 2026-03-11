using UnityEngine;
using UnityEngine.UI;

public class EndTurnButton : MonoBehaviour
{
    Button button;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the Button component attached to this GameObject
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }
        UIManager.Instance.SetButtonColors(button);
        UIManager.UpdateAllButtonVisuals += UpdateButtonVisuals;
    }

    void HandleClick()
    {
        if (LocalGameManager.Instance.RequestToEndTurn(LocalGameManager.Instance.localPlayer))
        {
            //Debug.Log("Request to end turn was accepted");
        }
        else
        {
            Debug.Log("Request to end turn was rejected");
        }
    }

    void UpdateButtonVisuals()
    {
        button.interactable = LocalGameManager.Instance.currentPlayer == LocalGameManager.Instance.localPlayer && UIManager.Instance.ActiveButtonCheck(button);
    }
}
