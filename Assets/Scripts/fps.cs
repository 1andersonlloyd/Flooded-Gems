using UnityEngine;


public class FPSLogger : MonoBehaviour
{
    float timer;
    
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 1f)
        {
            float fps = 1f / Time.deltaTime;
            //Debug.Log("FPS: " + Mathf.RoundToInt(fps) + " (calculated in fps.cs)");
            timer = 0f;
        }
    }
}