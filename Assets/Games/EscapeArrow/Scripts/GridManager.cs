using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int sizeX = 7, sizeY = 6;
    public GridCell cellPrefab;
    public Transform gridParent;

    private GridCell[,] _gridCells;
    public Dictionary<int, GridCell> IDMap = new Dictionary<int, GridCell>();

    public void CreateGrid()
    {
        int id = 0;
        _gridCells = new GridCell[sizeX, sizeY];
        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                var c = Instantiate(cellPrefab, gridParent);
                c.transform.position = GridToWorld(new Vector2Int(x,y));
                
                c.girdPos = new Vector2Int(x, y);
                c.cellId = id;
                
                _gridCells[x, y] = c;
                IDMap[id++] = c;
            }
        }
    }

    public Vector3 GridToWorld(Vector2Int p)
    {
        Vector3 originPos = gridParent.position - new Vector3(sizeX * .5f, 0, sizeY * .5f);
        return originPos + new Vector3(p.x + .5f,0,p.y + .5f);
    }

    public GridCell GetCell(Vector2Int p)
    {
        if (p.x < 0 || p.y < 0 || p.x >= sizeX || p.y >= sizeY) return null;
        return _gridCells[p.x, p.y];
    }
}
