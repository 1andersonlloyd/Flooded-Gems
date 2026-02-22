using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;
using UnityEditor.PackageManager;

public class GemsUI : MonoBehaviour
{

    private List<Image> gemImages;
  


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gemImages = new List<Image>(GetComponentsInChildren<Image>());
        //Debug.Log(gemImages.Count + " gem images found in player plaque");
        SetAllGemQuantity(new int[6] { 0, 0, 0, 0, 0, 0 });
    }

    // Updates all gem amounts from an array
    public void SetAllGemQuantity(int[] gemArray)
    {
        for(int i = 0; i < gemImages.Count; i++)
        {
            SetGemQuantity(i, gemArray[i]);
        }   
    }

    // Sets a specific gem amount from int values
    public void SetGemQuantity(int gemType, int num)
    {
        SetGemQuantity((Inventory.GemType)gemType, num);
    }

    // Sets a specific gem amount from enum and int value
    public void SetGemQuantity(Inventory.GemType gemType, int num)
    {
        if((int)gemType >= gemImages.Count)
        {
            Debug.Log("Higher gem id passed to SetGemQuantity that is present in the gemImages array. Maybe the value is >5 or the array was not initialized.");
            return;
        } 
        Image gemImage = gemImages[(int)gemType];

        if(num > 0)
        {
            gemImage.color = Color.white;
        }
        else
        {
            gemImage.color = new Color(1, 1, 1, 0.5f);
        }
        TextMeshProUGUI numText = gemImage.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        if(num > 1)
        {
            numText.text = num.ToString();
        }
        else
        {
            numText.text = "";
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
