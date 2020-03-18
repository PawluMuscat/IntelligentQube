using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorCreation : MonoBehaviour
{
    public int floorTileWidth;
    public int floorTileLength;

    [SerializeField] Transform activeFloorCubeHolder;
    [SerializeField] Transform inactiveFloorCubeHolder;
    [SerializeField] List<GameObject> floorCubes;
    [SerializeField] List<GameObject> inactiveFloorCubes;
    bool isMoving;
    int counter = 0;
    [SerializeField] float fallDuration;
    private GameManager gameManager;

    private void Start()
    {
        gameManager = GetComponent<GameManager>();
    }

    public void GenerateFloor(int floorLength, int floorWidth)
    {
        for (int i = 0; i < floorLength; i++)
        {
            for (int j = 0; j < floorWidth; j++)
            {
                Vector3 posToSpawn = new Vector3(j, 0, -i);
                AddFloorCube(posToSpawn);
            }
        }
    }

    public void SetFloorSize(int width, int length)
    {
        floorTileWidth = width;
        floorTileLength = length;
    }

    public void RemoveFloorCall()
    {
        List<GameObject> floorToRemove = floorCubes.GetRange(floorCubes.Count - floorTileWidth, floorTileWidth); ;
        StartCoroutine(RemoveFloor(fallDuration, floorToRemove));
    }

    private IEnumerator RemoveFloor(float duration, List<GameObject> tempList)
    {

        //Move removed floor pieces into a inactive holder to be used at a later time
        floorCubes.RemoveRange(floorCubes.Count - floorTileWidth, floorTileWidth);
        inactiveFloorCubes.AddRange(tempList);

        //Move the cubes down to look like they're falling
        foreach (GameObject floorCube in tempList)
        {
            isMoving = true;
            float counter = 0;
            Vector3 currentPos = floorCube.transform.position;
            Vector3 posToMoveTo = floorCube.transform.position - new Vector3(0, 5, 0);
            StartCoroutine(FloorDrop(counter, duration, floorCube, currentPos, posToMoveTo));
            yield return new WaitForSeconds(.08f);
            isMoving = false;
        }

    }

    private IEnumerator FloorDrop(float counter, float duration, GameObject floorCube, Vector3 currentPos, Vector3 posToMoveTo)
    {
        FloorInfo info = floorCube.GetComponent<FloorInfo>();
        info.isFalling = true;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            floorCube.transform.position = Vector3.Lerp(currentPos, posToMoveTo, counter / duration);
            yield return null;
        }
        floorCube.transform.parent = inactiveFloorCubeHolder;
        info.isFalling = false;
        //Move floor cubes to the holders position 
        floorCube.transform.position = inactiveFloorCubeHolder.transform.position;
        CalcFloorLength();
    }

    private void CalcFloorLength()
    {
        //counts how many blocks have fallen when 'Width' amount has fallen the length decreases
        counter++;
        if (counter >= floorTileWidth)
        {
            floorTileLength = floorCubes.Count / floorTileWidth;
            counter = 0;
        }
    }

    public void AddFloorToEnd()
    {
        //Add to the length of the floor - used when the player has completed a puzzle 'perfectly'
        for (int i = 0; i < floorTileWidth; i++)
        {
            Vector3 floorSpawnPos = new Vector3(i, 0, -floorTileLength);
            AddFloorCube(floorSpawnPos);
        }
        floorTileLength++;
        gameManager.floorLength++;
    }

    private void AddFloorCube(Vector3 posToSpawn)
    {
        GameObject floorGO = null;
        //Check to see if there are any inactive floor cubes
        if (inactiveFloorCubes.Count <= 0)
        {
            //If there aren't instantiate a new floor cube
            floorGO = Instantiate(Resources.Load("FloorCube") as GameObject, posToSpawn, Quaternion.identity, activeFloorCubeHolder);
        }
        else //If the inactive floor cube list isnt empty grab a floorcube from there
        {
            floorGO = inactiveFloorCubes[0];
            inactiveFloorCubes.Remove(floorGO);
            floorGO.transform.position = posToSpawn;
        }
        floorGO.GetComponent<FloorInfo>().SetCoord((int)posToSpawn.x, (int)-posToSpawn.z);
        floorCubes.Add(floorGO);
    }

    public void SpecialMark(Vector2 coord)
    {
        //Get the coordinates of the special cube that was activated
        int xCoord = (int)coord.x;
        int yCoord = (int)coord.y;
        //Cycle through the 9 coords surrounding and including the pos of the special cube
        for (int yPos = yCoord - 1; yPos < yCoord + 2; yPos++)
        {
            for (int xPos = xCoord - 1; xPos < xCoord + 2; xPos++)
            {
                Vector2 tempCoord = new Vector2(xPos, yPos);
                foreach (GameObject floor in floorCubes)
                {
                    //If there is a floor cube in the coord mark it as a special tile
                    FloorInfo floorInfo = floor.GetComponent<FloorInfo>();
                    if(floorInfo.GetCoord() == tempCoord)
                    {
                        floorInfo.SpecialMarkTile();
                        //Add the coord to a list so that it can be used to find blocks above the floor tiles to activate
                        if (!gameManager.specialBlockCoords.Contains(tempCoord))
                        {
                            gameManager.specialBlockCoords.Add(tempCoord);
                        }
                    }
                }
            }
        }
    }

    public void UnmarkAllSpecialBlocks()
    {
        foreach (GameObject floor in floorCubes)
        {
            FloorInfo floorInfo = floor.GetComponent<FloorInfo>();
            if(floorInfo != null)
            {
                if(floorInfo.isSpecialMarked)
                {
                    floorInfo.SpecialUnmarkTile();
                }
            }
        }
    }
}
