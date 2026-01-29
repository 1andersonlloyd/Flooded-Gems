using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiceTray : SlidingBar
{
    public bool verticalSliding = true;
    public float slidingScaler = 1.0f;

    public Dice[] dice;

    public TextMeshProUGUI resultsText;
    public TextMeshProUGUI threatModifierText;
    public Image spaceColorIndicator;

    // TODO: All of that sliding stuff from itemCard needs to be here as well
    // TODO: Needs to be able to manage one or two dice, depending on flood manager or dig die
    // TODO: Figure out what to do about threat modifier and total displaying for the flood manager

    protected override void Start()
    {
        base.Start();

        // Get all child dice
        dice = GetComponentsInChildren<Dice>();
    }

    public override void InitializeValues()
    {
        if (verticalSliding)
        {
            float height = rectTransform.rect.height * rectTransform.lossyScale.y;
            displayVector = new Vector2(0, height / canvas.scaleFactor * slidingScaler);
        }
        else
        {
            float width = rectTransform.rect.width * rectTransform.lossyScale.x;
            displayVector = new Vector2(width / canvas.scaleFactor * slidingScaler, 0);
        }
        base.InitializeValues();
    }
    public List<int> RollDice(int numDice, List<int> forcedRolls)
    {
        List<int> results = new List<int> { };
        if (dice.Length < numDice)
        {
            Debug.LogError("Dice tray has less than required dice");
            return results;
        }

        int total = 0;
        for (int i = 0; i < numDice; i++)
        {
            int forcedValue = 0;

            if (i < forcedRolls.Count)
            {
                forcedValue = forcedRolls[i];
            }

            int roll = dice[i].Roll(forcedValue);
            total += roll;
            results.Add(roll);
        }
        if (threatModifierText != null)
        {
            total += FloodThreatScale.Instance.getFloodThreatModifier();
        }

        if (resultsText != null)
        {
            float rollTime = dice[0].rollTime;
            StartCoroutine(SetResultsText(total, rollTime));
        }

        return results;
    }

    IEnumerator SetResultsText(int total, float delay)
    {
        yield return new WaitForSeconds(delay);
        resultsText.text = total.ToString();
        if(spaceColorIndicator != null)
        {
            spaceColorIndicator.color = BoardSpace.Colors[FloodThreatScale.getFloodReach(total)];
        }
         
    }

    public override void Hide()
    {
        if (resultsText != null)
        {
            resultsText.text = "";
        }
        base.Hide();
    }
    public override void Show()
    {
        if (resultsText != null)
        {
            resultsText.text = "";
        }
        if (threatModifierText != null)
        {
            threatModifierText.text = FloodThreatScale.Instance.getFloodThreatModifier().ToString();
        }
        if (spaceColorIndicator != null)
        {
            spaceColorIndicator.color = Color.white;
        }
        base.Show();
    }
}
