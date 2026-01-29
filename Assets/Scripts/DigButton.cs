using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class DigButton : MonoBehaviour
{
    public DiceTray diceTray;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the Button component attached to this GameObject
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void HandleClick()
    {
        if (TurnManager.Instance.RequestToDig(StateManager.Instance.localPlayer))
        {
            Debug.Log($"[{nameof(DigButton)}] Dig Request Accepted");
        }
    }

    public int RollDie()
    {
        diceTray.Show();
        return diceTray.RollDice(1, new List<int> { })[0];
    }

    public void CompleteDig()
    {
        diceTray.Hide();
    }
}
