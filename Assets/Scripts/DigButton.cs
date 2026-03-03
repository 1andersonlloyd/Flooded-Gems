using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class DigButton : MonoBehaviour
{
    Color normalColor = Color.white;
    Color disabledColor = new Color(128f/255f, 128f/255f, 128f/255f);

    private Image buttonImage;

    void Awake()
    {
        // Get the Button component attached to this GameObject
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }
        buttonImage = GetComponent<Image>();

        PlayerController.PlayerMoving += OnPlayerMove;
        PlayerController.FinishedDigRoll += OnPlayerFinishedDigRoll;
        TurnManager.StartingPlayerTurn += OnStartingPlayerTurn;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    

    }
    // Send the click request to the human local player to assess and perform
    private void HandleClick()
    {
        StateManager.Instance.localPlayer.DigButtonClicked();
    }

    void OnPlayerMove(PlayerController player, BoardSpace space)
    {
        if(player == StateManager.Instance.localPlayer)
        {
            UpdateButtonVisuals();
        }
    }
    void OnPlayerFinishedDigRoll(PlayerController player)
    {
        if(player == StateManager.Instance.localPlayer)
        {
            UpdateButtonVisuals();
        }
    }

    void OnStartingPlayerTurn(PlayerController player)
    {
        if(player == StateManager.Instance.localPlayer)
        {
            UpdateButtonVisuals();
        }
    }
    void UpdateButtonVisuals()
    {
        PlayerController player = StateManager.Instance.localPlayer;
        if(player == null )
        {
            return;
        }

        DigSpot currentDigSpot = player.currentSpace?.digSpot;
        buttonImage.color = (currentDigSpot != null && currentDigSpot != player.lastSuccessfulDigSpot && player.actionsLeft > 0) ? normalColor : disabledColor;
    }

}
