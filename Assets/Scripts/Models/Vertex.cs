using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex 
{
    public Vector3 vertex;

    public Vertex(Vector3 vertex)
    {
        this.vertex = vertex;
    }

    public Vertex(float x, float y, float z)
    {
        this.vertex = new Vector3(x, y, z);
    }
}
