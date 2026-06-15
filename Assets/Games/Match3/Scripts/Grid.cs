using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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
    public GameObject pieceBg;
    [Header("预制体列表")]
    public PiecePrefab[] piecePrefabs;

    private Dictionary<PieceType, GameObject> _piecePrefabDict;
    private GamePiece[,] _pieces;
    
    private void Start()
    {
        //初始化数据字典
        _piecePrefabDict = new Dictionary<PieceType, GameObject>();
        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            if (!_piecePrefabDict.ContainsKey(piecePrefabs[i].type))
            {
                _piecePrefabDict[piecePrefabs[i].type] = piecePrefabs[i].prefab;
            }
        }
        //生成背景
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GameObject background = Instantiate(pieceBg, GetWorldPosition(x,y), Quaternion.identity);
                background.gameObject.name = "BG " + x + "-" + y;
                background.transform.parent = transform;
            }
        }
        //生成单位
        _pieces = new GamePiece[xDim, yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GameObject newPiece = Instantiate(_piecePrefabDict[PieceType.Normal], Vector3.zero, quaternion.identity);
                newPiece.name = "piece " + x + "-" + y;
                newPiece.transform.parent = transform;

                _pieces[x, y] = newPiece.transform.GetComponent<GamePiece>();
                _pieces[x ,y].Init(x,y,this,PieceType.Normal);
                if (_pieces[x,y].IsMovable())
                { 
                    _pieces[x,y].MovablePieceRef.Move(x,y);
                }

                if (_pieces[x,y].IsColored())
                {
                    _pieces[x,y].ColorPieceRef.SetColor((ColorPiece.ColorType)Random.Range(0,_pieces[x,y].ColorPieceRef.NumberColor));
                }
            }
        }

    }

    public Vector2 GetWorldPosition(int x, int y)  
    {
        return new Vector2(transform.position.x - xDim / 2f + x, transform.position.y + yDim / 2f - y);
    }
}
