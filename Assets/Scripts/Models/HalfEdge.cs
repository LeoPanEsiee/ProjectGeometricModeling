using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HalfEdge
{

    public int index;

    public Vertex sourceVertex;
    public HalfEdge prevEdge;
    public HalfEdge nextEdge;

    public HalfEdge twinEdge;

    public Face face;
}