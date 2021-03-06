using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UIElements;

public class Face
{
    HalfEdge edge;

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
