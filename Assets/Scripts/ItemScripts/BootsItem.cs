using UnityEngine;

public class BootsItem : Item
{
    public BootsItem()
    {
        this.itemName = "boots";
        this.itemDescription = "boots";
    }

    public override void UseItem(PlayerController player, object input)
    {
        Debug.Log("Using boots");
    }

}
