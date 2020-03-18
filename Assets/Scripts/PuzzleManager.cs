using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] public List<Puzzle> puzzlesList = new List<Puzzle>();
    public int puzzleListSize;
    public Level[] levelData;

    private void Start()
    {
        //ReadPuzzleData("TextFiles/4x2");
        //puzzleListSize = puzzlesList.Count;
    }

    public void ReadPuzzleData(string path)
    {
        //Load the text file
        //Code a string that gets the dimensions needed for current level and wave so the correct path is chosen
        TextAsset puzzleData = Resources.Load(path) as TextAsset;
        //Remove all of the new lines and spaces
        string data = puzzleData.text.Replace(Environment.NewLine, string.Empty);
        data = data.Replace(" ", string.Empty);
        data = data.Replace("\n",string.Empty);
        //Split the data into the individual puzzles
        string[] puzzles = data.Split(',');
        //Split the puzzles into their individual rows and populate the Puzzle class
        int counter = 0;
        foreach (string puzzle in puzzles)
        {
            Puzzle newPuzzle = new Puzzle();
            newPuzzle.InputPuzzleData(puzzles[counter].Split('-'));
            newPuzzle.SetDimensions();
            puzzlesList.Add(newPuzzle);
            counter++;
        }
        puzzleListSize = puzzlesList.Count;
    }

    public Puzzle GetRandomPuzzle()
    {
        int rand = Random.Range(0, puzzleListSize);
        return puzzlesList[rand];
    }

}
