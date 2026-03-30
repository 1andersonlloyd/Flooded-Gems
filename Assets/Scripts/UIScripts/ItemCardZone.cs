using System.Collections.Generic;
using UnityEngine;

public class ItemCardZone : MonoBehaviour
{
    PlayerController owningPlayer;
    List<ItemCard> itemCards = new List<ItemCard>();
    public ItemCard itemCardPrefab;
    RectTransform rectTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Initialize()
    {
        rectTransform = GetComponent<RectTransform>();
        owningPlayer = LocalGameManager.Instance.localPlayer;
        owningPlayer.inventory.ItemAdded += OnItemAdded;
        owningPlayer.inventory.ItemRemoved += OnItemRemoved;
    }



    void OnItemAdded(PlayerController player, Item item)
    {
        if(player != owningPlayer) return;
            
        ItemCard newCard = Instantiate(itemCardPrefab, transform);

        RectTransform rt = newCard.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector3(0, 91.65f, 0);
        rt.localScale = new Vector3(0.66f, 0.66f, 0.66f);

        itemCards.Add(newCard);
    }

    void OnItemRemoved(PlayerController player, Item item) // What happens if there are duplicate item cards
    {
        if(player != owningPlayer) return;
    
    
    
    }
}
