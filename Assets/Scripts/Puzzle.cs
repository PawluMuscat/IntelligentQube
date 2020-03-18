using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle
{
    [SerializeField]private int _width;
    [SerializeField]private int _length;
    public string[] puzzleData;

    public void SetDimensions()
    {
        _width = puzzleData[0].Length;
        _length = puzzleData.Length;
    }

    public void InputPuzzleData(string[] data)
    {
        puzzleData = data;
    }

    public void DebugTool()
    {
        foreach (string data in puzzleData)
        {
            Debug.Log(data);
            Debug.Log(_width + " " + _length);
        }
    }
}
