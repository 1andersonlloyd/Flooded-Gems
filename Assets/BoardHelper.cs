using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

public class BoardTools : MonoBehaviour
{
    [ContextMenu("Set Nearest 4 Neighbors")]
    void SetNeighbors()
    {
        List<BoardSpace> allSpaces = BoardSpace.allSpaces;

        foreach (var space in allSpaces)
        {
            var nearest = allSpaces
                .Where(other => other != space)
                .OrderBy(other => Vector3.Distance(space.transform.position, other.transform.position))
                .Take(4)
                .ToList();

            space.neighbors = nearest;
            Debug.Log($"Space {space.name} has {nearest.Count} neighbors");
            #if UNITY_EDITOR
            EditorUtility.SetDirty(space); // Mark changed for saving
            #endif
        }

        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif

        Debug.Log("Neighbors assigned!");
    }
}
