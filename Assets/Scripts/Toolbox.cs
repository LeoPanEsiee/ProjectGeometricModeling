using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toolbox : MonoBehaviour
{

    delegate Vector3 ComputeVector3FromKxKz(float kX, float kZ);

    [SerializeField]
    MeshFilter cube_meshFilter;

    [SerializeField]
    bool activate = false;



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
        return GetHalfEdges(vertice, halfEdges).Count;
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
                List<Face> faces = GetFaces(halfEdge.sourceVertex, halfEdges);
                foreach(var face in faces)
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
            quads[index++] = triangles[i];
        }

        return quads;
    }

    public static Tuple<List<HalfEdge>, List<Face>, List<Vertex>> VertexFaceToHalfEdges(Mesh mesh)
    {
        List<HalfEdge> halfEdges = new List<HalfEdge>();
        List<Face> faces = new List<Face>();
        List<Vertex> vertices = new List<Vertex>();

        //On gère la liste des vertices
        for(int i = 0; i < mesh.vertices.Length; i++)
        {
            vertices.Add(new Vertex(i, mesh.vertices[i]));
        }

        int[] quads = TrianglesToQuads(mesh);

        int index = 0;
        for (int i = 0; i < quads.Length / 4 ; i++)
        {
            // On part du principe qu'une face est égale à un quad, qui demande de générer 4 halfEdges
            
            Vertex v1 = vertices[quads[index]];
            Vertex v2 = vertices[quads[index + 1]];
            Vertex v3 = vertices[quads[index + 2]];
            Vertex v4 = vertices[quads[index + 3]];
            
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


            //Enfin, on ajoute tout à la liste des vertices
            halfEdges.Add(e1);
            halfEdges.Add(e2);
            halfEdges.Add(e3);
            halfEdges.Add(e4);
            index += 4;
        }
        SetTwinEdges(halfEdges);

        var list = new Tuple<List<HalfEdge>, List<Face>, List<Vertex>>(halfEdges, faces, vertices);

        return list;
    }

    public static Mesh HalfEdgesToVertexFace(List<HalfEdge> halfEdges, List<Face> faces, List<Vertex> vertices)
    {
        Mesh mesh = new Mesh();
        List<Vector3> tempVertices = vertices.ConvertAll(vertice => vertice.vertex);
        List<int> quads = new List<int>();

        foreach(var face in faces)
        {
            quads.Add(face.edge.sourceVertex.index);
            quads.Add(face.edge.nextEdge.sourceVertex.index);
            quads.Add(face.edge.nextEdge.nextEdge.sourceVertex.index);
            quads.Add(face.edge.nextEdge.nextEdge.nextEdge.sourceVertex.index);
        }

        mesh.vertices = tempVertices.ToArray();
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

        return new Vertex(res / 4);

    }

    public static Vertex EdgePoints(HalfEdge halfEdge)
    {
        Vertex edgePoint = halfEdge.sourceVertex;
        edgePoint += halfEdge.nextEdge.sourceVertex;

        if (halfEdge.twinEdge == null)
        {
            return new Vertex(edgePoint / 2);
        }
        else
        {
            edgePoint += FacePoints(halfEdge.face);
            edgePoint += FacePoints(halfEdge.twinEdge.face);

            return new Vertex(edgePoint / 4);
        }
    }

    public static Vertex VertexPoints(Vertex vertice, List<HalfEdge> halfEdges)
    {
        //On crée une liste des HalEdges adjacentes à notre vertice
        List<HalfEdge> listHalfEdgesFromVertex = GetHalfEdges(vertice, halfEdges);

        // On crée une liste des faces adjacentes à notre vertice
        List<Face> listFaces = GetFaces(vertice,halfEdges);

        int n = Valence(vertice, halfEdges);


        // A voir si 3 ou po
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
                //R += (listHalfEdgesFromVertex[i].sourceVertex + listHalfEdgesFromVertex[i].nextEdge.sourceVertex) / 2;
                R += (vertice + listHalfEdgesFromVertex[i].nextEdge.sourceVertex) / 2;
            }

            R /= listHalfEdgesFromVertex.Count;

            return new Vertex(vertice.index, (Q + 2 * R + (n - 3) * vertice) / n);
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
            facePoint.index = vertices.Count;
            facePoints.Add(facePoint);
            vertices.Add(facePoint);
        }

        // De même pour les edgePoints
        // Voir si on doit en sélectionner ou tous les traiter
        foreach(var halfEdge in halfEdges)
        {
            Vertex edgePoint = EdgePoints(halfEdge);
            edgePoint.index = vertices.Count;
            edgePoints.Add(edgePoint);
            vertices.Add(edgePoint);
        }

        //Maintenant qu'on a tous nos points, pour chaque face on doit :
        //  - Créer des nouvelles faces avec les facePoints
        //  - Couper nos edges en 2 (l'ancienne edge sera connectée de la vertice actuelle au edgePoint et la nouvelle edge du edgePoint à la prochaine vertice)
        //  - Connecter les facePoints aux edgePoints

        int facesSize = faces.Count;
        for (int i = 0; i < facesSize; i++)
        {

            // On prend les 4 edges de la face
            HalfEdge halfEdge1 = faces[i].edge;
            HalfEdge halfEdge2 = faces[i].edge.nextEdge;
            HalfEdge halfEdge3 = faces[i].edge.nextEdge.nextEdge;
            HalfEdge halfEdge4 = faces[i].edge.nextEdge.nextEdge.nextEdge;

            // Créations des faces
            Face face1 = faces[i];
            Face face2 = new Face(faces.Count);
            Face face3 = new Face(faces.Count + 1);
            Face face4 = new Face(faces.Count + 2);

            faces.Add(face2);
            faces.Add(face3);
            faces.Add(face4);

            // Split des edges
            HalfEdge newHalfEdge1 = SplitEdge(halfEdge1, edgePoints[halfEdge1.index]);
            HalfEdge newHalfEdge2 = SplitEdge(halfEdge2, edgePoints[halfEdge2.index]);
            HalfEdge newHalfEdge3 = SplitEdge(halfEdge3, edgePoints[halfEdge3.index]);
            HalfEdge newHalfEdge4 = SplitEdge(halfEdge4, edgePoints[halfEdge4.index]);

            /*
            newHalfEdge1.index = halfEdges.Count;
            newHalfEdge2.index = halfEdges.Count + 1;
            newHalfEdge3.index = halfEdges.Count + 2;
            newHalfEdge4.index = halfEdges.Count + 3;
            */


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


        SetTwinEdges(halfEdges);
    }
    
    public static Mesh Catmull_Clark(Mesh mesh, int subdivisionLevel = 1)
    {
        Mesh newMesh = mesh;
        for(int i = 0; i < subdivisionLevel; i++)
        {

            Tuple<List<HalfEdge>, List<Face>, List<Vertex>> halfEdges = VertexFaceToHalfEdges(newMesh);
            CatmullClarkSubdivision(halfEdges.Item1, halfEdges.Item2, halfEdges.Item3);
            newMesh = HalfEdgesToVertexFace(halfEdges.Item1, halfEdges.Item2, halfEdges.Item3);

        }

        return newMesh;
    }
    #endregion



    void Update()
    {
        if (activate)
        {

            activate = false;
            cube_meshFilter.sharedMesh = Catmull_Clark(cube_meshFilter.sharedMesh, 1);

            Debug.Log(MeshDisplayInfo.ExportMeshCSV(cube_meshFilter.sharedMesh));
            //cube_meshFilter.sharedMesh = CreateQuad(new Vector3(4,0,2));
            //cube_meshFilter.sharedMesh = WrapNormalizedPlane(20, 10, (kX,kZ) => new Vector3( (kX-.5f)*8, 0, (kZ-.5f)*4 ));

        }
    }

    private Mesh WrapNormalizedPlaneQuads(int nSegmentsX, int nSegmentsZ, ComputeVector3FromKxKz computePosition)
    {
        Mesh newMesh = new Mesh();
        newMesh.name = "wrappedNormalizedPlaneQuads";


        Vector3[] vertices = new Vector3[(nSegmentsX + 1) * (nSegmentsZ + 1)];
        int[] quads = new int[nSegmentsX * nSegmentsZ * 4];

        // VERTICES
        int index = 0;
        for (int i = 0; i < nSegmentsX + 1; i++)
        {
            float kX = (float)i / nSegmentsX;

            for (int j = 0; j < nSegmentsZ + 1; j++)
            {
                float kZ = (float)j / nSegmentsZ;
                vertices[index++] = computePosition(kX, kZ);
            }
        }

        //Debug :
        //for (int i = 0; i < vertices.Length; i++)
        //{
        //    Debug.Log(i + " : " + vertices[i]);
        //}


        //TRIANGLES
        index = 0;
        for (int i = 0; i < nSegmentsX; i++)
        {
            for (int j = 0; j < nSegmentsZ; j++)
            {
                quads[index++] = i * (nSegmentsZ + 1) + j;
                quads[index++] = i * (nSegmentsZ + 1) + j + 1;
                quads[index++] = (i + 1) * (nSegmentsZ + 1) + j + 1;
                quads[index++] = (i + 1) * (nSegmentsZ + 1) + j;


            }
        }
        for (int j = 0; j < nSegmentsZ; j++)
        {
            for (int i = 0; i < nSegmentsX; i++)
            {
                quads[index++] = i + j * (nSegmentsX + 1);
                quads[index++] = i + 1 + j * (nSegmentsX + 1);
                quads[index++] = i + nSegmentsX + 1 + j * (nSegmentsX + 1);
            }
        }


        newMesh.vertices = vertices;

        newMesh.triangles = quads;
        newMesh.SetIndices(quads, MeshTopology.Quads, 0);
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();
        return newMesh;
    }

    public Mesh CreateQuad(Vector3 size)
    {
        Mesh newMesh = new Mesh();
        newMesh.name = "quad";

        Vector3 halfSize = size * .5f;

        Vector3[] vertices = new Vector3[4];
        int[] triangles = new int[2 * 3];

        vertices[0] = new Vector3(-halfSize.x, 0, -halfSize.z);
        vertices[1] = new Vector3(-halfSize.x, 0, halfSize.z);
        vertices[2] = new Vector3(halfSize.x, 0, halfSize.z);
        vertices[3] = new Vector3(halfSize.x, 0, -halfSize.z);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        newMesh.RecalculateBounds();
        return newMesh;
    }

    private Mesh WrapNormalizedPlane(int nSegmentsX, int nSegmentsZ, ComputeVector3FromKxKz computePosition)
    {
        Mesh newMesh = new Mesh();
        newMesh.name = "wrappedNormalizedPlane";


        Vector3[] vertices = new Vector3[(nSegmentsX + 1) * (nSegmentsZ + 1)];
        int[] triangles = new int[nSegmentsX * nSegmentsZ * 3 * 2];

        // VERTICES
        int index = 0;
        for (int i = 0; i < nSegmentsX + 1; i++)
        {
            float kX = (float)i / nSegmentsX;
            for (int j = 0; j < nSegmentsZ + 1; j++)
            {
                float kZ = (float)j / nSegmentsZ;
                vertices[index++] = computePosition(kX, kZ);
            }
        }

        //Debug :
        //for (int i = 0; i < vertices.Length; i++)
        //{
        //    Debug.Log(i + " : " + vertices[i]);
        //}


        //TRIANGLES
        index = 0;
        for (int j = 0; j < nSegmentsZ; j++)
        {
            for (int i = 0; i < nSegmentsX; i++)
            {
                triangles[index++] = i + j * (nSegmentsX + 1);
                triangles[index++] = i + 1 + j * (nSegmentsX + 1);
                triangles[index++] = i + nSegmentsX + 1 + j * (nSegmentsX + 1);

                triangles[index++] = i + 1 + j * (nSegmentsX + 1);
                triangles[index++] = i + nSegmentsX + 1 + 1 + j * (nSegmentsX + 1);
                triangles[index++] = i + nSegmentsX + 1 + j * (nSegmentsX + 1);
            }
        }

        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();
        return newMesh;
    }
}
