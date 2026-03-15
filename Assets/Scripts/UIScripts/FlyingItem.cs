using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
public class FlyingItem : MonoBehaviour
{
    // This class is for visuals of items or gems that fly from one spot to another
    [SerializeField]
    public static FlyingItem staticPrefab;
    public Image image;
    public List<Sprite> sprites;
    int spriteId = 0;
    RectTransform target;
    public float speed;
    float wait = 0.3f;
    bool invUpdated = false;
    Inventory inventory;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        if(staticPrefab == null){
            staticPrefab = Resources.Load<FlyingItem>("Flying Item");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        image.sprite = sprites[spriteId];
    }

    public static void SpawnFlyingItem(int spriteId, Vector3 spawnPosition, PlayerPlaque target)
    {
        FlyingItem newSpawn = Instantiate(staticPrefab);
        newSpawn.transform.SetParent(UIManager.Canvas.GetComponent<RectTransform>(), false);
        newSpawn.spriteId = spriteId;
        newSpawn.transform.position = Camera.main.WorldToScreenPoint(spawnPosition);
        newSpawn.target = target.GetElementRectTransform(spriteId);
        newSpawn.inventory = target.player.inventory;
    }

    // Update is called once per frame
    void Update()
    { 
        // Lerp to target position 
        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * speed);


        // If we are close enough to our target, snap to it and destroy self
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            transform.position = target.position;
            // Reduce the scale by 10 percent
            transform.localScale *= 0.9f;
            wait -= Time.deltaTime;

            if (!invUpdated)
            {
                inventory.UpdateListeners();
            }


            if (wait < 0){
                Destroy(gameObject);
                
            }
        }
    }
}
