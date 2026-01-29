using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class WaitIndicator : MonoBehaviour
{
    float percentage = 0;
    // Original rotation of the wait indicator
    Quaternion originalRotation;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (percentage <= 0 && transform.localScale != Vector3.zero)
        {
            transform.localScale = Vector3.zero;
        }
        else if (percentage > 0 && transform.localScale == Vector3.zero)
        {
            transform.localScale = new Vector3(1,1,1);
        }
    }
    
    public void SetPercentage(float _percentage)
    {
        percentage = Mathf.Clamp(_percentage, 0, 1);
        transform.rotation = originalRotation * Quaternion.Euler(0, 0, -percentage * 360);
    }
}
