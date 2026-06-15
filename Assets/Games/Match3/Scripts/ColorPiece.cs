using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPiece : MonoBehaviour
{
    public enum ColorType
    {
        Stick,
        Arrow,
        Bow,
        Fox,
        Hoe,
        Pickaxe,
        Shovel,
        Sword,
        Coal,
        Silver,
        Gold,
        Diamond,
        Any,
    }
    [Serializable]
    public struct ColorSprite
    {
        public ColorType type;
        public Sprite sprite;
    }

    public ColorSprite[] ColorSprites;

    private Dictionary<ColorType, Sprite> _colorSpritesDic;

    private ColorType _colorType;

    public ColorType Color
    {
        set => SetColor(value);
        get => _colorType;
    }

    public int NumberColor
    {
        get => ColorSprites.Length;
    }

    private SpriteRenderer _spriteRenderer;
    private void Awake()
    {
        _spriteRenderer = transform.Find("piece").transform.GetComponent<SpriteRenderer>();
        _colorSpritesDic = new Dictionary<ColorType, Sprite>();
        for (int i = 0; i < ColorSprites.Length; i++)
        {
            if (!_colorSpritesDic.ContainsKey(ColorSprites[i].type))
            {
                _colorSpritesDic[ColorSprites[i].type] = ColorSprites[i].sprite;
            }
        }
    }

    public void SetColor(ColorType newColor)
    {
        _colorType = newColor;

        if (_colorSpritesDic.ContainsKey(newColor))
        {
            _spriteRenderer.sprite = _colorSpritesDic[newColor]; 
        }
    }
}
