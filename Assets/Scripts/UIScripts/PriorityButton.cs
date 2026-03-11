using UnityEngine;
using UnityEngine.UI;

public class PriorityButton : MonoBehaviour
{
    Button button;
    bool holdingInterrupt = false;
    private Image buttonImage;
    public Color normalColor = Color.white;
    public Color pressedColor = Color.red;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        UpdateButtonVisual();
        button.onClick.AddListener(HandleClick);
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


    public void UpdateButtonVisual()
    {
        if (buttonImage != null && LocalGameManager.Instance.localPlayer != null)
        {
            buttonImage.color = holdingInterrupt ? pressedColor : normalColor;
        }
    }
}

