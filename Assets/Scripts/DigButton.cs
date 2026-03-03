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
            StateManager.Instance.localPlayer.ExecuteDig();
        }

    }

    public int RollDie()
    {
        //diceTray.Show();

        int dieResult = Random.Range(1, 7); //Probably not ideal to have this calculated here
        return diceTray.RollDice(new List<int> {dieResult })[0];
    }

    public void CompleteDig()
    {
        diceTray.Hide();
    }
}
