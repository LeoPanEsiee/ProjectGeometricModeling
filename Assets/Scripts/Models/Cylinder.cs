using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cylinder
{
    public Vector3 pt1;
    public Vector3 pt2;
    public float radius;

    public Cylinder(Vector3 p1, Vector3 p2, float r)
    {
        pt1 = p1;
        pt2 = p2;
        radius = r;
    }
}
