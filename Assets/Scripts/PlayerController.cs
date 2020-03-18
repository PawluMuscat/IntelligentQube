using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private bool tileMarked = false;
    [SerializeField] private Vector2 markedTileCoord;
    [SerializeField] private FloorCreation floorCreation;
    [SerializeField] private GameObject armatureMesh;
    [SerializeField] private FloorInfo markedTile;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private int mapWidth, mapLength;
    bool isFalling = false;
    private Animator animator;
    Vector3 startPos;
    public GameObject gameOverPanel;
    public TextMeshProUGUI LevelWaveEndScreen;


    private enum LookDir
    {
        forwards,
        backwards,
        left,
        right
    }

    private LookDir lookDir = LookDir.forwards; 

    void Start()
    {
        startPos = transform.position;
        animator = GetComponent<Animator>();
        if (floorCreation != null)
        {
            //Get the map width and length to get the contraints for the player movement (-1 for starting at 0);
            mapWidth = floorCreation.floorTileWidth - 1;
            mapLength = floorCreation.floorTileLength - 1;
        }
    }

    void Update()
    {
        if (mapLength != floorCreation.floorTileLength - 1)
        {
            mapLength = floorCreation.floorTileLength - 1;
        }
        if (mapWidth != floorCreation.floorTileWidth - 1)
        {
            mapWidth = floorCreation.floorTileWidth - 1;
        }

        //Get left and right movement
        float horizontalMovement = (Input.GetAxis("Horizontal") * movementSpeed) * Time.deltaTime;
        //Get up and down movement
        float verticalMovement = (Input.GetAxis("Vertical") * movementSpeed) * Time.deltaTime;

        //Rotate the mesh controller to make the player rotate towards the direction they are going.
        if (Input.GetAxis("Horizontal") > 0)
        {
            if (lookDir != LookDir.right)
            {
                armatureMesh.transform.rotation = Quaternion.LookRotation(Vector3.right);
                lookDir = LookDir.right;
            }
        }
        if (Input.GetAxis("Horizontal") < 0)
        {
            if (lookDir != LookDir.left)
            {
                armatureMesh.transform.rotation = Quaternion.LookRotation(Vector3.left);
                lookDir = LookDir.left;
            }
        }
        if (Input.GetAxis("Vertical") < 0)
        {
            if (lookDir != LookDir.backwards)
            {
                armatureMesh.transform.rotation = Quaternion.LookRotation(Vector3.back);
                lookDir = LookDir.backwards;
            }
        }
        if (Input.GetAxis("Vertical") > 0)
        {
            if (lookDir != LookDir.forwards)
            {
                armatureMesh.transform.rotation = Quaternion.LookRotation(Vector3.forward);
                lookDir = LookDir.forwards;
            }
        }
        //Use Normal activate/deactivate ability
        if (Input.GetButtonDown("Fire1"))
        {
            //When left click is pressed highlight the tile underneath the player if no other tiles are marked
            if (!tileMarked)
            {
                MarkFloorCube();
            }
            else if (tileMarked)
            {
                //If a tile is marked, unmark the tile and deactivate the cube above it 
                ActivateMarkedCube(markedTileCoord,false);
            }
        }
        //activate special ability
        if(Input.GetButtonDown("Fire2"))
        {
            //Special ability activates all of the special marked cubes (To array to make sure the list isnt changed throughout the loop)
            foreach (Vector2 coord in gameManager.specialBlockCoords.ToArray())
            {
                ActivateMarkedCube(coord,true);
            }
        }

        if(Input.GetButtonDown("Jump"))
        {
            gameManager.ReduceMovementTime();
        }
        if(Input.GetButtonUp("Jump"))
        {
            gameManager.RestoreMovementTime();
        }

        if (horizontalMovement != 0 || verticalMovement != 0)
        {
            animator.SetBool("IsMoving", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }
        if(isFalling)
        {
            animator.SetBool("IsFalling", true);
        }
        else
        {
            animator.SetBool("IsFalling", false);
        }
        transform.Translate(horizontalMovement, 0, verticalMovement);
        CheckFloorCube();
        //Constrains the player to the map

        if (transform.position.x < 0)
        {
            transform.position = new Vector3(0, transform.position.y, transform.position.z);
        }
        if (transform.position.x > mapWidth)
        {
            transform.position = new Vector3(mapWidth, transform.position.y, transform.position.z);
        }
        //If the player has fallen off end the game
        if(transform.position.y < 0.2)
        {
            LevelWaveEndScreen.text = "Level: " + (gameManager.level + 1) + " Wave: " + gameManager.wave;
            gameOverPanel.SetActive(true);
        }
        //Constrains the player if they aren't falling
        if (!isFalling)
        {
            if (transform.position.z > 0)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            }
            if (transform.position.z < -mapLength)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, -mapLength);
            }
            //safety for just incase the player gets stuck in a floor tile when running from falling cubes.
            if(transform.position.y < startPos.y)
            {
                transform.position = new Vector3(transform.position.x, startPos.y, transform.position.z);
            }
        }

    }

    private void ActivateMarkedCube(Vector2 markedCoord, bool isSpecial)
    {
        //Check if there is a cube above the marked tile and if there is deactivate it
        //find cube above marked tile
        CubeInfo cube = gameManager.GetCubeAboveMarkedTile(markedCoord);
        //If there is a cube above it Deactivate the cube
        if (cube != null)
        {
            StartCoroutine(gameManager.DeactivateCube(cube));
            gameManager.addedPauseTime += 0.8f;
        }
        //If it isn't a special trigger then unmark normally
        if (!isSpecial)
        {
            markedTile.UnmarkTile();
            markedTile = null;
            tileMarked = false;
        }
        else//Otherwise unmark as special
        {
            floorCreation.UnmarkAllSpecialBlocks();
            gameManager.specialBlockCoords.Clear();
        }
    }

    private void MarkFloorCube()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
        {
            if (hit.collider.tag == "Floor")
            {
                FloorInfo info = hit.collider.GetComponent<FloorInfo>();
                //Mark the tile
                markedTileCoord = info.GetCoord();
                markedTile = info;
                info.MarkTile();
                tileMarked = true;
            }
        }
    }

    private void CheckFloorCube()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
        {
            if (hit.collider.tag == "Floor")
            {
                FloorInfo info = hit.collider.GetComponent<FloorInfo>();
                if (info.isFalling)
                    isFalling = true;                 
                else
                    isFalling = false;
            }
        }
    }
}
