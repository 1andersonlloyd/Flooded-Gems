using System.Collections.Generic;
using UnityEngine;

public class CurrentPlayerMarker : MonoBehaviour
{       
    RectTransform rectTransform;
    // Dictionary<PlayerController, PlayerPlaque> playerPlaques = new Dictionary<PlayerController, PlayerPlaque>();
    public Vector2 targetPosition = Vector2.zero;

    public void Awake()
    {
        TurnManager.StartingPlayerTurn += OnPlayerTurnStart;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        // Lerp towards positon
        if(rectTransform.anchoredPosition != targetPosition){
            rectTransform.anchoredPosition = Vector3.Lerp(rectTransform.anchoredPosition, targetPosition, Time.deltaTime * 5);
            // Snap to position if close
            if(Vector3.Distance(rectTransform.anchoredPosition, targetPosition) < 0.5f)
            {
                rectTransform.anchoredPosition = targetPosition;
            }
        }
    }

    void OnPlayerTurnStart(PlayerController player)
    {
        // Get the rect transform of the player plaque
        RectTransform plaqueRect = UIManager.Instance.GetPlayerPlaque(player).GetComponent<RectTransform>();
        targetPosition = plaqueRect.anchoredPosition + (plaqueRect.rect.width / 2 * new Vector2(-1,0));
        transform.SetAsLastSibling();

    }
}
