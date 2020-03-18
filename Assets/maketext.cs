using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class maketext : MonoBehaviour
{
    public TextAsset text;
    // Start is called before the first frame update
    void Start()
    {
        ReadPuzzleData();
    }

    public void ReadPuzzleData()
    {
        //Load the text file
        //Code a string that gets the dimensions needed for current level and wave so the correct path is chosen
        TextAsset puzzleData = text;
        //Remove all of the new lines and spaces
        string data = puzzleData.text.Replace(Environment.NewLine, "-\n");
        System.IO.File.WriteAllText("Assets/Resources/testing.txt",data);
    }
}
