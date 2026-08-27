using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowManager : MonoBehaviour
{
    public Arrow prefab;
    public Transform arrowsParent;
    public int remainingCount;
    public float moveTime = .5f;
    
    public void SetUpArrows(List<List<int>> allArrowIDs)
    {
        foreach (var arrowIDs in allArrowIDs)
        {
            var a = Instantiate(prefab, arrowsParent);
            a.SetUpFromData(arrowIDs);
        }
        remainingCount = arrowsParent.childCount;
    }
}
