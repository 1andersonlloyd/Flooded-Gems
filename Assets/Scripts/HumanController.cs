using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;


public class HumanController : PlayerController
{
    public bool moveInputEnabled = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return; // UI is under the mouse, don't click board spaces

            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

            if (hit != null)
            {
                BoardSpace space = hit.GetComponent<BoardSpace>();
                if (space != null)
                {
                    HandleSpaceClicked(space);
                }
            }
        }
        base.Update();
    }

    public override void StartTurn()
    {
        base.StartTurn();


        // Player should be able to end the turn based on UI or automatic check in a different spot
    }

    public override void EndTurn()
    {




        // Call base to end the turn officially
        base.EndTurn();
    }

    public void HandleSpaceClicked(BoardSpace space)
    {
        Debug.Log("Space clicked: " + space.name);
        if (moveInputEnabled && TurnManager.Instance.RequestMove(this, space))
        {
            MoveButton.Instance.DisableMove();
            Debug.Log("Move Request was accepted");
        }
        else
        {
            Debug.Log("Move Request was rejected");
        }
    }

}
