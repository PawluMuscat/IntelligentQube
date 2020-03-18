using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMovement : MonoBehaviour
{
    [SerializeField] private float timeToMovePos;
    [SerializeField] private float timeToGetToStartPos;
    [SerializeField] private Vector3 moveToPos;
    public bool isMoving;
    private CubeInfo cubeInfo;
    GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        cubeInfo = GetComponentInChildren<CubeInfo>();
        StartCoroutine(MoveUpAtStart(timeToGetToStartPos));
    }

    void Update()
    {
        //Testing 
        if (Input.GetKeyDown(KeyCode.L))
        {
            MoveCube(timeToMovePos);
        }
    }

    //Call this in a List of activated cubes to move the activated cubes
    public void MoveCube(float timeToMoveIntoPos)
    {
        if(!isMoving && cubeInfo.isActive)
        {
            StartCoroutine(MoveTorward(transform, timeToMoveIntoPos));
        }
    }

    public void StartDeactivateCoroutine()
    {
        StartCoroutine(MoveDownAtDiactivate(timeToGetToStartPos));
    }

    public void MoveUp()
    {
        StartCoroutine(MoveUpAtStart(timeToGetToStartPos));
    }

    private IEnumerator MoveUpAtStart(float duration)
    {
        //Makes the cube move upwards to the start position
        if(isMoving)
        {
            yield break;
        }
        isMoving = true;
        float counter = 0;
        Vector3 currentPos = transform.position;
        Vector3 posToMoveTo = transform.position + new Vector3(0,1,0);
        while (counter < duration)
        {
            counter += Time.deltaTime;
            transform.position = Vector3.Lerp(currentPos, posToMoveTo, counter / duration);
            yield return null;
        }
        isMoving = false;

    }
    private IEnumerator MoveDownAtDiactivate(float duration)
    {
        if(isMoving)
        {
            yield break;
        }
        isMoving = true;
        float counter = 0;
        Vector3 currentPos = transform.position;
        Vector3 posToMoveTo = transform.position - new Vector3(0,1,0);
        while (counter < duration)
        {
            counter += Time.deltaTime;
            transform.position = Vector3.Lerp(currentPos, posToMoveTo, counter / duration);
            yield return null;
        }
        isMoving = false;
    }

    private IEnumerator MoveTorward(Transform startPos,float duration)
    {
        //Get the position 1 unit towards the player
        moveToPos = transform.position + Vector3.back;
        cubeInfo.SetCoord((int)moveToPos.x, (int)-moveToPos.z);
        Vector3 rotDir = transform.right;
        if (isMoving)
        {
            yield break;
        }
        isMoving = true;
        //Get the rotation speed (Angle to rotate/ time to rotate)
        float rotSpeed = -90 / duration;
        float counter = 0;
        float deltaAngle = 0;
        Vector3 currentPos = startPos.position;
        Quaternion startRot = transform.rotation;
        while (counter < duration)
        {
            //Slowly increment the delta angle for rotation
            deltaAngle += rotSpeed * Time.deltaTime;
            //make sure the delta angle is smaller than the overall angle that is to be rotated
            deltaAngle = Mathf.Min(deltaAngle, -90);
            //Increment the counter using delta time
            counter += Time.deltaTime;
            //Slowly move the cube towards the player 
            transform.position = Vector3.Lerp(currentPos, moveToPos, counter / duration);
            //slowly rotate the cube towards the play to make it look like its moving towards the player
            transform.rotation = startRot * Quaternion.AngleAxis(deltaAngle, rotDir);
            yield return null;
        }
        CheckIfCubeIsPastLevelLength();
        //Makes sure the rotation is a perfect 90°
        transform.rotation = startRot * new Quaternion(-90, 0, 0, 0);
        isMoving = false;
        //if the cube has been deactivated halfway through moving, move down after finished moving
        if (!cubeInfo.isActive)
        {
            StartCoroutine(MoveDownAtDiactivate(timeToGetToStartPos));
        }
    }

    private void CheckIfCubeIsPastLevelLength()
    {
        if (cubeInfo.coord.y > gameManager.floorLength -1)
        {
            Vector3 currentPos = transform.position;
            StartCoroutine(DropCube(timeToMovePos, currentPos));
        }
    }

    private IEnumerator DropCube(float duration, Vector3 currentPos)
    {
        float counter = 0;
        Vector3 posToMoveTo = currentPos - new Vector3(0, 5, 0);
        while (counter < duration)
        {
            counter += Time.deltaTime;
            transform.position = Vector3.Lerp(currentPos, posToMoveTo, counter / duration);
            yield return null;
        }
        gameManager.CubeOutOfBounds(gameObject);
    }
}
