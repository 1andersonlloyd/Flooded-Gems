using System.Collections.Generic;
using UnityEngine;

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

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        Canvas = GetComponent<Canvas>();
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
}
