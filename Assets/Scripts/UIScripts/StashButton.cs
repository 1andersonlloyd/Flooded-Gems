using UnityEngine;
using UnityEngine.UI;

public class StashButton : MonoBehaviour
{
    Button button;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        UIManager.Instance.SetButtonColors(button);
        UIManager.UpdateAllButtonVisuals += UpdateButtonVisuals;
        UpdateButtonVisuals();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateButtonVisuals()
    {
        PlayerController player = LocalGameManager.Instance.localPlayer;
        if(player == null )
        {
            return;
        }

        DigSpot currentDigSpot = player.currentSpace?.digSpot;
        if (button != null && LocalGameManager.Instance.localPlayer != null)
        {
            button.interactable = LocalGameManager.Instance.localPlayer.actionsLeft > 0 && UIManager.Instance.ActiveButtonCheck(button) && !currentDigSpot;
            // TODO: Add a check to see if the player is allowed to stash right now to this logic
        }
    }


}
