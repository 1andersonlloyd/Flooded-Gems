using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Net.Mime;

// Spaces on the map that players can walk on. They can have digspots and stashes.

public class BoardSpace : MonoBehaviour
{
    // Vector of neighboring spaces, able to be edited in the Unity editor
    public List<BoardSpace> neighbors = new List<BoardSpace>();
    public static List<BoardSpace> allSpaces = new List<BoardSpace>();
    public enum SpaceType
    {
        Green = 1,
        Blue = 2,
        Yellow = 3,
        Red = 4,
        Black = 5
    }
    public static readonly Dictionary<SpaceType, Color> Colors = new Dictionary<SpaceType, Color>
    {
        { SpaceType.Green, Color.green },
        { SpaceType.Blue, Color.blue },
        { SpaceType.Yellow, Color.yellow },
        { SpaceType.Red, Color.red },
        { SpaceType.Black, Color.black }
    };
    public SpaceType spaceType;

    public SpriteRenderer spriteRenderer;
    public LineRenderer lineRenderer;

    public DigSpot digSpot;

    void OnEnable()
    {
        // Add to allSpaces if not already in it
        if (!allSpaces.Contains(this))
        {
            allSpaces.Add(this);
        }
    }

    void OnDisable()
    {
        // Remove from allSpaces if in it
        if (allSpaces.Contains(this))
        {
            allSpaces.Remove(this);
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Check to see if it has a digspot child and if it does, set it as digSpot
        digSpot = GetComponentInChildren<DigSpot>();
        if (digSpot != null)
        {
            digSpot.transform.localPosition = new UnityEngine.Vector3(0,0,0);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Editor Script
    [ContextMenu("Set List to Current Selection")]
    public void SetToSelection()
    {
        List<GameObject> selection = new List<GameObject>(Selection.gameObjects);
        // Remove self from list if present
        selection.Remove(gameObject);
        neighbors = selection.Select(x => x.GetComponent<BoardSpace>()).ToList();
        Debug.Log("Assigned " + neighbors.Count + " objects.");
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.purple;
        Gizmos.DrawSphere(transform.position, 0.2f);

        Gizmos.color = Color.pink;
        foreach (BoardSpace neighbor in neighbors)
        {
            if (neighbor != null)
            {
                // Draw line halfway to neighbor
                //Gizmos.DrawLine(transform.position, neighbor.transform.position / 2 + transform.position / 2);
#if UNITY_EDITOR
                Handles.color = new Color(1f, 0.41f, 0.7f);
                Handles.DrawAAPolyLine(10f, transform.position, neighbor.transform.position / 2 + transform.position / 2); // 5 pixels thick anti-aliased line
#endif
            }
        }

    }



    void OnMouseEnter()
    {
        if (MapManager.Instance.displayHoverSprites)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    void OnMouseExit()
    {
        spriteRenderer.enabled = false;
    }

    public void highlightNeighbors()
    {
        foreach (BoardSpace neighbor in neighbors)
        {
            if (neighbor != null)
            {
                neighbor.lineRenderer.enabled = true;
                // Set the color
                Color color = new Color(204f, 0f, 255f, 1f);
                neighbor.lineRenderer.startColor = new Color(1f, 1f, 1f, 1f);
                neighbor.lineRenderer.endColor = new Color(1f, 1f, 1f, 1f);
                // Set the thickness
                neighbor.lineRenderer.startWidth = 0.07f;
                neighbor.lineRenderer.endWidth = 0.07f;
            }

        }
    }

    public DigSpot GetDigSpot()
    {
        return digSpot;
    }
}
