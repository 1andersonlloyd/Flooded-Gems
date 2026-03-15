using UnityEngine;
using UnityEngine.UI;

public class DigButton : MonoBehaviour
{
    Button button;

    void Awake()
    {
        // Get the Button component attached to this GameObject
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }

        UIManager.UpdateAllButtonVisuals += UpdateButtonVisuals;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UIManager.Instance.SetButtonColors(button);
    }
    // Send the click request to the human local player to assess and perform
    private void HandleClick()
    {
        LocalGameManager.Instance.localPlayer.DigButtonClicked();
    }

    void UpdateButtonVisuals()
    {
        PlayerController player = LocalGameManager.Instance.localPlayer;
        if(player == null )
        {
            return;
        }

        DigSpot currentDigSpot = player.currentSpace?.digSpot;
        button.interactable = currentDigSpot != null && (currentDigSpot != player.lastSuccessfulDigSpot || currentDigSpot == player.stashSpot) && player.actionsLeft > 0 && UIManager.Instance.ActiveButtonCheck(button);
    }

}
