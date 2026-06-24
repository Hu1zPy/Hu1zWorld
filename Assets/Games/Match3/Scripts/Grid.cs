using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
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
        RowClear,
        ColumnClear,
        Rainbow,
        Count
    }

    [Serializable]
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    }

    [Header("网格大小")] public int xDim;
    public int yDim;
    [Header("背景")] public GameObject pieceBg;
    [Header("预制体列表")] public PiecePrefab[] piecePrefabs;
    [Header("生成间隔")] public float fillTime;
    [Header("下降时间")] public float moveTime;

    private bool inverse = false;
    private GamePiece enterPiece;
    private GamePiece pressPiece;

    private bool isFilling;

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
                GameObject background = Instantiate(pieceBg, GetWorldPosition(x, y), Quaternion.identity);
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
                GenerateNewPiece(x, y, PieceType.Empty);
            }
        }


        Destroy(_pieces[1, 3].gameObject);
        GenerateNewPiece(1, 3, PieceType.Bubble);

        Destroy(_pieces[2, 4].gameObject);
        GenerateNewPiece(2, 4, PieceType.Bubble);

        Destroy(_pieces[4, 5].gameObject);
        GenerateNewPiece(4, 5, PieceType.Bubble);

        Destroy(_pieces[7, 7].gameObject);
        GenerateNewPiece(7, 7, PieceType.Bubble);

        StartCoroutine(Fill());
    }

    IEnumerator Fill()
    {
        bool needsRefill = true;
        isFilling = needsRefill;

        while (needsRefill)
        {
            yield return new WaitForSeconds(fillTime);
            while (FillStep())
            {
                inverse = !inverse;
                yield return new WaitForSeconds(fillTime);
            }

            needsRefill = ClearAllVailPieces();
            isFilling = needsRefill;
        }

    }

    public bool FillStep()
    {
        bool movePiece = false;
        //从第一行开始逐行检测，使掉落到最底层
        for (int y = 0; y < yDim - 1; y++)
        {
            for (int loopX = 0; loopX < xDim; loopX++)
            {
                int x = loopX;

                if (inverse)
                {
                    x = xDim - loopX - 1;
                }

                GamePiece piece = _pieces[x, y];
                if (piece.IsMovable())
                {
                    GamePiece pieceBelow = _pieces[x, y + 1];
                    //先竖向检测是否可以移动
                    if (pieceBelow.PieceType == PieceType.Empty)
                    {
                        Destroy(pieceBelow.gameObject);
                        piece.MovablePieceRef.Move(x, y + 1, moveTime);
                        _pieces[x, y + 1] = piece;
                        GenerateNewPiece(x, y, PieceType.Empty);
                        movePiece = true;
                    }
                    //再斜线检查
                    else
                    {
                        for (int diag = -1; diag <= 1; diag++)
                        {
                            if (diag != 0)
                            {
                                int diagX = x + diag;
                                if (inverse)
                                {
                                    diagX = x - diag;
                                }

                                if (diagX >= 0 && diagX < xDim)
                                {
                                    //检测斜向方块是否为空
                                    GamePiece diagonalPiece = _pieces[diagX, y + 1];
                                    if (diagonalPiece.PieceType == PieceType.Empty)
                                    {
                                        bool hasMovablePiece = true;

                                        //检测空白方块头上是否有可下落方块
                                        for (int aboveY = y; aboveY > 0; aboveY--)
                                        {
                                            GamePiece abovePiece = _pieces[diagX, aboveY];

                                            if (abovePiece.IsMovable())
                                            {
                                                break;
                                            }
                                            else if (abovePiece.PieceType != PieceType.Empty)
                                            {
                                                hasMovablePiece = false;
                                                break;
                                            }
                                        }

                                        //如果没有可下落方块。就斜向填充过去
                                        if (!hasMovablePiece)
                                        {
                                            Destroy(diagonalPiece.gameObject);
                                            piece.MovablePieceRef.Move(diagX, y + 1, moveTime);
                                            _pieces[diagX, y + 1] = piece;
                                            GenerateNewPiece(x, y, PieceType.Empty);
                                            movePiece = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
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
                Destroy(pieceBelow.gameObject);
                GameObject newPiece =
                    Instantiate(_piecePrefabDict[PieceType.Normal], GetWorldPosition(x, -1), quaternion.identity);
                newPiece.transform.parent = transform;
                newPiece.name = "piece" + x + "_0";

                _pieces[x, 0] = newPiece.transform.GetComponent<GamePiece>();
                _pieces[x, 0].Init(x, -1, this, PieceType.Normal);
                _pieces[x, 0].MovablePieceRef.Move(x, 0, moveTime);
                _pieces[x, 0].ColorPieceRef
                    .SetColor((ColorPiece.ColorType)Random.Range(5, _pieces[x, 0].ColorPieceRef.NumberColor));
                movePiece = true;
            }
        }

        isFilling = movePiece;
        return movePiece;
    }

    public GamePiece GenerateNewPiece(int x, int y, PieceType type)
    {
        GameObject newPiece = Instantiate(_piecePrefabDict[type], GetWorldPosition(x, y), quaternion.identity);
        newPiece.transform.parent = transform;
        newPiece.name = "Piece " + x + "_" + y;
        
        _pieces[x, y] = newPiece.GetComponent<GamePiece>();
        _pieces[x, y].Init(x, y, this, type);
        return _pieces[x, y];
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(transform.position.x - xDim / 2f + x, transform.position.y + yDim / 2f - y);
    }

    public bool IsAdjacent(GamePiece piece1, GamePiece piece2)
    {
        return (piece1.X == piece2.X && (int)Mathf.Abs(piece1.Y - piece2.Y) == 1) ||
               (piece1.Y == piece2.Y && (int)Mathf.Abs(piece1.X - piece2.X) == 1);
    }

    public void SwapPieces(GamePiece piece1, GamePiece piece2)
    {
        if (piece1.IsMovable() && piece2.IsMovable())
        {
            //交互数组元素
            _pieces[piece1.X, piece1.Y] = piece2;
            _pieces[piece2.X, piece2.Y] = piece1;

            if (GetMatch(piece1, piece2.X, piece2.Y) != null ||
                GetMatch(piece2, piece1.X, piece1.Y) != null ||
                piece1.PieceType == PieceType.Rainbow ||
                piece2.PieceType == PieceType.Rainbow ) {
                //保留坐标
                int piece1X = piece1.X;
                int piece1Y = piece1.Y;
                //移动
                piece1.MovablePieceRef.Move(piece2.X, piece2.Y, moveTime);
                piece2.MovablePieceRef.Move(piece1X, piece1Y, moveTime);

                if (piece1.PieceType == PieceType.Rainbow && piece1.IsClearable() && piece2.IsColored())
                {
                    ClearColorPiece clearColorPiece = piece1.GetComponent<ClearColorPiece>();
                    if (clearColorPiece)
                    {
                        clearColorPiece.Color = piece2.ColorPieceRef.Color;
                    }

                    ClearPiece(piece1.X, piece1.Y);
                }
                if (piece2.PieceType == PieceType.Rainbow && piece2.IsClearable() && piece1.IsColored())
                {
                    ClearColorPiece clearColorPiece = piece2.GetComponent<ClearColorPiece>();
                    if (clearColorPiece)
                    {
                        clearColorPiece.Color = piece1.ColorPieceRef.Color;
                    }

                    ClearPiece(piece2.X, piece2.Y);
                }

                ClearAllVailPieces();

                if (piece1.PieceType == PieceType.RowClear || piece1.PieceType == PieceType.ColumnClear)
                {
                    ClearPiece(piece1.X, piece1.Y);
                }

                if (piece2.PieceType == PieceType.RowClear || piece2.PieceType == PieceType.ColumnClear)
                {
                    ClearPiece(piece2.X, piece2.Y);
                }

                pressPiece = null;
                enterPiece = null;

                StartCoroutine(Fill());
            }
            else
            {
                //交互数组元素
                _pieces[piece1.X, piece1.Y] = piece1;
                _pieces[piece2.X, piece2.Y] = piece2;
            }
        }
    }

    public void EnterPiece(GamePiece piece)
    {
        enterPiece = piece;
    }

    public void PressPiece(GamePiece piece)
    {
        pressPiece = piece;
    }

    public void ReleasePiece()
    {
        if (IsAdjacent(enterPiece, pressPiece) && !isFilling)
        {
            SwapPieces(enterPiece, pressPiece);
        }
    }

    private List<GamePiece> GetMatch(GamePiece piece, int newX, int newY)
    {
        //判断是否可以配色
        if (!piece.IsColored())
        {
            return null;
        }

        //拿到对于颜色
        var color = piece.ColorPieceRef.Color;
        //创建对应列表
        var horizontalPieces = new List<GamePiece>();
        var verticalPieces = new List<GamePiece>();
        var matchingPieces = new List<GamePiece>();

        //开始匹配横排
        horizontalPieces.Add(piece);
        //分不同方向
        for (int dir = 0; dir <= 1; dir++)
        {
            //不同距离
            for (int xOffset = 1; xOffset < xDim; xOffset++)
            {
                int x = 0;
                //向左
                if (dir == 0)
                {
                    x = newX - xOffset;
                }

                //向右
                if (dir == 1)
                {
                    x = newX + xOffset;
                }

                //排除越界情况
                if (x < 0 || x >= xDim)
                {
                    break;
                }

                //如果颜色匹配则进列表并继续检测
                if (_pieces[x, newY].IsColored() && _pieces[x, newY].ColorPieceRef.Color == color)
                {
                    horizontalPieces.Add(_pieces[x, newY]);
                }
                //颜色不匹配则跳出该方向
                else
                {
                    break;
                }
            }
        }

        // 添加横向匹配单位
        if (horizontalPieces.Count >= 3)
        {
            for (int i = 0; i < horizontalPieces.Count; i++)
            {
                matchingPieces.Add(horizontalPieces[i]);
            }
        }

        // 开始检测 T L 方向
        if (horizontalPieces.Count >= 3)
        {
            for (int i = 0; i < horizontalPieces.Count; i++)
            {
                for (int dir = 0; dir <= 1; dir++)
                {
                    for (int yOffset = 1; yOffset < yDim; yOffset++)
                    {
                        int y = newY;

                        if (dir == 0)
                        {
                            y = newY - yOffset;
                        }

                        if (dir == 1)
                        {
                            y = newY + yOffset;
                        }

                        if (y < 0 || y >= yDim)
                        {
                            break;
                        }

                        if (_pieces[horizontalPieces[i].X, y].IsColored() &&
                            _pieces[horizontalPieces[i].X, y].ColorPieceRef.Color == color)
                        {
                            verticalPieces.Add(_pieces[horizontalPieces[i].X, y]);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (verticalPieces.Count < 2)
                {
                    verticalPieces.Clear();
                }
                else
                {
                    foreach (var verticalPiece in verticalPieces)
                    {
                        matchingPieces.Add(verticalPiece);
                    }

                    break;
                }
            }
        }

        // 返回匹配列表
        if (matchingPieces.Count >= 3)
        {
            return matchingPieces;
        }

        //如果横排没有可消除的
        horizontalPieces.Clear();
        verticalPieces.Clear();
        //开始匹配竖排
        verticalPieces.Add(piece);
        //分不同方向
        for (int dir = 0; dir <= 1; dir++)
        {
            //不同距离
            for (int yOffer = 1; yOffer < yDim; yOffer++)
            {
                int y = 0;
                //向上
                if (dir == 0)
                {
                    y = newY + yOffer;
                }

                //向下
                if (dir == 1)
                {
                    y = newY - yOffer;
                }

                //排除越界情况
                if (y < 0 || y >= yDim)
                {
                    break;
                }

                //如果颜色匹配则进列表并继续检测
                if (_pieces[newX, y].IsColored() && _pieces[newX, y].ColorPieceRef.Color == color)
                {
                    verticalPieces.Add(_pieces[newX, y]);
                }
                //颜色不匹配则跳出该方向
                else
                {
                    break;
                }
            }
        }

        // 添加竖向匹配单位
        if (verticalPieces.Count >= 3)
        {
            for (int i = 0; i < verticalPieces.Count; i++)
            {
                matchingPieces.Add(verticalPieces[i]);
            }
        }

        //开始检查 T L 方向的
        if (verticalPieces.Count >= 3)
        {
            for (int i = 0; i < verticalPieces.Count; i++)
            {
                for (int dir = 0; dir <= 1; dir++)
                {
                    for (int xOffset = 1; xOffset < xDim; xOffset++)
                    {
                        int x = newX;

                        if (dir == 0)
                        {
                            x = newX - xOffset;
                        }

                        if (dir == 1)
                        {
                            x = newX + xOffset;
                        }

                        if (x < 0 || x >= xDim)
                        {
                            break;
                        }

                        if (_pieces[x, verticalPieces[i].Y].IsColored() &&
                            _pieces[x, verticalPieces[i].Y].ColorPieceRef.Color == color)
                        {
                            horizontalPieces.Add(_pieces[x, verticalPieces[i].Y]);
                        }
                    }
                }

                if (horizontalPieces.Count < 2)
                {
                    horizontalPieces.Clear();
                }
                else
                {
                    foreach (var horizontalPiece in horizontalPieces)
                    {
                        matchingPieces.Add(horizontalPiece);
                    }

                    break;
                }
            }
        }

        //返回匹配列表
        if (matchingPieces.Count >= 3)
        {
            return matchingPieces;
        }

        return null;
    }

    private bool ClearAllVailPieces()
    {
        bool needsRefill = false;

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (_pieces[x, y].IsClearable())
                {
                    List<GamePiece> match = GetMatch(_pieces[x, y], x, y);
                    if (match != null)
                    {
                        //生成特殊方块类型
                        PieceType specialPieceType = PieceType.Count;
                        if (match.Count == 4)
                        {
                            //不是由交换进入的
                            if (pressPiece == null || enterPiece == null)
                            {
                                specialPieceType = (PieceType)Random.Range((int)PieceType.RowClear,
                                    (int)PieceType.ColumnClear);
                            }
                            else if (enterPiece.Y == pressPiece.Y)
                            {
                                specialPieceType = PieceType.RowClear;
                            }
                            else
                            {
                                specialPieceType = PieceType.ColumnClear;
                            }
                        }else if (match.Count >= 5)
                        {
                            specialPieceType = PieceType.Rainbow;
                        }

                        //生成特殊方块位置(默认随机)
                        int specialPieceX = match[Random.Range(0, match.Count)].X;
                        int specialPieceY = match[Random.Range(0, match.Count)].Y;

                        for (int i = 0; i < match.Count; i++)
                        {
                            if (ClearPiece(match[i].X, match[i].Y))
                            {
                                needsRefill = true;
                                //如果是交换导致的消除，则在交换处生成
                                if (match[i] == enterPiece || match[i] == pressPiece)
                                {
                                    specialPieceX = match[i].X;
                                    specialPieceY = match[i].Y;
                                }
                            }
                        }
                        if (specialPieceType != PieceType.Count)
                        {
                            Destroy(_pieces[specialPieceX, specialPieceY].gameObject);
                            GamePiece newPiece = GenerateNewPiece(specialPieceX, specialPieceY, specialPieceType);

                            Debug.Log("测试---生成新的特殊方块：" + specialPieceType.ToString());
                            
                            if ((specialPieceType == PieceType.RowClear || specialPieceType == PieceType.ColumnClear) 
                                && newPiece.IsColored() && match[0].IsColored()) {
                                newPiece.ColorPieceRef.SetColor(match[0].ColorPieceRef.Color);
                            }else if (specialPieceType == PieceType.Rainbow && newPiece.IsColored())
                            {
                                newPiece.ColorPieceRef.SetColor(ColorPiece.ColorType.Any);
                            }
                        }
                    }
                }
            }
        }

        return needsRefill;
    }

    private bool ClearPiece(int x, int y)
    {
        if (_pieces[x, y].IsClearable() && !_pieces[x, y].ClearablePieceRef.IsBeingCleared)
        {
            _pieces[x, y].ClearablePieceRef.Clear();
            GenerateNewPiece(x, y, PieceType.Empty);
            ClearObstacle(x, y);
            return true;
        }


        return false;
    }

    private void ClearObstacle(int x, int y)
    {
        for (int adjacentX = x - 1; adjacentX <= x + 1; adjacentX++)
        {
            if (adjacentX != x && adjacentX >= 0 && adjacentX < xDim)
            {
                if (_pieces[adjacentX, y].PieceType == PieceType.Bubble && _pieces[adjacentX, y].IsClearable())
                {
                    _pieces[adjacentX, y].ClearablePieceRef.Clear();
                    GenerateNewPiece(adjacentX, y, PieceType.Empty);
                }
            }
        }

        for (int adjacentY = y - 1; adjacentY <= y + 1; adjacentY++)
        {
            if (adjacentY != y && adjacentY >= 0 && adjacentY < yDim)
            {
                if (_pieces[x, adjacentY].PieceType == PieceType.Bubble && _pieces[x, adjacentY].IsClearable())
                {
                    _pieces[x, adjacentY].ClearablePieceRef.Clear();
                    GenerateNewPiece(x, adjacentY, PieceType.Empty);
                }
            }
        }
    }

    public void ClearRow(int y)
    {
        for (int x = 0; x < xDim; x++)
        {
            ClearPiece(x, y);
        }
    }

    public void ClearColumn(int x)
    {
        for (int y = 0; y < yDim; y++)
        {
            ClearPiece(x, y);
        }
    }

    public void ClearColor(ColorPiece.ColorType colorType)
    {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (_pieces[x, y].IsColored() && 
                    (_pieces[x, y].ColorPieceRef.Color == colorType || colorType == ColorPiece.ColorType.Any))
                {
                    ClearPiece(x,y); 
                }
            }
        }
    }

}

