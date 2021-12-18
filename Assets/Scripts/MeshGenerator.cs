using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    delegate Vector3 ComputeVector3FromKxKz(float kX, float kZ);
    private MeshFilter m_Mf;
    private Mesh mMesh;

    private void Awake()
    {
        m_Mf = GetComponent<MeshFilter>();

        //Triangle :
        //m_Mf.sharedMesh = CreateTriangle();

        // Quads simples / avancés:
        //m_Mf.sharedMesh = CreateQuad(new Vector3(4,0,2));
        //m_Mf.sharedMesh = WrapNormalizedQuads(20, 10, (kX, Kz) => new Vector3((kX - 0.5f) * 4, 0, (Kz-.5f) * 2));

        // Strip :
        //m_Mf.sharedMesh = CreateStrip(new Vector3(4, 0, 2), 200);

        // Plane simples / avancés : 
        //m_Mf.sharedMesh = CreatePlane(new Vector3(4, 0, 2), 20, 10);
        //m_Mf.sharedMesh = WrapNormalizedPlane(20, 10, (kX,kZ) => new Vector3( (kX-.5f)*8, 0, (kZ-.5f)*4 ));

        // Autres formes :
        /*
    m_Mf.sharedMesh = WrapNormalizedPlane(20, 10, (kX, kZ) =>
    {
        float theta = kX * 2 * Mathf.PI;
        float z = 4 * kZ;
        float rho = 2;
        return new Vector3(rho * Mathf.Cos(theta), z, rho * Mathf.Sin(theta));
    })
    ;

    m_Mf.sharedMesh = WrapNormalizedPlane(20, 10, (kX, kZ) =>
    {
        float theta = kX * 2 * Mathf.PI;
        float phi = (1-kZ) * Mathf.PI;
        float rho = 2;
        return new Vector3(rho * Mathf.Cos(theta) * Mathf.Sin(phi), rho*Mathf.Cos(phi), rho * Mathf.Sin(theta) * Mathf.Sin(phi));
    })
    ;
        */

        // Polygone Regulier
        mMesh = CreateRegularPolygonXZQuads(5f, 9);
        m_Mf.sharedMesh = mMesh;
        //List<HalfEdge> cv = Toolbox.VertexFaceToHalfEdges(mMesh.vertices, mMesh.GetIndices(0));


        gameObject.AddComponent<MeshCollider>();
    }

    public Mesh CreateTriangle()
    {
        Mesh newMesh = new Mesh();
        newMesh.name = "triangle";

        Vector3[] vertices = new Vector3[3];
        int[] triangles = new int[3];

        vertices[0] = Vector3.right;
        vertices[1] = Vector3.up;
        vertices[2] = Vector3.forward;
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        newMesh.RecalculateBounds();
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

    public Mesh CreateStrip(Vector3 size, int nSegments)
    {
        Mesh newMesh = new Mesh();
        newMesh.name = "strip";

        Vector3 halfSize = size * .5f;

        Vector3[] vertices = new Vector3[(nSegments + 1) * 2];
        int[] triangles = new int[nSegments * 2 * 3];

        // VERTICES
        for (int i = 0; i < nSegments + 1; i++)
        {
            float k = (float)i / nSegments;
            float y = .125f * Mathf.Sin(k * Mathf.PI * 2 * 3);

            vertices[i] = new Vector3(Mathf.Lerp(-halfSize.x, halfSize.x, k), y, -halfSize.z);
            vertices[i + nSegments + 1] = new Vector3(Mathf.Lerp(-halfSize.x, halfSize.x, k), y, halfSize.z);
        }

        //TRIANGLES
        int index = 0;
        for (int i = 0; i < nSegments; i++)
        {
            triangles[index++] = i;
            triangles[index++] = i + nSegments + 1;
            triangles[index++] = i + nSegments + 1 + 1;

            triangles[index++] = i;
            triangles[index++] = i + nSegments + 1 + 1;
            triangles[index++] = i + 1;
        }

        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();
        return newMesh;
    }

    public Mesh CreatePlane(Vector3 size, int nSegmentsX, int nSegmentsZ)
    {
        Mesh newMesh = new Mesh();
        newMesh.name = "plane";

        Vector3 halfSize = size * .5f;


        Vector3[] vertices = new Vector3[(nSegmentsX+1) * (nSegmentsZ +1)];
        int[] triangles = new int[nSegmentsX * nSegmentsZ * 3 * 2];

        // VERTICES
        for (int i = 0; i < nSegmentsZ + 1; i++)
        {

            //float offsetZ = (float) i / nSegmentsZ;

            for (int j = 0; j < nSegmentsX + 1; j++)
            {

                //float offsetX = (float) j / nSegmentsX;

                vertices[j + (i * (nSegmentsX+1))] = new Vector3(
                    (float)-size.x / 2 + (float)(size.x * j) / nSegmentsX
                    , 0, 
                    (float)-size.z / 2 + (float)(size.z * i) / nSegmentsZ
                    );

                //vertices[j + (i*nSegmentsX)] = new Vector3(Mathf.Lerp(-halfSize.x, halfSize.x, offsetX), 0, Mathf.Lerp(-halfSize.z, halfSize.z, offsetZ));

                //float y = .125f * Mathf.Sin(k * Mathf.PI * 2 * 3);

                //vertices[i + nSegmentsX + 1] = new Vector3(Mathf.Lerp(-halfSize.x, halfSize.x, offsetX), 0, halfSize.z);
            }
        }


        for(int i = 0; i < vertices.Length; i++)
        {
            //print(" : %d", i, vertices[i]);
            Debug.Log(i + " : " + vertices[i]);
        }

        
        //TRIANGLES
        int index = 0;
        for (int i = 0; i < nSegmentsX; i++)
        {
            for (int j = 0; j < nSegmentsZ; j++)
            {
            
                triangles[index++] = i + j * (nSegmentsZ +1);
                triangles[index++] = i + 1 + j * (nSegmentsZ +1);
                triangles[index++] = i + nSegmentsX + 1 + j * (nSegmentsZ+1);

                triangles[index++] = i + 1 + j * (nSegmentsZ+1);
                triangles[index++] = i + nSegmentsX + 1 + 1 + j * (nSegmentsZ+1);
                triangles[index++] = i + nSegmentsX + 1 + j * (nSegmentsZ+1);
            }
        }
        

        newMesh.vertices = vertices;
        
        newMesh.triangles = triangles;
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();
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
        for(int i = 0; i < nSegmentsX; i++)
        {
            for(int j = 0; j < nSegmentsZ; j++)
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

    public Mesh CreateRegularPolygonXZQuads(float r, int nQuads)
    {
        Mesh newMesh = new Mesh();
        newMesh.name = "RegularPolygonQuads";
        Vector3[] vertices = new Vector3[nQuads * 2 + 1];
        int[] quads = new int[nQuads * 4];

        //Centre
        vertices[0] = Vector3.zero;

        //Vertices
        for (int i = 0; i < nQuads; i++)
        {
            float x = r * Mathf.Sin((2 * Mathf.PI * i) / nQuads);
            float z = r * Mathf.Cos((2 * Mathf.PI * i) / nQuads);

            Vector3 centerVertice = new Vector3(x, 0, z);
            
            x = r * Mathf.Sin((2 * Mathf.PI * (i + 1)) / nQuads);
            z = r * Mathf.Cos((2 * Mathf.PI * (i + 1)) / nQuads);
            
            Vector3 nextVertice = new Vector3(x, 0, z);

            //On calcule le point central
            Vector3 midNextVertice = Vector3.Lerp(centerVertice, nextVertice, .5f);

            //On les ajoute à la liste
            vertices[i * 2 + 1] = centerVertice;
            vertices[i * 2 + 2] = midNextVertice;
        }

        //Quads
        int index = 0;
        for (int i = 0; i < nQuads; i++)
        {
            quads[index++] = 0;
            quads[index++] = (i == 0) ? nQuads * 2 : 2 * i;
            quads[index++] = i * 2 + 1;
            quads[index++] = i * 2 + 2;

        }

        newMesh.vertices = vertices;
        newMesh.SetIndices(quads, MeshTopology.Quads, 0);
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();

        return newMesh;
    }

}