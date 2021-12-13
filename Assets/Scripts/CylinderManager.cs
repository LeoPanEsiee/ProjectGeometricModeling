using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CylinderManager : MonoBehaviour
{

    //Go
    [SerializeField]
    GameObject pt1;
    [SerializeField]
    GameObject pt2;

    [SerializeField]
    GameObject goCylinder;


    //Math
    Cylinder cylinder1;
    Segment s1;

    // Start is called before the first frame update
    void Start()
    {
        Debug.DrawLine(pt1.transform.position, pt2.transform.position, Color.red, 100);

        //cylinder1 = new Cylinder(goCylinder.transform.position, goCylinder.transform.localScale.x);
        s1 = new Segment(pt1.transform.position, pt2.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static bool InterSegCylInf(Segment segment, Cylinder cylinder, out Vector3 interPt1, out Vector3 interPt2, out Vector3 interNormal)
    {
        interPt1 = new Vector3();
        interPt2 = new Vector3();
        interNormal = new Vector3();

        Vector3 u = (cylinder.pt2 - cylinder.pt1) / Vector3.Magnitude(cylinder.pt2 - cylinder.pt1);
        Vector3 AB = segment.pt2 - segment.pt1;
        Vector3 PA = cylinder.pt1 - segment.pt2;
        Vector3 PQ = cylinder.pt2 - cylinder.pt1;

        float a = AB.magnitude * AB.magnitude - (Vector3.Dot(PQ, AB) * Vector3.Dot(PQ, AB)) / (PQ.magnitude * PQ.magnitude);
        float b = 2 * Vector3.Dot(PA, AB) - 2 * ((Vector3.Dot(AB, PQ) * Vector3.Dot(PA, PQ)) / (PQ.magnitude * PQ.magnitude));
        float c = Vector3.Dot(PA, PA) - ((Vector3.Dot(PA, PQ) * Vector3.Dot(PA, PQ)) / (PQ.magnitude * PQ.magnitude));

        float Delt = (b * b) - 4 * a * c;

        float racine1;
        float racine2;

        if (Delt < 0)
        {
            return false;
        }
        else
        {
            racine1 = (-b - Mathf.Sqrt(Delt)) / 2 * a;
            racine2 = (-b + Mathf.Sqrt(Delt)) / 2 * a;
        }

        float t1 = racine1;
        float t2 = racine2;
        interPt1 = segment.pt1 + t1 * AB;
        interPt2 = segment.pt1 + t2 * AB;

        return true;
    }


}
