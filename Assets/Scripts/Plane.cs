using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane
{
    public Vector3 normal;
    public float d;
    

    public Plane(Vector3 normal, float d)
    {
        this.normal = normal;
        this.d = d;
    }
}
