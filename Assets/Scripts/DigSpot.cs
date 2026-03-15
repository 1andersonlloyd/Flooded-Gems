using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System;


public class DigSpot : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public TextMeshPro textMeshRenderer;
    public Inventory.GemType gemType = Inventory.GemType.None;
    // The number 1-6 that needs to be rolled to successfully loot this spot
    public int rollGoal = 0;

    public enum DigSpotType { GemSpot, ItemSpot, StashSpot}
    public DigSpotType digSpotType;
    public static Action PlayerStashDestroyed;

    PlayerController stashOwner = null;
    int[] stashContents = null;
    BoardSpace space = null;


    public List<WeightedLootEntry> lootTable = new List<WeightedLootEntry>
    {
        new WeightedLootEntry{ item = new Item(), weight = 1.0f },
        new WeightedLootEntry{ item = new BootsItem(), weight = 1.0f }

        // Add each new implemented item here to add it to the loot table
    };

    void Start()
    {
        if(digSpotType != DigSpotType.StashSpot){
            if(gemType != Inventory.GemType.None)
            {
                digSpotType = DigSpotType.GemSpot;
            }
            else
            {
                digSpotType = DigSpotType.ItemSpot;
            } 
        }
    }
    // Updates text in editor
    void OnValidate()
    {
        textMeshRenderer.text = rollGoal.ToString();
    } 

    public void InitializeStashSpot(PlayerController player, int[] gemArray)
    {
        digSpotType = DigSpotType.StashSpot;
        stashOwner = player;
        rollGoal = 6;
        stashContents = gemArray;
        spriteRenderer.sprite = MapManager.Instance.stashSprites[(int)player.playerColor];


        // TODO: Spawn the flying gem images

    }



    public virtual bool Dig(PlayerController player, int rollValue)
    {
        // If the stash owner digs their own stash
        if(digSpotType == DigSpotType.StashSpot && stashOwner != null && stashOwner == player)
        {
            player.inventory.AddGemArray(stashContents);
            for(int i = 0; i < stashContents.Length; i++)
            {
                for(int j = 0; j < stashContents[i]; j++)
                {
                    FlyingItem.SpawnFlyingItem(i, transform.position, UIManager.Instance.GetPlayerPlaque(player));
                }
            }
            DestroyStash();
            return true;
        }
     


        if (rollValue < rollGoal)
        {
            Debug.Log("Failed to dig, " + player.playerName + " rolled " + rollValue + " against goal of " + rollGoal);
            return false;
        }
        else
        {
            Debug.Log("Successfully dug, " + player.playerName + " rolled " + rollValue + " against goal of " + rollGoal);
            // GemSpot dig
            if(digSpotType == DigSpotType.GemSpot){
                player.inventory.AddGem(gemType);
                FlyingItem.SpawnFlyingItem((int)gemType, transform.position, UIManager.Instance.GetPlayerPlaque(player));
            }
            // ItemSpot dig
            else if(digSpotType == DigSpotType.ItemSpot)
            {
                Item item = generateItem();
                player.inventory.AddItem(item);
                FlyingItem.SpawnFlyingItem(6, transform.position, UIManager.Instance.GetPlayerPlaque(player));
            }
            // StashSpot dig
            else if(digSpotType == DigSpotType.StashSpot)
            {
                // Get the index of a random gem type from the stashContents array that has a value of >0
                if(stashContents != null)
                {
                    List<int> validIndexes = new List<int>();
                    for(int i = 0; i < stashContents.Length; i++)
                    {
                        if(stashContents[i] > 0)
                        {
                            validIndexes.Add(i);
                        }
                    }
                    int index = UnityEngine.Random.Range(0, validIndexes.Count);
                    stashContents[index]--;
                    player.inventory.AddGem((Inventory.GemType)index);
                    FlyingItem.SpawnFlyingItem(index, transform.position, UIManager.Instance.GetPlayerPlaque(player));

                    // If the stash is empty, destroy the stash
                    bool empty = true;
                    for(int i = 0; i < stashContents.Length; i++)
                    {
                        if(stashContents[i] > 0)
                        {
                            empty = false;
                        }
                    }
                    if(empty)
                    {
                        DestroyStash();
                    }

                }
            }
            // Invalid dig type
            else
            {
                Debug.LogError("Somehow tried to dig a spot with an invalid digSpotType");
                return false;
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

        float randomWeight = UnityEngine.Random.Range(0.0f, totalWeight);
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

    public void DestroyStash()
    {
        Debug.Log("Destroying stash");
        stashOwner.stashSpot = null;
        // Get the parent object
        BoardSpace space = GetComponentInParent<BoardSpace>();
        space.digSpot = null;
        PlayerStashDestroyed?.Invoke();
        MapManager.Instance.ShiftPlayersOnSpace();
        Destroy(gameObject);
    }
}

public class WeightedLootEntry
{
    public Item item;
    public float weight;
}