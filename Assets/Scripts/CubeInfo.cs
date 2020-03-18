using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeInfo : MonoBehaviour
{
    [SerializeField] public Vector2 coord;

    public enum CubeType
    {
        normal,
        special,
        forbidden
    }

    [SerializeField] public CubeType typeOfCube;

    [SerializeField]private Material material;

    public bool isActive = false;

    CubeMovement cubeMovement;

    void Start()
    {
        cubeMovement = GetComponentInParent<CubeMovement>();
        SetMaterial();
    }

    public void SetCoord(int xPos, int zPos)
    {
        coord.x = xPos;
        coord.y = zPos;
    }

    public void SetType(int cubeInfo)
    {
        typeOfCube = (CubeType)cubeInfo;
        SetMaterial();
    }

    void SetMaterial()
    {
        switch (typeOfCube)
        {
            //Change the look of the cube based on what type it is.
            case CubeType.normal:
                material = Resources.Load("Materials/Cube_Normal") as Material;
                break;
            case CubeType.forbidden:
                material = Resources.Load("Materials/Cube_Forbidden") as Material;
                break;
            case CubeType.special:
                material = Resources.Load("Materials/Cube_Special") as Material;
                break;
            default:
                material = Resources.Load("Materials/Cube_Normal") as Material;
                break;
        }
        GetComponent<Renderer>().material = material;
    }

    public void DeactivateCube()
    {
        isActive = false;
        cubeMovement.StartDeactivateCoroutine();
    }
}
