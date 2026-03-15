using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;


public class HumanController : PlayerController
{
    public bool moveInputEnabled = false;

    protected override void Awake()
    {
        base.Awake();
    }


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

    public void HandleSpaceClicked(BoardSpace space)
    {
        Debug.Log("Space clicked: " + space.name);
        if (moveInputEnabled && LocalGameManager.Instance.RequestMove(this, space))
        {
            MoveButton.Instance.DisableMove();
            Debug.Log("Move Request was accepted");
        }
        else
        {
            Debug.Log("Move Request was rejected");
        }
    }


    public void DigButtonClicked()
    {
        if (LocalGameManager.Instance.RequestToDig(LocalGameManager.Instance.localPlayer))
        {
            Debug.Log($"[{nameof(DigButton)}] Dig Request Accepted");
            StartCoroutine(ExecuteDigCoroutine());
        }
    }

    public void StashButtonSubmitted(int[] gemArray)
    {
        if(gemArray == null)
        {
            Debug.LogError("Invalid gemArray passed from StashButtonSubmitted");
            return;
        }
        
        if(gemArray.Length != 6)
        {
            Debug.LogError("Invalid gemArray passed from StashButtonSubmitted");
            return;
        }

        bool valid = false;
        foreach(int i in gemArray)
        {
            if(i > 0)
            {
                valid = true;
            }else if(i < 0)
            {
                Debug.LogError("Invalid gemArray passed from StashButtonSubmitted");
                return;
            }
        }
        if(!valid)
        {
            Debug.LogError("Invalid gemArray (empty array) passed from StashButtonSubmitted");
            return;
        }

        Debug.Log($"Requesting to stash gems: {gemArray[0]}, {gemArray[1]}, {gemArray[2]}, {gemArray[3]}, {gemArray[4]}, {gemArray[5]}");

        if (LocalGameManager.Instance.RequestToStash(LocalGameManager.Instance.localPlayer))
        {
            Debug.Log($"[{nameof(StashButton)}] Stash request Accepted");
            StartCoroutine(BuryStashCoroutine(gemArray));
        }
    }

}
