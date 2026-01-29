using UnityEngine;
using UnityEngine.UI;

public class MoveButton : MonoBehaviour
{
    Button button;
    public static MoveButton Instance;
    private Image buttonImage;

    public Color normalColor = Color.white;
    public Color pressedColor = Color.red;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        // Get the Image component attached to this GameObject
        buttonImage = GetComponent<Image>();
        // Set the initial color of the button
        UpdateButtonVisual();
        // Get the Button component attached to this GameObject
        button = GetComponent<Button>();
        button.onClick.AddListener(HandleClick);
    }
    // Update is called once per frame
    void Update()
    {

    }
    void HandleClick()
    {
        Debug.Log("Button clicked!");

        // Check StateManager to see if current player is local player
        if (StateManager.Instance.localPlayer != TurnManager.Instance.currentPlayer)
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
        if (StateManager.Instance.localPlayer != null)
        {
            HumanController localPlayer = StateManager.Instance.localPlayer;
            localPlayer.moveInputEnabled = true;
            UpdateButtonVisual();
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
        if (StateManager.Instance.localPlayer != null)
        {
            StateManager.Instance.localPlayer.moveInputEnabled = false;
            UpdateButtonVisual();
            MapManager.Instance.ClearHighlights();
            MapManager.Instance.DisableSprites();
        }
        else
        {
            Debug.LogError("Local player is null!");
        }
    }

    public void ToggleMove()
    {
        if (StateManager.Instance.localPlayer != null)
        {
            if (StateManager.Instance.localPlayer.moveInputEnabled)
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

    public void UpdateButtonVisual()
    {
        if (buttonImage != null && StateManager.Instance.localPlayer != null)
        {
            buttonImage.color = StateManager.Instance.localPlayer.moveInputEnabled ? pressedColor : normalColor;
        }
    }
}