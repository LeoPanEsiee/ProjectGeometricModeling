using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex 
{
    public int index;
    public Vector3 vertex;


    public Vertex() {}
    public Vertex(Vector3 vertex)
    {
        this.vertex = vertex;
    }
    public Vertex(int index, Vector3 vertex)
    {
        this.index = index;
        this.vertex = vertex;
    }

    public Vertex(float x, float y, float z)
    {
        this.vertex = new Vector3(x, y, z);
    }

    public static Vertex operator+ (Vertex v1, Vertex v2)
    {
        return new Vertex(v1.vertex + v2.vertex);
    }

    public static Vertex operator/ (Vertex v, int i)
    {
        return new Vertex(v.vertex / i);
    }

    public static Vertex operator* (int i, Vertex v)
    {
        return new Vertex(i * v.vertex);
    }

}
