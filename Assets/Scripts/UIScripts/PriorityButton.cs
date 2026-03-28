using UnityEngine;
using UnityEngine.UI;

public class PriorityButton : MonoBehaviour
{
    Button button;
    bool holdingInterrupt = false;
    private Image buttonImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        UIManager.Instance.SetButtonColors(button);
        UpdateButtonVisual();
        button.onClick.AddListener(HandleClick);
        LocalGameManager.OnCurrentPhaseChanged += PhaseChange;
    }

    // Update is called once per frame
    void Update()
    {

    }
    void HandleClick()
    {
        Debug.Log("PriorityButton clicked");
        if (!holdingInterrupt)
        {
            holdingInterrupt = true;
            LocalGameManager.Instance.AddPlayerToInterruptList(LocalGameManager.Instance.localPlayer);
        }
        else
        {
            holdingInterrupt = false;
            LocalGameManager.Instance.RemovePlayerFromInterruptList(LocalGameManager.Instance.localPlayer);
        }
    }

    void PhaseChange(TurnPhase turnPhase)
    {
        UpdateButtonVisual();
    }

    public void UpdateButtonVisual()
    {
        // if (buttonImage != null && LocalGameManager.Instance.localPlayer != null)
        // {
        //     button.interactable = LocalGameManager.Instance.currentPhase == TurnPhase.WaitingForPlayerInput;
        // }
    }
}

