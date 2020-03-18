using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorInfo : MonoBehaviour
{

    [SerializeField] private Vector2 coord;
    [SerializeField] private GameObject tileMarker;
    [SerializeField] private GameObject tileSpecialMarker;
    [SerializeField] private bool isMarked;
    public bool isFalling = false;
    public bool isSpecialMarked = false;

    public Vector2 GetCoord()
    {
        return coord;
    }

    public void SetCoord(int xCoord, int zCoord)
    {
        coord.x = xCoord;
        coord.y = zCoord;
    }

    public void MarkTile()
    {
        isMarked = true;
        tileMarker.SetActive(true);
        //If the tile is already special marked, prioritize normal marking
        if(isSpecialMarked)
        {
            tileSpecialMarker.SetActive(false);
        }
    }

    public void SpecialMarkTile()
    {
        isSpecialMarked = true;
        tileSpecialMarker.SetActive(true);
    }

    public void UnmarkTile()
    {
        isMarked = false;
        tileMarker.SetActive(false);
        //If it was previously special marked re-activate the special marker
        if(isSpecialMarked)
        {
            tileSpecialMarker.SetActive(true);
        }
    }

    public void SpecialUnmarkTile()
    {
        isSpecialMarked = false;
        tileSpecialMarker.SetActive(false);
    }
}
