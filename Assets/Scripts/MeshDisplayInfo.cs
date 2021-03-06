using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(MeshFilter))]
public class MeshDisplayInfo : MonoBehaviour
{

    MeshFilter m_Mf;
    [Header("edges")]
    [SerializeField] bool m_DisplayEdges;
    [SerializeField] int m_NMaxEdges;

    [Header("normals")]
    [SerializeField] bool m_DisplayNormals;
    [SerializeField] int m_NMaxNormals;

    [SerializeField] float m_NormalScaleFactor;

    [Header("vertices")]
    [SerializeField] bool m_DisplayVertices;
    [SerializeField] int m_NMaxVertices;



    // Start is called before the first frame update
    private void Awake()
    {
        m_Mf = GetComponent<MeshFilter>();
    }

    private void OnDrawGizmos()
    {
        if (! (m_Mf && !m_Mf.sharedMesh)) return;

        Vector3[] vertices = m_Mf.sharedMesh.vertices;


        //edges
        if (m_DisplayEdges)
        {
            int[] quads = m_Mf.sharedMesh.GetIndices(0);

            Gizmos.color = Color.black;
            int index = 0;
            for (int i = 0; i < Mathf.Min(quads.Length / 4, m_NMaxEdges); i++)
            {
                int index1 = quads[index++];
                int index2 = quads[index++];
                int index3 = quads[index++];
                int index4 = quads[index++];

                Vector3 pt1 = transform.TransformPoint(vertices[index1]);
                Vector3 pt2 = transform.TransformPoint(vertices[index2]);
                Vector3 pt3 = transform.TransformPoint(vertices[index3]);
                Vector3 pt4 = transform.TransformPoint(vertices[index4]);

                Gizmos.DrawLine(pt1, pt2);
                Gizmos.DrawLine(pt3, pt2);
                Gizmos.DrawLine(pt1, pt4);
                Gizmos.DrawLine(pt4, pt3);
            }
        }

        //normals
        if (m_DisplayNormals)
        {
            Vector3[] normals = m_Mf.sharedMesh.normals;

            Gizmos.color = Color.black;
            for (int i = 0; i < Mathf.Min(normals.Length, m_NMaxNormals); i++)
            {
                Vector3 pos = transform.TransformPoint(vertices[i]);
                Vector3 normal = transform.TransformDirection(normals[i]);

                Gizmos.DrawLine(pos, pos + normal* m_NormalScaleFactor);
            }
        }

        //vertices
        if (m_DisplayVertices)
        {
            Vector3[] normals = m_Mf.sharedMesh.normals;

            Gizmos.color = Color.red;
            GUIStyle myStyle = new GUIStyle();
            myStyle.fontSize = 16;
            myStyle.normal.textColor = Color.red;


            for (int i = 0; i < Mathf.Min(vertices.Length, m_NMaxVertices); i++)
            {
                Vector3 pos = transform.TransformPoint(vertices[i]);

                Gizmos.DrawSphere(pos, .01f);
                Handles.Label(pos, i.ToString(), myStyle);
                
            }
        }



    }

}
