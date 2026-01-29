using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    // Vector of mapSpace gameobjects
    public List<BoardSpace> boardSpaces;
    public static MapManager Instance;
    public bool displayHoverSprites = false;

    public float defaultHighlightThickness = 0.07f;
    public const float floodHighlightThickness = 0.3f;
    public Color floodHighlightColor = new Color(0f, 0f, 1f, 1f);

    // Makes sure to add Stashes and remove them from this vvv
    public List<BoardSpace> digSpotSpaces = new List<BoardSpace>();
    public BoardSpace greenSpace ;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

        // Fill boardSpaces with all boardSpace children
        foreach (Transform child in transform)
        {
            boardSpaces.Add(child.GetComponent<BoardSpace>());
        }

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public BoardSpace GetRandomBoardSpaceOfType(BoardSpace.SpaceType desiredSpaceType)
    {
        // Pick a random space from spaces and check if it is of the correct type, if not retry
        BoardSpace space;
        int tries = 0;
        while (tries < 100)
        {
            space = boardSpaces[Random.Range(0, boardSpaces.Count)];
            if (space.spaceType == desiredSpaceType)
            {
                return space;
            }
            tries++;
        }
        Debug.Log("Could not find space of type " + desiredSpaceType + " after 100 tries");
        return null;
    }

    public void HighlightNeighborsInRadius(BoardSpace boardSpace, int radius)
    {
        // If the radius is 0, return
        if (radius == 0)
        {
            return;
        }
        // Get the neighbors of the boardSpace
        List<BoardSpace> neighbors = boardSpace.neighbors;
        // For each neighbor
        foreach (BoardSpace neighbor in neighbors)
        {
            // Highlight the neighbor
            neighbor.lineRenderer.enabled = true;
            // Set the color
            Color color = new Color(204f, 0f, 255f, 1f);
            neighbor.lineRenderer.startColor = new Color(1f, 1f, 1f, 1f);
            neighbor.lineRenderer.endColor = new Color(1f, 1f, 1f, 1f);
            // Set the thickness
            neighbor.lineRenderer.startWidth = defaultHighlightThickness;
            neighbor.lineRenderer.endWidth = defaultHighlightThickness;

            // If the radius is greater than 1
            if (radius > 1)
            {
                // Highlight the neighbors of the neighbor
                HighlightNeighborsInRadius(neighbor, radius - 1);
            }
        }
    }

    public void HighlightFloodedSpaces(BoardSpace.SpaceType floodReach, float thicknessOverride)
    {
        float thickness;
        if (thicknessOverride > 0)
        {
            thickness = thicknessOverride;
        }else
        {
            thickness = floodHighlightThickness;
        }

        // Set linerenderer to active and navy blue on all spaces that have a type that is greater than or equal to floodReach
        foreach (BoardSpace boardSpace in boardSpaces)
        {
            if (boardSpace.spaceType >= floodReach)
            {
                boardSpace.lineRenderer.enabled = true;
                boardSpace.lineRenderer.startColor = floodHighlightColor;
                boardSpace.lineRenderer.endColor = floodHighlightColor;
                // Set the thickness to extra large
                //Debug.Log("Setting thickness to " + thickness);
                boardSpace.lineRenderer.startWidth = thickness;
                boardSpace.lineRenderer.endWidth = thickness;
            }
        }
    }

    public void ClearHighlights()
    {
        // Disable each lineRenderer in each boardSpace
        foreach (BoardSpace boardSpace in boardSpaces)
        {
            boardSpace.lineRenderer.enabled = false;
        }
    }

    public void DisableSprites()
    {
        // Disable each spriteRenderer in each boardSpace
        foreach (BoardSpace boardSpace in boardSpaces)
        {
            boardSpace.spriteRenderer.enabled = false;
        }
        displayHoverSprites = false;
    }
    public List<BoardSpace> FindShortestPath(BoardSpace start, BoardSpace end)
    {
        // Create a queue to store the boardSpaces to visit
        Queue<BoardSpace> queue = new Queue<BoardSpace>();
        // Create a dictionary to store the previous boardSpace in the shortest path
        Dictionary<BoardSpace, BoardSpace> previous = new Dictionary<BoardSpace, BoardSpace>();
        // Create a dictionary to store the distance from the start boardSpace
        Dictionary<BoardSpace, int> distance = new Dictionary<BoardSpace, int>();
        // Add the start boardSpace to the queue
        queue.Enqueue(start);
        // Set the distance of the start boardSpace to 0
        distance[start] = 0;
        // While the queue is not empty
        while (queue.Count > 0)
        {
            // Dequeue a boardSpace from the queue
            BoardSpace current = queue.Dequeue();
            // If the current boardSpace is the end boardSpace
            if (current == end)
            {
                // Create a list to store the shortest path
                List<BoardSpace> path = new List<BoardSpace>();
                // While the current boardSpace is not the start boardSpace
                while (current != start)
                {
                    // Add the current boardSpace to the path
                    path.Add(current);
                    // Set the current boardSpace to the previous boardSpace
                    current = previous[current];
                }
                // Add the start boardSpace to the path
                path.Add(start);
                // Reverse the path
                path.Reverse();
                // Return the path
                return path;
            }
            // For each neighbor of the current boardSpace
            foreach (BoardSpace neighbor in current.neighbors)
            {
                // If the neighbor has not been visited
                if (!distance.ContainsKey(neighbor))
                {
                    // Add the neighbor to the queue
                    queue.Enqueue(neighbor);
                    // Set the distance of the neighbor
                    distance[neighbor] = distance[current] + 1;
                    // Set the previous boardSpace of the neighbor
                    previous[neighbor] = current;
                }
            }

        }
        // If no path is found, return null
        return null;
    }

    public int GetDistanceToSpace(BoardSpace start, BoardSpace end)
    {
        return FindShortestPath(start, end).Count - 1;
    }


    public List<BoardSpace> GetAllDigSpotSpaces()
    {
        if(digSpotSpaces.Count == 0)
        {
            GenerateAllDigSpotSpaces();
        }
        return digSpotSpaces;
    }

    void GenerateAllDigSpotSpaces()
    {
        foreach (BoardSpace space in boardSpaces)
        {
            if (space.digSpot != null)
            {
                digSpotSpaces.Add(space);
            }
        }
        Debug.Log("Found " + digSpotSpaces.Count + " dig spot spaces and stored the list");
    }


}
