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
        Empty,
        Normal,
        Bubble,
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
    [Header("生成间隔")]
    public float fillTime;
    [Header("下降时间")]
    public float moveTime;

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
                GenerateNewPiece(x,y,PieceType.Empty);
            }
        }
        
        Destroy(_pieces[4,5].gameObject);
        GenerateNewPiece(4, 5, PieceType.Bubble);
        
        StartCoroutine(Fill());
    }

    IEnumerator Fill()
    {
        while (FillStep())
        {
            yield return new WaitForSeconds(fillTime);
        }
    }
    public bool FillStep()
    {
        bool movePiece = false;
        //从第一行开始逐行检测，使掉落到最底层
        for (int y = 0; y < yDim -1; y++)
        {
            for (int x = 0; x < xDim; x++)
            {
                GamePiece piece = _pieces[x, y];
                if (piece.IsMovable())
                {
                    GamePiece pieceBelow = _pieces[x, y + 1];
                    if (pieceBelow.PieceType == PieceType.Empty)
                    {
                        Destroy(pieceBelow);
                        piece.MovablePieceRef.Move(x, y + 1,moveTime);
                        _pieces[x, y + 1] = piece;
                        GenerateNewPiece(x, y, PieceType.Empty);
                        movePiece = true;
                    }
                }
            }
        }
        //检测第一行空位
        for (int x = 0; x < xDim; x++)
        {
            GamePiece pieceBelow = _pieces[x, 0];
            if (pieceBelow.PieceType == PieceType.Empty)
            {
                Destroy(pieceBelow);
                GameObject newPiece =
                    Instantiate(_piecePrefabDict[PieceType.Normal], GetWorldPosition(x, -1), quaternion.identity);
                newPiece.transform.parent = transform;
                newPiece.name = "piece" + x + "_0";
                
                _pieces[x, 0] = newPiece.transform.GetComponent<GamePiece>();
                _pieces[x, 0].Init(x, -1, this, PieceType.Normal);
                _pieces[x, 0].MovablePieceRef.Move(x, 0,moveTime);
                _pieces[x,0].ColorPieceRef.SetColor((ColorPiece.ColorType)Random.Range(0,_pieces[x,0].ColorPieceRef.NumberColor));
                movePiece = true;
            }
        }
        return movePiece;
    }

    public GamePiece GenerateNewPiece(int x, int y, PieceType type)
    {
        GameObject newPiece = Instantiate(_piecePrefabDict[type], GetWorldPosition(x, y), quaternion.identity);
        newPiece.transform.parent = transform;
        newPiece.name = "Piece " + x + "_" + y;

        _pieces[x, y] = newPiece.GetComponent<GamePiece>();
        _pieces[x, y].Init(x,y,this,type);
        return _pieces[x, y];
    }
    
    public Vector2 GetWorldPosition(int x, int y)  
    {
        return new Vector2(transform.position.x - xDim / 2f + x, transform.position.y + yDim / 2f - y);
    }
}
