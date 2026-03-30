using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.Events;
using NUnit.Framework;
using UnityEngine.UIElements;
public enum PlayerColor { Blue, Red, Green, Yellow}

public abstract class PlayerController : MonoBehaviour
{
    public string playerName = "Default Player";

    public PlayerColor playerColor = 0;

    public int actionsLeft = 0;
    public BoardSpace startingSpace;
    public BoardSpace currentSpace;

    public Vector3 targetPosition;
    [SerializeField]
    SpriteRenderer spriteRenderer;
    [SerializeField]
    List<Sprite> playerSprites = new List<Sprite>();
    public float targetAlpha = 1.0f;
    float moveSpeed = 20f;
    public Inventory inventory;
    public DigSpot lastSuccessfulDigSpot = null;
    public DigSpot stashSpot = null;
    public bool aiSelectNewGoal = false; // A flag to let AI versions know that they need to recalculate their destination space
    public bool localPause = false;

    public static Action<PlayerController, BoardSpace> PlayerMoving;
    public static Action<PlayerController, DigSpot, int> RolledDieForDigging; // An event for CPU to respond to with shovels or fake gems, (player, digspot, die result)
    public static Action<PlayerController> FinishedDigRoll;
    public static Action<PlayerController> FinishedStashing;

    protected virtual void Awake()
    {
        inventory = new Inventory(this);
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetPosition = transform.position;
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1);
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        if (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
        if (spriteRenderer.color.a != targetAlpha)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Lerp(spriteRenderer.color.a, targetAlpha, Time.deltaTime * 5));
        }

    }

    public void InitializePlayer(string name, BoardSpace startingSpace, int spriteIndex)
    {
        playerName = name;
        this.startingSpace = startingSpace;
        currentSpace = startingSpace;
        transform.position = startingSpace.transform.position;
        targetPosition = transform.position;
        spriteRenderer.sprite = playerSprites[Math.Min(spriteIndex, playerSprites.Count)];
        playerColor = (PlayerColor)spriteIndex;
    }

    public virtual void StartTurn()
    {
        Debug.Log("Starting Turn for " + playerName);
        actionsLeft = LocalGameManager.Instance.actionsPerTurn;
        LocalGameManager.Instance.currentPhase = TurnPhase.Interruptable;
    }

    public virtual void EndTurn()
    {
        actionsLeft = 0;
        LocalGameManager.Instance.RequestToEndTurn(this);
    }


    public virtual void ExecuteMove(List<BoardSpace> path)
    {
        BoardSpace targetSpace = path[path.Count - 1];

        Debug.Log("Moving for " + playerName);
        // Play animation for moving the player
        //transform.position = targetSpace.transform.position;
        targetPosition = targetSpace.transform.position;

        // Update player's board space
        currentSpace = targetSpace;

        actionsLeft -= path.Count - 1;
        
        PlayerMoving?.Invoke(this, targetSpace);
    }

    public virtual void ExecuteJump(BoardSpace targetSpace)
    {
        Debug.Log("Teleporting for " + playerName);
        // Play animation for teleporting the player
        //transform.position = targetSpace.transform.position;
        targetPosition = targetSpace.transform.position;

        // Update player's board space
        currentSpace = targetSpace;
        PlayerMoving?.Invoke(this, targetSpace);

    }

    // The coroutine that performs the dig, currently called by code in HumanController and AIController
    protected virtual IEnumerator ExecuteDigCoroutine()
    {

        // Play animation for digging?

        // Check for digspot
        DigSpot digSpot = currentSpace.GetDigSpot();
        if(digSpot == null)
        {
            Debug.Log("Error, no dig spot to dig");
            yield break;
        }
        // Prevent repeatedly digging the same spot to farm items
        if(digSpot == lastSuccessfulDigSpot)
        {
            Debug.LogError("Error, can't dig at a the spot you successfully dug at last");
            yield break;
        }

        actionsLeft--;

        // Roll die
        int dieResult;
        // Gives an automatic success if digging your own stash spot
        if(digSpot.digSpotType == DigSpot.DigSpotType.StashSpot && digSpot.stashOwner == this)
        {
            dieResult = 6;
            Debug.Log("Auto-success on stash dig");
        }
        else
        // otherwise rolls a d6 normally
        {
            dieResult = UnityEngine.Random.Range(1, 7);
            UIManager.Instance.RollPlayerDie(this, dieResult);
        }

        // This is a bandaid fix to just let the die rolling animation finish
        yield return new WaitForSeconds(1f);


        // Broadcast the result and allow other players and AI to respond with items.
        RolledDieForDigging?.Invoke(this, digSpot, dieResult);

        // Wait for input/delay to allow for item usage in response
        LocalGameManager.Instance.currentPhase = TurnPhase.Interruptable;
        yield return LocalGameManager.Instance.WaitForInteruptsCoroutine(LocalGameManager.Instance.waitTime, null);
        LocalGameManager.Instance.currentPhase = TurnPhase.NonInterruptable;


        // Complete dig result
        if (digSpot.Dig(this, dieResult)) // Tries to dig with rolled number
        {
            lastSuccessfulDigSpot = digSpot; // Sets last digspot to prevent repeat digging
            Debug.Log("Player " + playerName + " successfully dug with a " + dieResult.ToString() + "!" );
            aiSelectNewGoal = true;            

        }
        FinishedDigRoll?.Invoke(this);

    }
    public virtual IEnumerator BuryStashCoroutine(int[] gemsToBuryArray)
    {
        Debug.Log("Burying Stash for " + playerName);
        // Double check for gems in inventory

        // Double check space is valid for stash and no stash exists yet (maybe allow multiple stashes?)

        // Create stash object on space

        // Transfer gems from invetory to stash



        // Make sure that current gem inventory array has enough gems for what is being removed
        int[] gemArray = inventory.GetGemArray();
        if(gemArray == null)
        {
            Debug.LogError("Error, failed to get inventory gem array when burying stash");
            yield break;
        }
        
        for(int i = 0; i < gemsToBuryArray.Length; i++)
        {
            if(gemsToBuryArray[i] > gemArray[i])
            {
                Debug.LogError("Error, not enough " + (Inventory.GemType)i + " gems to bury");
                yield break;
            }
        }

        DigSpot stashDigSpot = MapManager.Instance.AddStashDigSpot(this, gemsToBuryArray, currentSpace);
        stashSpot = stashDigSpot;
        inventory.RemoveGemArray(gemsToBuryArray);

        actionsLeft--;

        FinishedStashing?.Invoke(this);
    }

    public virtual void UseItem()
    {
        Debug.Log("Using Item for " + playerName);

        // TODO: Maybe this should be part of the UI? Unsure how much logic can be part of the UI.
    }
}

public class Inventory
{
    public enum GemType{Green, Yellow, Red, Blue, Black, White, None}
    public PlayerController inventoryOwner;
    public List<Item> items = new List<Item>();
    public int[] gems = new int[6];
    public Action<int[], int> onInventoryChanged;
    public Action<PlayerController, Item> ItemAdded;
    public Action<PlayerController, Item> ItemRemoved;

    public Inventory(PlayerController owner = null)
    {
        inventoryOwner = owner;
    }

    // Gem management
    public void AddGem(GemType gem)
    {
        Debug.Log("Adding " + gem.ToString() + " gem to inventory");
        gems[(int)gem] += 1;
        Debug.Log("Updated Gem Array" + GetGemArrayString());

        // UpdateListeners();
        // Not updating here to instead allow for the flying item script to time it correctly with the item's arrival
    }

    public void AddGemArray(int[] gemsToAdd)
    {
        for (int i = 0; i < gemsToAdd.Length; i++)
        {
            gems[i] += gemsToAdd[i];
        }
        Debug.Log("Updated Gem Array" + GetGemArrayString());

        // UpdateListeners();
        // Not updating here to instead allow for the flying item script to time it correctly with the item's arrival
    }
    public bool HasGem(GemType gem)
    {
        if(gem == GemType.None)
        {
            return false;
        }
        return gems[(int)gem] > 0;
    }
    public void RemoveGem(GemType gem)
    {
        Debug.Log("Removing " + gem.ToString() + " gem from inventory");
        gems[(int)gem] -= 1;
        UpdateListeners();
    }

    public void RemoveGemArray(int[] gemsToRemove)
    {
        for(int i = 0; i < gemsToRemove.Length; i++)
        {
            gems[i] -= gemsToRemove[i];
        }
        UpdateListeners();
    }
    public int[] GetGemArray()
    {
        return new int[]{gems[0], gems[1], gems[2], gems[3], gems[4], gems[5]};
    }
    public string GetGemArrayString()
    {
        return "[" + gems[0] + ", " + gems[1] + ", " + gems[2] + ", " + gems[3] + ", " + gems[4] + ", " + gems[5] + "]";
    }

    public bool HasEveryGem()
    {
        bool hasEveryGem = true;
        for(int i = 0; i < gems.Length; i++)
        {
            if (gems[i] <= 0)
            {
                hasEveryGem = false;
            }
        }
        return hasEveryGem;   
    }

    public int GetGemTotal()
    {
        int totalGems = 0;
        for (int i = 0; i < gems.Length; i++)
        {
            totalGems += gems[i];
        }
        return totalGems;
    }

    // Item management
    public void AddItem(Item item)
    {
    
        Debug.Log("Adding item " + item.itemName + " to inventory");
        items.Add(item);
        ItemAdded?.Invoke(inventoryOwner, item);

        //UpdateListeners();
        // Not updating here to instead allow for the flying item script to time it correctly with the item's arrival
    }
    
    public void RemoveItem(Item item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
        }
        UpdateListeners();
    }

    public void UseItem(Item item)
    {
        if (!items.Contains(item))
        {
            Debug.Log("Error, item does not exist in inventory");
            return;
        }
        UpdateListeners();
    
    }

    public void UpdateListeners()
    {
        onInventoryChanged?.Invoke(GetGemArray(), items.Count);
    }



}
