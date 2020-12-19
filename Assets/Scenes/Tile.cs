using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{

    int x;
    int y;
    Unit unit;

    public Tile(int x, int y)
    {
        this.x = x;
        this.y = y;
        unit = null;
    }

    public void setLoc(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void setX(int x) { this.x = x; }
    public void setY(int y) { this.y = y; }

    public int getX() { return x; }
    public int getY() { return y; }

    public Transform getTransform() { return this.gameObject.transform; }

    public void setMat(Material mat)
    {
        this.gameObject.GetComponent<MeshRenderer>().material = mat;
    }

    public void setUnit(Unit unit)
    {
        this.unit = unit;
    }

    public Unit getUnit() { return unit; }
}
