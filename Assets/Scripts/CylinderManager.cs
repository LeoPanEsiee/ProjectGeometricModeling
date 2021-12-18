using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CylinderManager : MonoBehaviour
{

    //Go
    //Segment
    [SerializeField]
    GameObject pt1;
    [SerializeField]
    GameObject pt2;

    [SerializeField]
    GameObject goCylinder;



    float radius = .5f;

    //Math
    Cylinder cylinder1;
    Segment s1;


    static bool intersec1 = false;
    static bool intersec2 = false;


    // Start is called before the first frame update
    void Start()
    {
        Debug.DrawLine(pt1.transform.position, pt2.transform.position, Color.red, 100);

        //cylinder1 = new Cylinder(goCylinder.transform.position, goCylinder.transform.localScale.x);
        s1 = new Segment(pt1.transform.position, pt2.transform.position);
        cylinder1 = new Cylinder(goCylinder.transform.position - goCylinder.transform.up * 2,
                                 goCylinder.transform.position + goCylinder.transform.up * 2,
                                 radius);
    }

    // Update is called once per frame
    void Update()
    {
        goCylinder.transform.Rotate(20f * Time.deltaTime, 0, 0, Space.Self);

        cylinder1.pt1 = goCylinder.transform.position - goCylinder.transform.up *  2;
        cylinder1.pt2 = goCylinder.transform.position + goCylinder.transform.up *  2;

        Vector3 interPt1, interPt2 , interNormal;
        if (InterSegmentCylinder(s1, cylinder1, out interPt1, out interPt2, out interNormal))
        {
            if (intersec1)
            {
                createIntersectionSphere(interPt1);
            }

            if (intersec2)
            {
                createIntersectionSphere(interPt2);
            }

        }

    }
    private void createIntersectionSphere(Vector3 position)
    {
        GameObject newSphere;
        newSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        newSphere.transform.localScale = new Vector3(1, 1, 1);
        newSphere.transform.position = position;
        newSphere.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        Destroy(newSphere, .05f);
    }


    
    public static bool InterSegmentCylinder(Segment segment, Cylinder cylinder, out Vector3 interPt1, out Vector3 interPt2, out Vector3 interNormal)
    {
        interPt1 = new Vector3(0,0,0);
        interPt2 = new Vector3(0,0,0);
        interNormal = new Vector3(0,0,0);

        Vector3 u = (cylinder.pt2 - cylinder.pt1) / Vector3.Magnitude(cylinder.pt2 - cylinder.pt1);
        Vector3 AB = segment.pt2 - segment.pt1;
        Vector3 PA = segment.pt1 - cylinder.pt1;
        Vector3 PQ = cylinder.pt2 - cylinder.pt1;
        /*
        float a = AB.magnitude * AB.magnitude - (Vector3.Dot(PQ, AB) * Vector3.Dot(PQ, AB)) / (PQ.magnitude * PQ.magnitude);
        float b = 2 * Vector3.Dot(PA, AB) - 2 * ((Vector3.Dot(AB, PQ) * Vector3.Dot(PA, PQ)) / (PQ.magnitude * PQ.magnitude));
        float c = Vector3.Dot(PA, PA) - ((Vector3.Dot(PA, PQ) * Vector3.Dot(PA, PQ)) / (PQ.magnitude * PQ.magnitude));
        */

        float a = Vector3.Dot(AB, AB) - 2 * Vector3.Dot(AB, Vector3.Dot(AB, PQ) / PQ.magnitude * u) + Mathf.Pow(Vector3.Dot(AB, PQ) / PQ.magnitude, 2) * Vector3.Dot(u, u);
        float b = 2 * Vector3.Dot(AB, PA) - 4 * Vector3.Dot(AB, Vector3.Dot(PA, PQ) / PQ.magnitude * u) + 2 * Vector3.Dot(AB, PQ) * Vector3.Dot(PA, PQ) / Mathf.Pow(PQ.magnitude, 2) * Vector3.Dot(u, u);
        float c = Vector3.Dot(PA, PA) - 2 * Vector3.Dot(PA, Vector3.Dot(PA, PQ) / PQ.magnitude * u) + Mathf.Pow(Vector3.Dot(PA, PQ) / PQ.magnitude, 2) * Vector3.Dot(u, u) - Mathf.Pow(cylinder.radius, 2);

        float Delt = (b * b) - 4 * a * c;


        float racine1;
        float racine2;

        if (Delt < 0)
        {
            return false;
        }
        else
        {
            racine1 = (-b - Mathf.Sqrt(Delt)) / (2 * a);
            racine2 = (-b + Mathf.Sqrt(Delt)) / (2 * a);
        }

        intersec1 = false;
        intersec2 = false;
        if (racine1 >= 0 && racine1 <= 1)
        {
            interPt1 = segment.pt1 + racine1 * AB;
            intersec1 = true;
        }
        if (racine2 >= 0 && racine2 <= 1)
        {
            interPt2 = segment.pt1 + racine2 * AB;
            intersec2 = true;
        }
        if (intersec1 || intersec2)
        {
            return true;
        }

        return false;
    }


    /*
public static bool InterSegmentCylinder(Segment segment, Cylinder cylinder, out Vector3 interpt, out Vector3 interNormal)
{
    interpt = new Vector3();
    interNormal = new Vector3();

    Vector3 AB = segment.pt2 - segment.pt1;
    Vector3 PA = segment.pt1 - cylinder.pt1;
    Vector3 PQ = cylinder.pt2 - cylinder.pt1;
    Vector3 u = PQ / PQ.magnitude;

    float a = Vector3.Dot(AB, AB) - 2 * Vector3.Dot(AB, Vector3.Dot(AB, PQ) / PQ.magnitude * u) + Mathf.Pow(Vector3.Dot(AB, PQ) / PQ.magnitude, 2) * Vector3.Dot(u, u);
    float b = 2 * Vector3.Dot(AB, PA) - 4 * Vector3.Dot(AB, Vector3.Dot(PA, PQ) / PQ.magnitude * u) + 2 * Vector3.Dot(AB, PQ) * Vector3.Dot(PA, PQ) / Mathf.Pow(PQ.magnitude, 2) * Vector3.Dot(u, u);
    float c = Vector3.Dot(PA, PA) - 2 * Vector3.Dot(PA, Vector3.Dot(PA, PQ) / PQ.magnitude * u) + Mathf.Pow(Vector3.Dot(PA, PQ) / PQ.magnitude, 2) * Vector3.Dot(u, u) - Mathf.Pow(cylinder.radius, 2);

    float det = b * b - 4f * a * c;

    if (det < 0)
    {
        return false;
    }

    float x1 = (-b - Mathf.Sqrt(det)) / (2f * a);
    float x2 = (-b + Mathf.Sqrt(det)) / (2f * a);

    if (x1 >= 0 && x1 <= 1)
    {
        interpt = segment.pt1 + x1 * AB;
        Vector3 H = cylinder.pt1 + Vector3.Dot(interpt - cylinder.pt1, u) * u;
        interNormal = interpt - H;
        interNormal.Normalize();
        return true;
    }
    if (x2 >= 0 && x2 <= 1)
    {
        interpt = segment.pt1 + x2 * AB;
        Vector3 H = cylinder.pt1 + Vector3.Dot(interpt - cylinder.pt1, u) * u;
        interNormal = -(interpt - H);
        interNormal.Normalize();
        return true;
    }

    return false;
}

    */


}
