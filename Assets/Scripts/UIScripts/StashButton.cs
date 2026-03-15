using UnityEngine;
using UnityEngine.UI;

public class StashButton : MonoBehaviour
{
    Button button;


    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }
    }

    void Start()
    {
        UIManager.Instance.SetButtonColors(button);
        UIManager.UpdateAllButtonVisuals += UpdateButtonVisuals;
        UpdateButtonVisuals();
    }

    void HandleClick()
    {

        // For the time being however just stash all held gems
        HumanController player = LocalGameManager.Instance.localPlayer;

        // TODO: Have a menu UI pop-up or slider to select the number of gems specifically to pass instead of just putting them all
        Debug.Log("Stashbutton clicked");
        player.StashButtonSubmitted(player.inventory.GetGemArray()); // Prototyping code, may have to move StashButtonSubmitted to a different function after choosing gems
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
