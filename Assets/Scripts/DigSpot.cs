using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DigSpot : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public TextMeshPro textMeshRenderer;
    public bool blackText = false;
    public Inventory.GemType gemType = Inventory.GemType.None;
    // The number 1-6 that needs to be rolled to successfully loot this spot
    public int rollGoal = 0;

    public enum DigSpotType { GemSpot, ItemSpot, StashSpot}
    public DigSpotType digSpotType;

    public List<WeightedLootEntry> lootTable = new List<WeightedLootEntry>
    {
        new WeightedLootEntry{ item = new Item(), weight = 1.0f },
        new WeightedLootEntry{ item = new BootsItem(), weight = 1.0f }

        // Add each new implemented item here to add it to the loot table
    };

    void Start()
    {
        if(gemType != Inventory.GemType.None)
        {
            digSpotType = DigSpotType.GemSpot;
        }
        else
        {
            digSpotType = DigSpotType.ItemSpot;
        } //NOTE: This doesn't currently account for stash spots, but that might be fine as long as you remember this sets the values

    }
    void OnValidate()
    {
        textMeshRenderer.text = rollGoal.ToString();
    } 

    public bool Dig(PlayerController player, int rollValue)
    {
        if (rollValue < rollGoal)
        {
            Debug.Log("Failed to dig, " + player.playerName + " rolled " + rollValue + " against goal of " + rollGoal);
            return false;
        }
        else
        {
            Debug.Log("Successfully dug, " + player.playerName + " rolled " + rollValue + " against goal of " + rollGoal);
            if (gemType != Inventory.GemType.None)
            {
                player.inventory.AddGem(gemType);
            }
            else
            {
                Item item = generateItem();
                player.inventory.AddItem(item);
            }
        }
        return true;
    }

    
    public Item generateItem()
    {
        float totalWeight = 0.0f;
        foreach (WeightedLootEntry entry in lootTable)
        {
            totalWeight += entry.weight;
        }

        float randomWeight = Random.Range(0.0f, totalWeight);
        float currentWeight = 0.0f;
        foreach (WeightedLootEntry entry in lootTable)
        {
            currentWeight += entry.weight;
            if (currentWeight >= randomWeight)
            {
                return entry.item;
            }
        }
        return null;
    }
}

public class WeightedLootEntry
{
    public Item item;
    public float weight;
}