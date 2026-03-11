using UnityEngine;
using UnityEngine.UI;

public class MoveButton : MonoBehaviour
{
    Button button;
    public static MoveButton Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        button = GetComponent<Button>();
        UIManager.Instance.SetButtonColors(button);
        
        button.onClick.AddListener(HandleClick);
        PlayerController.PlayerMoving += PlayerMovingListener;
        UIManager.UpdateAllButtonVisuals += UpdateButtonVisuals;
        UpdateButtonVisuals();
    }
    // Update is called once per frame
    void Update()
    {

    }
    void HandleClick()
    {
        Debug.Log("Button clicked!");

        // Check StateManager to see if current player is local player
        if (LocalGameManager.Instance.localPlayer != LocalGameManager.Instance.currentPlayer)
        {
            Debug.Log("Not local player's turn!");

            // TODO: Last player can interupt flood phase to move
            // TODO: Player can interupt maybe during wait times on their own turn to do move, like after a dig result, so fix

            return;
        }

        ToggleMove();
    }

    public void EnableMove()
    {
        if (LocalGameManager.Instance.localPlayer != null)
        {
            HumanController localPlayer = LocalGameManager.Instance.localPlayer;
            localPlayer.moveInputEnabled = true;

            UIManager.Instance.ClaimActiveButton(button);

            UpdateButtonVisuals();
            MapManager.Instance.ClearHighlights();
            MapManager.Instance.HighlightNeighborsInRadius(localPlayer.currentSpace, localPlayer.actionsLeft);
            MapManager.Instance.displayHoverSprites = true;
        }
        else
        {
            Debug.LogError("Local player is null!");
        }
    }

    public void DisableMove()
    {
        if (LocalGameManager.Instance.localPlayer != null)
        {
            if(LocalGameManager.Instance.localPlayer.moveInputEnabled){
                LocalGameManager.Instance.localPlayer.moveInputEnabled = false;
                UIManager.Instance.ReleaseActiveButton(button);
                UpdateButtonVisuals();
                MapManager.Instance.ClearHighlights();
                MapManager.Instance.DisableSprites();
            }
        }
        else
        {
            Debug.LogError("Local player is null!");
        }
    }

    public void ToggleMove()
    {
        if (LocalGameManager.Instance.localPlayer != null)
        {
            if (LocalGameManager.Instance.localPlayer.moveInputEnabled)
            {
                DisableMove();
            }
            else
            {
                EnableMove();
            }


        }
        else
        {
            Debug.LogError("Local player is null!");
        }
    }

    void PlayerMovingListener(PlayerController player, BoardSpace space)
    {
        if(player == LocalGameManager.Instance.localPlayer)
        {
            UpdateButtonVisuals();
        }
    }
    public void UpdateButtonVisuals()
    {
        if (button != null && LocalGameManager.Instance.localPlayer != null)
        {
            button.interactable = LocalGameManager.Instance.localPlayer.actionsLeft > 0 && UIManager.Instance.ActiveButtonCheck(button);
        }
    }
}