using UnityEngine;

public class Item
{
    public string itemName;
    public string itemDescription;

    public Item()
    {
        itemName = "Default Item";
        itemDescription = "This is an item";
    }

    public Item(string name, string description)
    {
        itemName = name;
        itemDescription = description;
    }

    public virtual void UseItem(PlayerController player, object input)
    {
        Debug.Log("Using " + itemName);
        Debug.Log("Input type: " + input.GetType());
    }

    // TODO: I think each item should be able to launch its own UI menu maybe? I'm not sure. Ideally all item information is contained in this class

    // TODO: Needs external inventory management system
}
