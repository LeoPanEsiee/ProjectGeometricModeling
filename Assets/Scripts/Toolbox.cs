using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toolbox : MonoBehaviour
{
    MeshGenerator mg = new MeshGenerator();

    public static List<HalfEdge> VertexFaceToHalfEdges(Vector3[] vertices, int[] faces)
    {
        int index = 0;
        List<HalfEdge> res = new List<HalfEdge>();

        for (int i = 0; i < faces.Length;)
        {
            // On part du principe qu'une face est égale à un quad, qui demande de généré 4 HalfEdges
            
            // Le premier edge / première vertices initialisent la face
            Vertex v1 = new Vertex(vertices[faces[i++]]);
            HalfEdge e1 = new HalfEdge(index++, v1, null, null, null, null);
            Face currentFace = new Face(e1);
            e1.face = currentFace;

            // Les trois autres sont plus rapides à construire

            Vertex v2 = new Vertex(vertices[faces[i++]]);
            HalfEdge e2 = new HalfEdge(index++, v1, null, null, null, currentFace);
            e1.nextEdge = e2;
            e2.prevEdge = e1;

            Vertex v3 = new Vertex(vertices[faces[i++]]);
            HalfEdge e3 = new HalfEdge(index++, v1, null, null, null, currentFace);
            e2.nextEdge = e3;
            e3.prevEdge = e2;

            Vertex v4 = new Vertex(vertices[faces[i++]]);
            HalfEdge e4 = new HalfEdge(index++, v1, null, null, null, currentFace);
            e3.nextEdge = e4;
            e4.prevEdge = e3;
            e4.nextEdge = e1;
            e1.prevEdge = e4;

            //On doit finalement créer les twin edges, aka les mêmes edges dans un sens contraire
            HalfEdge t1 = new HalfEdge(index++, v2, null, null, e1, currentFace);
            HalfEdge t2 = new HalfEdge(index++, v3, null, t1, e2, currentFace);
            HalfEdge t3 = new HalfEdge(index++, v4, null, t2, e3, currentFace);
            HalfEdge t4 = new HalfEdge(index++, v1, t1, t3, e4, currentFace);

            // On les complète
            t1.prevEdge = t2;
            t1.nextEdge = t4;
            t2.prevEdge = t3;
            t3.prevEdge = t4;

            // On complète nos halfEdges de base
            e1.twinEdge = t1;
            e2.twinEdge = t2;
            e3.twinEdge = t3;
            e4.twinEdge = t4;

            //Enfin, on ajoute tout à la liste
            res.Add(e1);
            res.Add(e2);
            res.Add(e3);
            res.Add(e4);
            res.Add(t1);
            res.Add(t2);
            res.Add(t3);
            res.Add(t4);

        }
            
        return res;

    }
}
