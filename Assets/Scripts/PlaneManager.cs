using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class PlaneManager : MonoBehaviour
{

    //Go
    [SerializeField]
    GameObject pt1;
    [SerializeField]
    GameObject pt2;

    [SerializeField]
    GameObject goPlane;

    //Math
    Plane plan1;
    Segment s1;

    //bool touche = false;



    // Start is called before the first frame update
    void Start()
    {

        Debug.DrawLine(pt1.transform.position, pt2.transform.position, Color.red, 100);

        plan1 = new Plane(goPlane.transform.forward, Vector3.Dot(goPlane.transform.position, goPlane.transform.forward));
        s1 = new Segment(pt1.transform.position, pt2.transform.position);

    }
    

    // Update is called once per frame
    void Update()
    {

        goPlane.transform.Rotate(20f * Time.deltaTime,0,0, Space.Self);

        plan1.normal = goPlane.transform.forward;
        plan1.d = Vector3.Dot(goPlane.transform.position, plan1.normal);

        //Debug.Log("Plan 1 : " + plan1.normal + " " + plan1.d);

        GameObject newSphere;

        Vector3 interPt, interNormal;
        if(InterSegmentPlane(s1, plan1, out interPt, out interNormal))
        {
            //Debug.Log("CAAA FONCTIONNE");
            //touche = true;
            newSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            newSphere.transform.position = interPt;
            Destroy(newSphere,.1f);
        }
        /*
        else
        {
            if (touche)
            {

                touche = false;
                Debug.Log("NE TOUCHE PAS");
            }
        }
        */

    }

    public bool InterSegmentPlane(Segment seg, Plane plane, out Vector3 interPt, out Vector3 interNormal)
    {
        interPt = new Vector3(0,0,0);
        interNormal = new Vector3(0, 0, 0);
        //Vector3 O = new Vector3(0, 0, 0);
        Vector3 AB = seg.pt2 - seg.pt1;
        float dotABn = Vector3.Dot(AB, plane.normal);

        if (Mathf.Approximately(dotABn, 0)) { 
            return false; 
        }

        Vector3 OA = seg.pt1;
        float t = (plane.d - Vector3.Dot(OA, plane.normal)) / dotABn;

        if (t < 0 || t > 1) { 
            return false; 
        }

        interPt = seg.pt1 + t * AB;
        interNormal = (dotABn < 0) ? plane.normal : -plane.normal;
        return true;
    }


}
