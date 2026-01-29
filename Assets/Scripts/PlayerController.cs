using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using NUnit.Framework;
using UnityEngine.UIElements;
public abstract class PlayerController : MonoBehaviour
{
    public string playerName = "Default Player";

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
    public bool aiSelectNewGoal = false; // A flag to let AI versions know that they need to recalculate their destination space

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetPosition = transform.position;
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1);
        inventory = new Inventory();
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
    }

    public virtual void StartTurn()
    {
        Debug.Log("Starting Turn for " + playerName);
        actionsLeft = 3;
        StateManager.Instance.currentPhase = TurnPhase.WaitingForPlayerInput;
    }

    public virtual void EndTurn()
    {
        actionsLeft = 0;
        if (TurnManager.Instance.RequestToEndTurn(this))
        {
            Debug.Log("Request to end turn accepted");
        }else{
            Debug.Log("Request to end turn rejected");
        }
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
    }

    public virtual void ExecuteJump(BoardSpace targetSpace)
    {
        Debug.Log("Teleporting for " + playerName);
        // Play animation for teleporting the player
        //transform.position = targetSpace.transform.position;
        targetPosition = targetSpace.transform.position;

        // Update player's board space
        currentSpace = targetSpace;
    }

    public virtual void ExecuteDig()
    {
        Debug.Log("Digging for " + playerName);

        StartCoroutine(ExecuteDigCoroutine());

        actionsLeft--;
    }

    protected virtual IEnumerator ExecuteDigCoroutine()
    {

        // Play animation for digging

        // Check for digspot
        DigSpot digSpot = currentSpace.GetDigSpot();
        if(digSpot == null)
        {
            Debug.Log("Error, no dig spot to dig");
            yield break;
        }

        // Roll die
        int dieResult = 0;
        if (StateManager.Instance.localPlayer == this) // Rolling die on UI for local player
        {
            dieResult = TurnManager.Instance.digButton.RollDie();
        }else if(this is AIController)
        {
            // TODO: This is filler code to allow functionality. Ai players need an ui for rolling dice
            dieResult = UnityEngine.Random.Range(1, 7);
            // NOTE: Doing online functionality will require relegating this function only to the host instance. This might not be the perfect place for this note, but just remember it
        }
        // else I don't think other online player rolling functionality will be here, but idk yet


        yield return new WaitForSeconds(1f);


        // Wait for input/delay to allow for item usage
        yield return TurnManager.Instance.WaitForInteruptsCoroutine(TurnManager.Instance.waitTime, null);


        // Complete dig result
        if (digSpot.Dig(this, dieResult)) // Tries to dig with rolled number
        {
            lastSuccessfulDigSpot = digSpot; // Sets last digspot to prevent repeat digging
            Debug.Log("Player " + playerName + " successfully dug with a " + dieResult.ToString() + "!" );
            aiSelectNewGoal = true;
        }
        TurnManager.Instance.digButton.CompleteDig(); // Finishes the button animation
   
    }
    public virtual void BuryStash(BoardSpace targetSpace, int[] gemsToBuryArray)
    {
        Debug.Log("Burying Stash for " + playerName);
        // Double check for gems in inventory

        // Double check space is valid for stash and no stash exists yet (maybe allow multiple stashes?)

        // Create stash object on space

        // Transfer gems from invetory to stash

        actionsLeft--;
    }

    public virtual void UseItem()
    {
        Debug.Log("Using Item for " + playerName);

        // TODO: Maybe this should be part of the UI? Unsure how much logic can be part of the UI.
    }


    // TODO: I need a ton of simple logic functions for determining what actions are possible to be taken by the player



}

public class Inventory
{
    public enum GemType{Green, Yellow, Red, Blue, Black, White, None}

    public List<Item> items = new List<Item>();
    public int[] gems = new int[6];

    // Gem management
    public void AddGem(GemType gem)
    {
        Debug.Log("Adding " + gem.ToString() + " gem to inventory");
        gems[(int)gem] += 1;
        Debug.Log("Updated Gem Array" + GetGemArrayString());

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

    // Item management
    public void AddItem(Item item)
    {
    
        Debug.Log("Adding item " + item.itemName + " to inventory");
        items.Add(item);
    }
    
    public void RemoveItem(Item item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
        }
    }

    public void UseItem(Item item)
    {
        if (!items.Contains(item))
        {
            Debug.Log("Error, item does not exist in inventory");
            return;
        }




    }
}
