using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerPlaque : MonoBehaviour
{

    [SerializeField]
    protected Image plaqueImage;
    [SerializeField]
    protected TextMeshProUGUI playerName;
    [SerializeField]
    protected Image itemBagSprite;
    [SerializeField]
    protected TextMeshProUGUI itemCounter;
    [SerializeField]
    protected Image stashSprite;
    [SerializeField]
    protected GemsUI gemInventoryUI;


    PlayerController player;




    [SerializeField]
    protected List<Sprite> gemSprites;
    [SerializeField]
    protected List<Sprite> plaqueSprites;
    [SerializeField]
    protected List<Sprite> stashSprites;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public void Initialize(PlayerController player, int playerColor, string name)
    {
        this.player = player;
        player.inventory.onInventoryChanged += InventoryChanged;
        SetPlayerColor(playerColor);
        SetName(name);
    }

    public void SetName(string name)
    {
        if (name.Length > 15)
        {
            name = name.Substring(0, 15);
        }
        playerName.text = name;
    }
    public void SetPlayerColor(int color)
    {
        plaqueImage.sprite = plaqueSprites[color];
        stashSprite.sprite = stashSprites[color];
        Debug.Log(color + " ID");
    }

#region GemsUI
    public void SetAllGemQuantity(int[] gemArray)
    {
        gemInventoryUI.SetAllGemQuantity(gemArray);
    }

    public void SetGemQuantity(int gemType, int num)
    {
        gemInventoryUI.SetGemQuantity(gemType, num);
    }
    public void SetGemQuantity(Inventory.GemType gemType, int num)
    {
        gemInventoryUI.SetGemQuantity(gemType, num);

    }
#endregion


    public void SetItemQuantity(int num)
    {
        itemCounter.text = num.ToString();
    }
    
    public void InventoryChanged(int[] gemArray, int itemCount)
    {
        SetAllGemQuantity(gemArray);
        itemCounter.text = itemCount.ToString();
    }

}
