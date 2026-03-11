using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public static Canvas Canvas;

    public GameObject actionBar;
    public FloodThreatScale floodThreatScale;

    public RectTransform playerPlaqueBar;
    public PlayerPlaque playerPlaquePrefab;
    public List<PlayerPlaque> playerPlaques = new List<PlayerPlaque>();
    Dictionary<PlayerController, PlayerPlaque> plaqueDictionary = new Dictionary<PlayerController, PlayerPlaque>();

    Color normalColor = Color.white;
    Color pressedColor = Color.gray;
    Color disabledColor = new Color(128f/255f, 128f/255f, 128f/255f);
    public Button activeButton = null;
    public static Action UpdateAllButtonVisuals;

#region Initialization
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        Canvas = GetComponent<Canvas>();
        PlayerController.PlayerMoving += OnPlayerMove;
        PlayerController.FinishedDigRoll += OnPlayerFinishedDigRoll;
        LocalGameManager.StartingPlayerTurn += OnStartingPlayerTurn;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerController.FinishedDigRoll += HidePlayerPlaqueDiceTray;

    }

    public void AddPlayerPlaque(PlayerController player, int numberPlayers, int playerID)
    {
        List<float> plaqueYCoords;
        switch (numberPlayers)
        {
            case 2:
                plaqueYCoords = new List<float> { 0.75f, 0.25f };

            break;
            case 3:
                plaqueYCoords = new List<float> { 0.8333f, 0.5f, 0.1666f };
            break;

            case 4:
                plaqueYCoords = new List<float> { 0.875f, 0.625f, 0.375f, 0.125f };
            break;
            default:
                Debug.LogError("Unsupported number of players. " + numberPlayers + " players passed.");
                return;
        }
        float plaqueYCoord = plaqueYCoords[playerID];

        // Instantiate plaque as a child of the rectTransform object
        PlayerPlaque plaque = Instantiate(playerPlaquePrefab, playerPlaqueBar);
        RectTransform plaqueRectTransform = plaque.GetComponent<RectTransform>();
        plaqueRectTransform.anchoredPosition = new Vector3(0, plaqueYCoord * playerPlaqueBar.rect.height, 0);
        plaqueRectTransform.anchorMax = new Vector2(0.5f, 0);
        plaqueRectTransform.anchorMin = new Vector2(0.5f, 0);
        playerPlaques.Add(plaque);
        plaque.Initialize(player, playerID, player.playerName);

        // Add plaque to the dictionary for easier lookup
        plaqueDictionary.Add(player, plaque);
    }
#endregion Initialization
#region Plaque Actions
    public PlayerPlaque GetPlayerPlaque(PlayerController player)
    {
        foreach(PlayerPlaque plaque in playerPlaques)
        {
            if(plaque.player == player)
            {
                return plaque;
            }
        }
        Debug.LogError("Could not find plaque for player " + player.playerName);
        return null;
    }

    public void RollPlayerDie(PlayerController player, int forcedDieValue)
    {
        PlayerPlaque plaque = plaqueDictionary[player];
        plaque.RollDie(forcedDieValue);

    }

    void HidePlayerPlaqueDiceTray(PlayerController player)
    {
        plaqueDictionary[player].HideDiceTray();
    }

#endregion Plaque Actions
#region Button Logic
    void OnPlayerMove(PlayerController player, BoardSpace space)
    {
        if(player == LocalGameManager.Instance.localPlayer)
        {
            UpdateAllButtonVisuals?.Invoke();
        }
    }
    void OnPlayerFinishedDigRoll(PlayerController player)
    {
        if(player == LocalGameManager.Instance.localPlayer)
        {
            UpdateAllButtonVisuals?.Invoke();
        }
    }
    void OnStartingPlayerTurn(PlayerController player)
    {
        if(player == LocalGameManager.Instance.localPlayer)
        {
            UpdateAllButtonVisuals?.Invoke();
        }
    }
    public void SetButtonColors(Button button)
    {
        ColorBlock colorBlock = button.colors;
        colorBlock.normalColor = normalColor;
        colorBlock.pressedColor = pressedColor;
        colorBlock.disabledColor = disabledColor;
        button.colors = colorBlock;
    }

    public void ClaimActiveButton(Button button)
    {
        if(activeButton == null)
        {
            activeButton = button;
            UpdateAllButtonVisuals?.Invoke();
        }
        else
        {
            Debug.LogError("Cannot claim active button state for " + button.name + ". Button " + activeButton.name + " is already claiming the state.");
        }
    }
    public void ReleaseActiveButton(Button button)
    {
        if(activeButton == button)
        {
            activeButton = null;
            UpdateAllButtonVisuals?.Invoke();
        }
        else
        {
            Debug.LogError("Cannot release active button state for " + button.name + ", it does not have claim over it right now.");
        }
    }


    // Checks to see if the passed button is allowed to be active by seeing if the active button is null or equals the passed button
    public bool ActiveButtonCheck(Button button)
    {
        return button == activeButton || activeButton == null;
    }
#endregion Button Logic
}
