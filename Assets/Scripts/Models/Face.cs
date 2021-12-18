using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face
{
    public int index;
    public HalfEdge edge;

    public Face(HalfEdge edge)
    {
        this.edge = edge;
    }

    public Face(int i)
    {
        this.index = i;
    }

    public Face(int index, HalfEdge edge)
    {
        this.index = index;
        this.edge = edge;
    }

    public void GetFaceVerticesAndEdges(out List<Vertex> vertices, out List<HalfEdge> edges)
    {
        edges = new List<HalfEdge>();
        vertices = new List<Vertex>();

        HalfEdge currentEdge = edge;

        while(currentEdge.nextEdge != edge)
        {
            edges.Add(currentEdge);
            vertices.Add(currentEdge.sourceVertex);

            currentEdge = currentEdge.nextEdge;
        }
    }

}
