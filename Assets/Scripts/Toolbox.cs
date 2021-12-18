using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toolbox : MonoBehaviour
{
    MeshGenerator mg = new MeshGenerator();

    #region Vertice Manager
    public static List<HalfEdge> GetHalfEdges(Vertex vertice, List<HalfEdge> halfEdges)
    {
        List<HalfEdge> listHalfEdgesFromVertex = new List<HalfEdge>();
        for (int i = 0; i < halfEdges.Count; i++)
        {
            if (halfEdges[i].sourceVertex.vertex == vertice.vertex)
            {
                listHalfEdgesFromVertex.Add(halfEdges[i]);
            }
        }
        return listHalfEdgesFromVertex;
    }

    public static List<Face> GetFaces(Vertex vertice, List<HalfEdge> halfEdges)
    {
        List<Face> faces = new List<Face>();

        List<HalfEdge> listHalfEdgesFromVertex = GetHalfEdges(vertice, halfEdges);
        foreach(var halfEdge in listHalfEdgesFromVertex)
        {
            faces.Add(halfEdge.face);
        }
        return faces;
    }

    public static int Valence(Vertex vertice, List<HalfEdge> halfEdges)
    {
        return GetFaces(vertice, halfEdges).Count;
    }

    #endregion

    #region Half Edge Manager
    public static void SetTwinEdges(List<HalfEdge> halfEdges)
    {
        List<HalfEdge> visitedHalfEdges = new List<HalfEdge>();
        foreach(var halfEdge in halfEdges)
        {
            if (visitedHalfEdges.Contains(halfEdge))
            {
                continue;
            }
            else
            {
                foreach(var face in GetFaces(halfEdge.sourceVertex, halfEdges))
                {
                    HalfEdge twinEdge = face.edge;
                    for(int i = 0; i < 4; i++)
                    {
                        if(Equals(twinEdge, halfEdge))
                        {
                            Vertex halfEdgeSourceVertex = halfEdge.sourceVertex;
                            Vertex halfEdgeNextSourceVertex = halfEdge.nextEdge.sourceVertex;
                            Vertex twinEdgeSourceVertex = twinEdge.sourceVertex;
                            Vertex twinEdgeNextSourceVertex = twinEdge.nextEdge.sourceVertex;

                            if(halfEdgeSourceVertex == twinEdgeNextSourceVertex && halfEdgeNextSourceVertex == twinEdgeSourceVertex)
                            {
                                halfEdge.twinEdge = twinEdge;
                                twinEdge.twinEdge = halfEdge;
                                visitedHalfEdges.Add(halfEdge);
                                visitedHalfEdges.Add(twinEdge);
                            }
                        }
                        twinEdge = twinEdge.nextEdge;
                    }
                }
            }
        }
    }

    #endregion

    #region Conversions
    public static int[] TrianglesToQuads(Mesh mesh)
    {
        int[] triangles = mesh.triangles;
        int[] quads = new int[triangles.Length / 6 * 4];

        int index = 0;

        for(int i = 0; i < triangles.Length; i++)
        {
            quads[index++] = triangles[i++];
            quads[index++] = triangles[i++];
            quads[index++] = triangles[i++];
            i += 2;
            quads[index++] = triangles[i++];
        }

        return quads;
    }

    public static Tuple<List<HalfEdge>, List<Face>, List<Vertex>> VertexFaceToHalfEdges(Mesh mesh)
    {
        List<HalfEdge> halfEdges = new List<HalfEdge>();
        List<Face> faces = new List<Face>();
        List<Vertex> vertices = new List<Vertex>();

        var list = new Tuple<List<HalfEdge>, List<Face>, List<Vertex>>(halfEdges, faces, vertices);

        //On gère la liste des vertices
        for(int i = 0; i < mesh.vertices.Length; i++)
        {
            vertices.Add(new Vertex(i, mesh.vertices[i]));
        }

        int[] facesIndex = TrianglesToQuads(mesh);

        int index = 0;
        for (int i = 0; i < facesIndex.Length / 4 ; i++)
        {
            // On part du principe qu'une face est égale à un quad, qui demande de généré 4 HalfEdges
            
            Vertex v1 = vertices[facesIndex[index]];
            Vertex v2 = vertices[facesIndex[index + 1]];
            Vertex v3 = vertices[facesIndex[index + 2]];
            Vertex v4 = vertices[facesIndex[index + 3]];
            
            HalfEdge e1 = new HalfEdge(index, v1, null, null, null, null);
            
            // On gère la liste des faces
            Face currentFace = new Face(index/4, e1);
            e1.face = currentFace;
            faces.Add(currentFace);

            HalfEdge e2 = new HalfEdge(index + 1, v2, e1, null, null, currentFace);
            HalfEdge e3 = new HalfEdge(index + 2, v3, e2, null, null, currentFace);
            HalfEdge e4 = new HalfEdge(index + 3, v4, e3, e1, null, currentFace);
            
            e1.prevEdge = e4;

            e1.nextEdge = e2;
            e2.nextEdge = e3;
            e3.nextEdge = e4;


            //On doit finalement créer les twin edges, aka les mêmes edges dans un sens contraire
            HalfEdge t1 = new HalfEdge(index++, v2, null, null, e1, currentFace);
            HalfEdge t2 = new HalfEdge(index++, v3, null, t1, e2, currentFace);
            HalfEdge t3 = new HalfEdge(index++, v4, null, t2, e3, currentFace);
            HalfEdge t4 = new HalfEdge(index++, v1, t1, t3, e4, currentFace);

            // On les complète
            t1.nextEdge = t4;

            t1.prevEdge = t2;
            t2.prevEdge = t3;
            t3.prevEdge = t4;

            // On complète nos halfEdges de base
            e1.twinEdge = t1;
            e2.twinEdge = t2;
            e3.twinEdge = t3;
            e4.twinEdge = t4;

            //Enfin, on ajoute tout à la liste des vertices
            halfEdges.Add(e1);
            halfEdges.Add(e2);
            halfEdges.Add(e3);
            halfEdges.Add(e4);
            halfEdges.Add(t1);
            halfEdges.Add(t2);
            halfEdges.Add(t3);
            halfEdges.Add(t4);

        }
        return list;
    }

    public static Mesh HalfHedgesToVertexFace(List <HalfEdge> halfEdges)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> quads = new List<int>();

        foreach(var halfEdge in halfEdges)
        {
            vertices.Add(halfEdge.sourceVertex.vertex);
            quads.Add(halfEdge.face.edge.sourceVertex.index);
            quads.Add(halfEdge.face.edge.nextEdge.sourceVertex.index);
            quads.Add(halfEdge.face.edge.nextEdge.nextEdge.sourceVertex.index);
            quads.Add(halfEdge.face.edge.nextEdge.nextEdge.nextEdge.sourceVertex.index);
        }

        mesh.vertices = vertices.ToArray();
        mesh.SetIndices(quads, MeshTopology.Quads, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }

    #endregion

    #region Subdivision Manager : Points
    public static Vertex FacePoints(Face face)
    {
        HalfEdge currentHedge = face.edge;
        Vertex res = currentHedge.sourceVertex;

        for(int i = 0; i < 3; i++)
        {
            currentHedge = currentHedge.nextEdge;
            res += currentHedge.sourceVertex;
        }

        return res / 4;

    }

    public static Vertex EdgePoints(HalfEdge halfEdge)
    {
        Vertex edgePoint = halfEdge.sourceVertex;
        edgePoint += halfEdge.nextEdge.sourceVertex;

        if (halfEdge.twinEdge == null)
        {
            return edgePoint / 2;
        }
        else
        {
            edgePoint += FacePoints(halfEdge.face);
            edgePoint += FacePoints(halfEdge.twinEdge.face);

            return edgePoint / 4;
        }
    }

    public static Vertex VertexPoints(Vertex vertice, List<HalfEdge> halfEdges)
    {
        //On crée une liste des HalEdges adjacentes à notre vertice
        List<HalfEdge> listHalfEdgesFromVertex = GetHalfEdges(vertice, halfEdges);

        // On crée une liste des faces adjacentes à notre vertice
        List<Face> listFaces = new List<Face>();

        for(int i = 0; i < listHalfEdgesFromVertex.Count; i++)
        {
            if (listHalfEdgesFromVertex[i].face != null)
            {
                listFaces.Add(listHalfEdgesFromVertex[i].face);
            }
        }

        int n = Valence(vertice, halfEdges);

        if(n >= 3)
        {

            // Q : moyenne des Face Points

            Vertex Q = new Vertex();
            for(int i = 0; i < listFaces.Count; i++)
            {
                Q += FacePoints(listFaces[i]);
            }

            Q /= listFaces.Count;

            // R : moyenne des Mid-Edge Points

            Vertex R = new Vertex();
            for (int i = 0; i < listHalfEdgesFromVertex.Count; i++)
            {
                R += (listHalfEdgesFromVertex[i].sourceVertex + listHalfEdgesFromVertex[i].nextEdge.sourceVertex) / 2;
            }

            R /= listHalfEdgesFromVertex.Count;

            return (Q + 2 * R + (n - 3) * vertice) / n;
        }
        else
        {
            return vertice;
        }
    }

    #endregion

    #region Subdivision Manager : Edges

    public static HalfEdge SplitEdge(HalfEdge halfEdge, Vertex vertice)
    {
        HalfEdge newHalfEdge = new HalfEdge(vertice, halfEdge, halfEdge.nextEdge, null, halfEdge.face);
        halfEdge.nextEdge = newHalfEdge;

        return newHalfEdge;
    }

    public static void ConnectEdgePointToFacePoint(Vertex facePoint, Face face, HalfEdge newHalfEdge, List<HalfEdge> hEdges)
    {
        // En donnant ici une face en construction avec ses edges ainsi que les 2 points supplémentaires, 
        // on peut facilement refermer la face et la compléter tout en veillant à ajouter les nouvelles edges à la liste

        // On est dans cette situation :
        /*
         *    EdgePoint       FacePoint
         *       .                .
         *       |
         *       |
         *       |       Face
         *       |
         *       |
         *       |________________
         *          newHalfEdge
         * 
         * 
         *  Il nous faut donc créer les edges supplémentaires en gérer les prevEdge/nextEdge
         * 
         */

        // On créer d'abord une liste pour gérer nos Edges:
        HalfEdge[] halfEdges = new HalfEdge[4];
        halfEdges[0] = newHalfEdge;
        halfEdges[1] = halfEdges[0].nextEdge;
        halfEdges[2] = new HalfEdge(hEdges.Count, halfEdges[1].nextEdge.sourceVertex, halfEdges[1], null, null, face);
        halfEdges[3] = new HalfEdge(hEdges.Count+1, facePoint, halfEdges[2], halfEdges[0], null, face);

        halfEdges[0].prevEdge = halfEdges[3];
        halfEdges[1].nextEdge = halfEdges[2];
        halfEdges[2].nextEdge = halfEdges[3];

        face.edge = newHalfEdge;

        hEdges.Add(halfEdges[2]);
        hEdges.Add(halfEdges[3]);
    }

    #endregion

    #region Catmull Clark

    public static void CatmullClarkSubdivision(List<HalfEdge> halfEdges, List<Face> faces, List<Vertex> vertices)
    {
        // On crée les listes des nouveaux points (facePoints / edgePoints)
        List<Vertex> facePoints = new List<Vertex>();
        List<Vertex> edgePoints = new List<Vertex>();

        // On doit d'abord s'occuper des vertexPoints, avant de pouvoir modifier la liste des vertices
        for(int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = VertexPoints(vertices[i], halfEdges);
        }

        // On s'occupe ensuite des facePoints
        foreach(var face in faces)
        {
            Vertex facePoint = FacePoints(face);
            facePoints.Add(facePoint);
            vertices.Add(facePoint);
        }

        // De même pour les edgePoints
        foreach(var halfEdge in halfEdges)
        {
            Vertex edgePoint = EdgePoints(halfEdge);
            edgePoints.Add(edgePoint);
            vertices.Add(edgePoint);
        }

        //Maintenant qu'on a tous nos points, pour chaque face on doit :
        //  - Créer des nouvelles faces avec les facePoints
        //  - Couper nos edges en 2 (l'ancienne edge sera connectée de la vertice actuelle au edgePoint et la nouvelle edge du edgePoint à la prochaine vertice)
        //  - Connecter les facePoints aux edgePoints

        for(int i = 0; i < faces.Count; i++)
        {
            // On prend les 4 edges de la face
            HalfEdge halfEdge1 = faces[i].edge;
            HalfEdge halfEdge2 = faces[i].edge.nextEdge;
            HalfEdge halfEdge3 = faces[i].edge.nextEdge.nextEdge;
            HalfEdge halfEdge4 = faces[i].edge.nextEdge.nextEdge.nextEdge;

            // Créations des faces
            Face face1 = faces[i];
            Face face2 = new Face(faces.Count + 1);
            Face face3 = new Face(faces.Count + 2);
            Face face4 = new Face(faces.Count + 3);

            faces.Add(face2);
            faces.Add(face3);
            faces.Add(face4);

            // Split des edges
            HalfEdge newHalfEdge1 = SplitEdge(halfEdge1, edgePoints[halfEdge1.index]);
            HalfEdge newHalfEdge2 = SplitEdge(halfEdge2, edgePoints[halfEdge2.index]);
            HalfEdge newHalfEdge3 = SplitEdge(halfEdge3, edgePoints[halfEdge3.index]);
            HalfEdge newHalfEdge4 = SplitEdge(halfEdge4, edgePoints[halfEdge4.index]);

            newHalfEdge1.index = halfEdges.Count;
            newHalfEdge2.index = halfEdges.Count + 1;
            newHalfEdge3.index = halfEdges.Count + 2;
            newHalfEdge4.index = halfEdges.Count + 3;


            halfEdges.Add(newHalfEdge1);
            halfEdges.Add(newHalfEdge2);
            halfEdges.Add(newHalfEdge3);
            halfEdges.Add(newHalfEdge4);

            halfEdge1.prevEdge = newHalfEdge4;
            halfEdge2.prevEdge = newHalfEdge1;
            halfEdge3.prevEdge = newHalfEdge2;
            halfEdge4.prevEdge = newHalfEdge3;

            // On met à jour la correspondance entre les edges et les faces
            newHalfEdge1.face = face2;
            newHalfEdge2.face = face3;
            newHalfEdge3.face = face4;
            newHalfEdge4.face = face1;

            halfEdge2.face = face2;
            halfEdge3.face = face3;
            halfEdge4.face = face4;

            // On connecte enfin le facePoint aux 4 autres edgePoints
            Vertex currentFacePoint = facePoints[faces[i].index];
            ConnectEdgePointToFacePoint(currentFacePoint, face1, newHalfEdge4, halfEdges);
            ConnectEdgePointToFacePoint(currentFacePoint, face2, newHalfEdge1, halfEdges);
            ConnectEdgePointToFacePoint(currentFacePoint, face3, newHalfEdge2, halfEdges);
            ConnectEdgePointToFacePoint(currentFacePoint, face4, newHalfEdge3, halfEdges);
        }
    }
    
    public static Mesh Catmull_Clark(Mesh mesh, int subdivisionLevel = 1)
    {
        Mesh newMesh = mesh;
        for(int i = 0; i < subdivisionLevel; i++)
        {
            Tuple<List<HalfEdge>, List<Face>, List<Vertex>> halfEdges = VertexFaceToHalfEdges(newMesh);
            CatmullClarkSubdivision(halfEdges.Item1, halfEdges.Item2, halfEdges.Item3);
            newMesh = HalfHedgesToVertexFace(halfEdges.Item1);
        }
        return newMesh;
    }
    #endregion
}
