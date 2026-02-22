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
    protected TextMeshProUGUI itemCount;
    
    [SerializeField]
    protected GemsUI gemInventoryUI;



    [SerializeField]
    protected List<Sprite> gemSprites;
    [SerializeField]
    protected List<Sprite> plaqueSprites;
    [SerializeField]
    protected List<Sprite> stashSprites;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetPlayerColor(0);
    }

    public void initialize(int playerColor, string name)
    {
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
        itemCount.text = num.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    
    public void UpdatePlaque()
    {
        


    }

}
