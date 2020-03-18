using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int level = 0;
    public int wave = 1;

    [SerializeField] int width;
    public int floorLength;
    [SerializeField] int waveLength;
    [SerializeField] int puzzleLength;

    [SerializeField] float waveWaitTime;
    [SerializeField] float pauseTimeBetweenRoll;
    [SerializeField] private float timeToMovePos = 0.8f;
    public float addedPauseTime = 0;

    [SerializeField]List<GameObject> activeWaveBlocks = new List<GameObject>();
    [SerializeField]List<GameObject> inactiveBlocks = new List<GameObject>();
    public List<GameObject> activePuzzleBlocks = new List<GameObject>();
    public List<Vector2> specialBlockCoords = new List<Vector2>();

    [SerializeField] Transform activeWaveHolder;
    [SerializeField] Transform activeCubeHolder;
    [SerializeField] Transform inactiveCubeHolder;
    [SerializeField] private TextMeshProUGUI blockCounterText;
    [SerializeField] private TextMeshProUGUI waveCounter;

    FloorCreation floorCreation;
    PuzzleManager puzzleManager;
    Level levelData;
    private bool puzzleActive = false;
    private bool waveActive = false;
    private bool waveFinished = false;
    private bool isPerfect = true;
    private bool movementFinished = true;
    
    [SerializeField] private int fallenBlockCounter;


    void Start()
    {
        blockCounterText.text = "Block Counter: " + fallenBlockCounter.ToString();
        floorCreation = gameObject.GetComponent<FloorCreation>();
        puzzleManager = gameObject.GetComponent<PuzzleManager>();
        LoadLevel();
        LoadWave(wave);   
    }

    private void Update()
    {
        if(puzzleActive && movementFinished)
        {
            StartCoroutine(MovePuzzleCubes());
        }
    }

    private void LoadLevel()
    {
        levelData = puzzleManager.levelData[level];
        width = (int)levelData.startingFloorSize.x;
        floorLength = (int)levelData.startingFloorSize.y;
        floorCreation.SetFloorSize(width, floorLength);
        floorCreation.GenerateFloor(floorLength, width);
    }

    private void LoadWave(int currentWave)
    {
        waveCounter.text = "Level: " + (level + 1) + " Wave: " + wave;
        waveLength = levelData.waveSize[currentWave - 1];
        //Get puzzle list for the correct width x length
        //Get puzzle length by getting the wave size and dividing by the amount of puzzles in the wave.
        puzzleLength = levelData.waveSize[currentWave -1] / levelData.puzzlesPerWave;
        //Get the path for the correct puzzles for the wave.
        string path = "TextFiles/" + width.ToString() + "x" + puzzleLength.ToString();
        //load the puzzles into the puzzle manager
        puzzleManager.ReadPuzzleData(path);
        StartCoroutine("LoadWavePerLine");     
    }

    private void LoadPuzzle()
    {
        //Only allow a new puzzle to be loaded if there isnt already a puzzle.
        
        if (!puzzleActive && waveActive)
        {
            isPerfect = true;
            waveFinished = false;
            //Get the data for a random puzzle from the correct data we initilized earlier
            Puzzle puzzle = puzzleManager.GetRandomPuzzle();
            //Get the amount of blocks needed for the current puzzle
            int blocksNeededForPuzzle = puzzleLength * width;
            //Take the blocks from the active wave blocks and put them in active puzzle
            List<GameObject> tempBlockList = activeWaveBlocks.GetRange(activeWaveBlocks.Count - blocksNeededForPuzzle, blocksNeededForPuzzle);
            activePuzzleBlocks.AddRange(tempBlockList);
            activeWaveBlocks.RemoveRange(activeWaveBlocks.Count - blocksNeededForPuzzle, blocksNeededForPuzzle);
            //Put all the Puzzle cubes into the cube holder for clean hierarchy
            foreach (GameObject cube in activePuzzleBlocks)
            {
                cube.transform.parent = activeCubeHolder;
            }
            string[] puzzleInfo = puzzle.puzzleData;
            //Activate each of the active blocks and turn them into the correct type based on the puzzle data
            for (int i = 0; i < puzzleInfo.Length; i++)
            {

                for (int j = 0; j < puzzleInfo[i].Length; j++)
                {
                    CubeInfo cubeInfo = activePuzzleBlocks[(i * puzzleInfo[i].Length) + j].GetComponentInChildren<CubeInfo>();
                    if (cubeInfo != null)
                    {
                        cubeInfo.isActive = true;
                        char tmpChar = puzzleInfo[i][j];
                        cubeInfo.SetType(int.Parse(tmpChar.ToString()));
                    }
                    else
                    {
                        Debug.Log("Error: No CubeInfo on GameObject");
                    }
                }
            }
            puzzleActive = true;
        }
    }

    private IEnumerator LoadWavePerLine()
    {
        //Set-up the wave blocks and put them in the activeWaveBlocks list (back to front)
        for (int i = 0; i < waveLength; i++)
        {
            yield return new WaitForSeconds(waveWaitTime);
            for (int j = 0; j < width; j++)
            {
                Vector3 posToSpawn = new Vector3(j, 0.5f, -i);
                //Try to get the object from the pool
                GameObject cubeGO = GetObjectFromPool();
                if(cubeGO == null)
                {
                    //If there isn't an object available in the pool instantiate one
                    cubeGO = Instantiate(Resources.Load("Cube") as GameObject, posToSpawn, Quaternion.identity, activeWaveHolder);
                }
                else
                {
                    //If there is an object in the pool then move it to the correct position
                    cubeGO.transform.position = posToSpawn;
                    //Make it into a normal block for initilisation
                    cubeGO.GetComponentInChildren<CubeInfo>().SetType(0);
                    //Make it move up like it would on start
                    cubeGO.GetComponent<CubeMovement>().MoveUp();
                }
                cubeGO.GetComponentInChildren<CubeInfo>().SetCoord(j, i);
                activeWaveBlocks.Add(cubeGO);
            }            
        }
        waveActive = true;
        yield return new WaitForSeconds(1);
        LoadPuzzle();
    }

    private GameObject GetObjectFromPool()
    { 
        if(inactiveBlocks.Count <= 0)
        {
            return null;
        }
        else
        {
            GameObject objectFromPool = inactiveBlocks[0];
            inactiveBlocks.Remove(objectFromPool);
            return objectFromPool;

        }
    }

    public CubeInfo GetCubeAboveMarkedTile(Vector2 markedTilePos)
    {
        foreach (GameObject cube in activePuzzleBlocks)
        {
            if(cube.GetComponentInChildren<CubeInfo>().coord == markedTilePos)
            {            
                return cube.GetComponentInChildren<CubeInfo>(); 
            }
        }
        return null;
    }

    public IEnumerator DeactivateCube(CubeInfo cube)
    {
        //Remove the deactivated cube from the list
        GameObject parent = cube.transform.parent.gameObject;
        
        //deactivate the block and move it to a holder
        cube.DeactivateCube();
        while(parent.GetComponent<CubeMovement>().isMoving)
        {
            yield return null;
        }
        activePuzzleBlocks.Remove(parent);
        inactiveBlocks.Add(parent);
        parent.transform.position = inactiveCubeHolder.position;
        parent.transform.parent = inactiveCubeHolder;

        //Check the type of block that was deactivated - if it was special or forbidden do extra.
        //If forbidden cube was deactivated -3 to block counter
        if(cube.typeOfCube == CubeInfo.CubeType.forbidden)
        {
            DeductFromFallenBlockCounter(3);
        }
        //If it was special mark the 9 floor tiles around the cubes pos as marked.
        if(cube.typeOfCube == CubeInfo.CubeType.special)
        {
            MarkFloorCubesForSpecial(cube.coord);
        }
        CheckIfPuzzleIsFinished();
    }

    private void CheckIfPuzzleIsFinished()
    {
        //If all the current puzzle's blocks are no longer in play
        if (activePuzzleBlocks.Count <= 0)
        {
            puzzleActive = false;
            //Check to see if the puzzle was 'perfect' if so add a length of floor
            if (isPerfect)
            {
                //Turn is perfect to false to make sure only 1 can be called after each puzzle
                isPerfect = false;
                floorCreation.AddFloorToEnd();
            }
            //Check to see if there are still more blocks in the wave
            if(activeWaveBlocks.Count > 0)
            {
                //Load the next puzzle
                LoadPuzzle();
            }
            else
            {
                //Have a second check to make sure this can only be called once if being called by more than 1 coroutine at a time
                if (!waveFinished)
                {
                    waveFinished = true;
                    waveActive = false;
                    wave++;
                    LoadWave(wave);
                }
            }

        }
    }

    private void DeductFromFallenBlockCounter(int amountToDeduct)
    {
        //Deduct from the block counter
        fallenBlockCounter -= amountToDeduct;
        //Puzzle Can't be 'perfect'
        isPerfect = false;
        int remainder = 0;
        if(fallenBlockCounter <= 0)
        {
            //Remove the last bit of floor
            floorCreation.RemoveFloorCall();
            //Get the remainder if there is any
            remainder = fallenBlockCounter;
            //Reset the counter
            fallenBlockCounter = 3;
            //Take off the remainder to accomodate for overflow
            fallenBlockCounter += remainder;
            //Make sure the length is accurate
            floorLength--;
            //floorCreation.floorTileLength--;
            //Recall to make sure overflow is counted
            DeductFromFallenBlockCounter(0);
        }
        blockCounterText.text = "Block Counter: " + fallenBlockCounter.ToString();
    }

    public void CubeOutOfBounds(GameObject puzzleCube)
    {
        //Remove the block thats fallen from the active puzzle list
        activePuzzleBlocks.Remove(puzzleCube);
        //Put it into the inactive cube list
        inactiveBlocks.Add(puzzleCube);
        //Set the position and parent to the holders 
        puzzleCube.transform.position = inactiveCubeHolder.position;
        puzzleCube.transform.parent = inactiveCubeHolder;
        //Make sure block counter is only deducted from by the right blocks
        CubeInfo cubeInfo = puzzleCube.GetComponentInChildren<CubeInfo>();
        if (cubeInfo.typeOfCube == CubeInfo.CubeType.normal || cubeInfo.typeOfCube == CubeInfo.CubeType.special)
        {
            DeductFromFallenBlockCounter(1);
        }
        CheckIfPuzzleIsFinished();
    }

    void MarkFloorCubesForSpecial(Vector2 specialCoord)
    {
        floorCreation.SpecialMark(specialCoord);
    }

    public void ReduceMovementTime()
    {
        timeToMovePos = 0.1f;
        pauseTimeBetweenRoll = 0.1f;
    }

    public void RestoreMovementTime()
    {
        timeToMovePos = 1.6f;
        pauseTimeBetweenRoll = 0.8f;
    }

    private IEnumerator MovePuzzleCubes()
    {
        movementFinished = false;
        foreach (GameObject cube in activePuzzleBlocks)
        {
            CubeMovement cubeMovement = cube.GetComponent<CubeMovement>();
            cubeMovement.MoveCube(timeToMovePos);
        }
        //while the blocks are moving wait
        while (activePuzzleBlocks[0].GetComponent<CubeMovement>().isMoving)
        {
            //Only stay in the while loop aslong as there is atleast 1 active block
            if (activePuzzleBlocks.Count <= 0)
            {
                break;
            }
            yield return null;
        }
        //Once the blocks have stopped moving wait x seconds before allowing the blocks to move again
        float counter = 0;
        float pauseTimer = pauseTimeBetweenRoll;
        while (counter < pauseTimer)
        {
            counter += Time.deltaTime;
            //Allow for time to be added to the pause for when a block is activated
            pauseTimer += addedPauseTime;
            addedPauseTime = 0;
            yield return null;
        }
        movementFinished = true;
    }
}
