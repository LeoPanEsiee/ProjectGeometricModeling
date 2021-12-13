using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cylinder
{
    public Vector3 point1;
    public Vector3 point2;
    public float radius;

    public Cylinder(Vector3 p1, Vector3 p2, float r)
    {
        point1 = p1;
        point2 = p2;
        radius = r;
    }
}
