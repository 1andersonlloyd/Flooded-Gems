using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dice : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI diceText;

    public float rollTime = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }
        if (diceText == null)
        {
            diceText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int Roll(int forcedRoll)
    {
        int result;
        if (forcedRoll != 0)
        {
            result = forcedRoll;
        }
        else
        {
            result = Random.Range(1, 7);
        }
        StartCoroutine(rollDieCoroutine(result, rollTime));
        return result;
    }

    IEnumerator rollDieCoroutine(int result, float time)
    {
        float timeLeft = time;
        while (timeLeft > 0)
        {
            diceText.text = Random.Range(1, 7).ToString();
            yield return null;
            timeLeft -= Time.deltaTime;
        }

        diceText.text = result.ToString();
    }

}
