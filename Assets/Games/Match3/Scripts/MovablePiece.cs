using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovablePiece : MonoBehaviour
{
    private IEnumerator moveIenumerator;
    private GamePiece _gamePiece;
    

    private void Awake()
    {
        _gamePiece = GetComponent<GamePiece>();
    }

    public void Move(int newX, int newY,float moveTime)
    {
        if (moveIenumerator != null)
        {
            StopCoroutine(moveIenumerator);
        }

        moveIenumerator = MoveCoroutine(newX, newY, moveTime);
        StartCoroutine(moveIenumerator);
    }

    IEnumerator MoveCoroutine(int newX,int newY,float moveTime)
    {
        _gamePiece.X = newX;
        _gamePiece.Y = newY;

        Vector3 startPos = transform.position;
        Vector3 endPos = _gamePiece.GridRef.GetWorldPosition(newX, newY);
        
        for (float t = 0; t < moveTime; t += Time.deltaTime)
        {
            _gamePiece.transform.position = Vector3.Lerp(startPos, endPos, t / moveTime);
            yield return 0;
        }

        _gamePiece.transform.position = endPos;
    }
}
