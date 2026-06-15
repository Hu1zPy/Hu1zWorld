using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public enum PieceType
    {
        Normal,
        Count
    }

    [Serializable]
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    }
    [Header("网格大小")]
    public int xDim;
    public int yDim;
    [Header("背景")]
    public GameObject pieceBG;
    [Header("预制体列表")]
    public PiecePrefab[] piecePrefabs;

    private Dictionary<PieceType, GameObject> piecePrefabDict;
    private GameObject[,] pieces;
    
    private void Start()
    {
        piecePrefabDict = new Dictionary<PieceType, GameObject>();
        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            if (!piecePrefabDict.ContainsKey(piecePrefabs[i].type))
            {
                piecePrefabDict[piecePrefabs[i].type] = piecePrefabs[i].prefab;
            }
        }
        
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GameObject background = Instantiate(pieceBG, GetWorldPosition(x,y), Quaternion.identity);
                background.gameObject.name = "BG " + x + "-" + y;
                background.transform.parent = transform;
            }
        }

        pieces = new GameObject[xDim, yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                pieces[x, y] = Instantiate(piecePrefabDict[PieceType.Normal], GetWorldPosition(x,y), quaternion.identity);
                pieces[x, y].name = "piece " + x + "-" + y;
                pieces[x, y].transform.parent = transform;
            }
        }

    }

    public Vector2 GetWorldPosition(int x, int y)  
    {
        return new Vector2(transform.position.x - xDim / 2f + x, transform.position.y + yDim / 2f - y);
    }
}
