using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalManager : MonoBehaviour
{
    [SerializeField]
    GameObject pt1;
    [SerializeField]
    GameObject pt2;


    [SerializeField]
    GameObject goPlane;

    [SerializeField]
    GameObject goSphere;

    [SerializeField]
    GameObject goCylinder;



    [SerializeField]
    GameObject pointDistance;


    [SerializeField]
    GameObject goTextBox;

    [SerializeField]
    int form = 0;




    //Math
    Segment seg1;
    Plane plan1;
    Sphere sphere1;
    Cylinder cylinder1;





    GameObject newSphere;

    static bool sphere_intersec1 = false;
    static bool sphere_intersec2 = false;

    static bool cyl_intersec1 = false;
    static bool cyl_intersec2 = false;


    Text myText;




    void Start()
    {

        Debug.DrawLine(pt1.transform.position, pt2.transform.position, Color.red, 100);

        seg1 = new Segment(pt1.transform.position, pt2.transform.position);
        plan1 = new Plane(goPlane.transform.forward, Vector3.Dot(goPlane.transform.position, goPlane.transform.forward));
        sphere1 = new Sphere(goSphere.transform.position, goSphere.transform.localScale.x);

        cylinder1 = new Cylinder(goCylinder.transform.position - goCylinder.transform.up * 2,
                                 goCylinder.transform.position + goCylinder.transform.up * 2,
                                 .5f);
        myText = goTextBox.GetComponent<Text>();
    }

    void Update()
    {
        switch (form)
        {

            case 1:
                myText.text = "";
                goPlane.SetActive(true);
                goSphere.SetActive(false);
                goCylinder.SetActive(false);
                UpdatingPlane();
                Vector3 intersectionPlan, interNormal;
                if (InterSegmentPlane(seg1, plan1, out intersectionPlan, out interNormal))
                {
                    CreateIntersectionSphere(intersectionPlan, Color.red);
                }
                break;
            case 2:
                myText.text = "";
                goPlane.SetActive(false);
                goSphere.SetActive(true);
                goCylinder.SetActive(false);
                UpdateSphere();
                Vector3 intersectionSphere1, intersectionSphere2;
                if (InterSegmentSphere(seg1, sphere1, out intersectionSphere1, out intersectionSphere2))
                {
                    if (sphere_intersec1)
                    {
                        CreateIntersectionSphere(intersectionSphere1, Color.blue);
                    }

                    if (sphere_intersec2)
                    {
                        CreateIntersectionSphere(intersectionSphere2, Color.blue);
                    }
                }
                break;
            case 3:

                myText.text = "";
                goPlane.SetActive(false);
                goSphere.SetActive(false);
                goCylinder.SetActive(true);
                UpdateCylinder();
                Vector3 pointIntersecCylindre1, pointIntersecCylindre2;
                if (InterSegmentCylinder(seg1, cylinder1, out pointIntersecCylindre1, out pointIntersecCylindre2, out interNormal))
                {
                    if (cyl_intersec1)
                    {
                        CreateIntersectionSphere(pointIntersecCylindre1, Color.yellow);
                    }

                    if (cyl_intersec2)
                    {
                        CreateIntersectionSphere(pointIntersecCylindre2, Color.yellow);
                    }

                }
                break;

            case 4:
                goPlane.SetActive(true);
                goSphere.SetActive(false);
                goCylinder.SetActive(false);

                UpdateDistances();
                UpdatingPlane(); 
                break;

            default:
                myText.text = "";
                goPlane.SetActive(false);
                goSphere.SetActive(false);
                goCylinder.SetActive(false);
                break;
        }
    }

    private void CreateIntersectionSphere(Vector3 position, Color c)
    {
        newSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        newSphere.transform.localScale = new Vector3(1, 1, 1);
        newSphere.transform.position = position;
        newSphere.GetComponent<Renderer>().material.SetColor("_Color", c);
        Destroy(newSphere, .05f);
    }

    private void UpdatingPlane()
    {
        goPlane.transform.Rotate(20f * Time.deltaTime, 0, 0, Space.Self);
        plan1.normal = goPlane.transform.forward;
        plan1.d = Vector3.Dot(goPlane.transform.position, plan1.normal);
    }

    private void UpdateSphere()
    {
        sphere1.center = goSphere.transform.position;
        sphere1.radius = goSphere.transform.localScale.x;
    }

    private void UpdateCylinder()
    {
        goCylinder.transform.Rotate(20f * Time.deltaTime, 0, 0, Space.Self);

        cylinder1.pt1 = goCylinder.transform.position - goCylinder.transform.up * 2;
        cylinder1.pt2 = goCylinder.transform.position + goCylinder.transform.up * 2;
    }

    public bool InterSegmentPlane(Segment seg, Plane plane, out Vector3 interPt, out Vector3 interNormal)
    {
        interPt = new Vector3(0, 0, 0);
        interNormal = new Vector3(0, 0, 0);
        Vector3 AB = seg.pt2 - seg.pt1;
        float dotABn = Vector3.Dot(AB, plane.normal);

        if (Mathf.Approximately(dotABn, 0))
        {
            return false;
        }

        Vector3 OA = seg.pt1;
        float t = (plane.d - Vector3.Dot(OA, plane.normal)) / dotABn;

        if (t < 0 || t > 1)
        {
            return false;
        }

        interPt = seg.pt1 + t * AB;
        interNormal = (dotABn < 0) ? plane.normal : -plane.normal;
        return true;
    }

    public bool InterSegmentSphere(Segment seg, Sphere sphere, out Vector3 interPt1, out Vector3 interPt2)
    {
        interPt1 = new Vector3(0, 0, 0);
        interPt2 = new Vector3(0, 0, 0);
        Vector3 AB = seg.pt2 - seg.pt1;
        Vector3 T = seg.pt1 - sphere.center;

        float a = Vector3.Dot(AB, AB);
        float b = 2 * Vector3.Dot(AB, T);
        float c = Vector3.Dot(T, T) - Mathf.Sqrt(sphere.radius);

        float delta = b * b - 4 * a * c;

        if (delta < 0)
        {
            return false;
        }
        float x1 = (-b + Mathf.Sqrt(delta)) / (2 * a);
        float x2 = (-b - Mathf.Sqrt(delta)) / (2 * a);


        sphere_intersec1 = false;
        sphere_intersec2 = false;

        if (x1 >= 0 && x1 <= 1)
        {
            interPt1 = seg.pt1 + x1 * AB;
            sphere_intersec1 = true;
        }
        if (x2 >= 0 && x2 <= 1)
        {
            interPt2 = seg.pt1 + x2 * AB;
            sphere_intersec2 = true;
        }
        if (sphere_intersec1 || sphere_intersec2)
        {
            return true;
        }
        return false;


    }
    public static bool InterSegmentCylinder(Segment segment, Cylinder cylinder, out Vector3 interPt1, out Vector3 interPt2, out Vector3 interNormal)
    {
        interPt1 = new Vector3(0, 0, 0);
        interPt2 = new Vector3(0, 0, 0);
        interNormal = new Vector3(0, 0, 0);

        Vector3 u = (cylinder.pt2 - cylinder.pt1) / Vector3.Magnitude(cylinder.pt2 - cylinder.pt1);
        Vector3 AB = segment.pt2 - segment.pt1;
        Vector3 PA = segment.pt1 - cylinder.pt1;
        Vector3 PQ = cylinder.pt2 - cylinder.pt1;

        float a = Vector3.Dot(AB, AB) - 2 * Vector3.Dot(AB, Vector3.Dot(AB, PQ) / PQ.magnitude * u) + Mathf.Pow(Vector3.Dot(AB, PQ) / PQ.magnitude, 2) * Vector3.Dot(u, u);
        float b = 2 * Vector3.Dot(AB, PA) - 4 * Vector3.Dot(AB, Vector3.Dot(PA, PQ) / PQ.magnitude * u) + 2 * Vector3.Dot(AB, PQ) * Vector3.Dot(PA, PQ) / Mathf.Pow(PQ.magnitude, 2) * Vector3.Dot(u, u);
        float c = Vector3.Dot(PA, PA) - 2 * Vector3.Dot(PA, Vector3.Dot(PA, PQ) / PQ.magnitude * u) + Mathf.Pow(Vector3.Dot(PA, PQ) / PQ.magnitude, 2) * Vector3.Dot(u, u) - Mathf.Pow(cylinder.radius, 2);

        float Delt = (b * b) - 4 * a * c;



        if (Delt < 0)
        {
            return false;
        }

        float x1 = (-b - Mathf.Sqrt(Delt)) / (2 * a);
        float x2 = (-b + Mathf.Sqrt(Delt)) / (2 * a);

        cyl_intersec1 = false;
        cyl_intersec2 = false;
        if (x1 >= 0 && x1 <= 1)
        {
            interPt1 = segment.pt1 + x1 * AB;
            cyl_intersec1 = true;
        }
        if (x2 >= 0 && x2 <= 1)
        {
            interPt2 = segment.pt1 + x2 * AB;
            cyl_intersec2 = true;
        }
        if (cyl_intersec1 || cyl_intersec2)
        {
            return true;
        }

        return false;
    }

    private void UpdateDistances()
    {

        float distancePointPlane, distancePointSegment;
        Vector3 planePoint = goPlane.transform.position;
        DistancePointPlane(pointDistance.transform.position, planePoint, plan1, out distancePointPlane);
        DistancePointSegment(seg1, pointDistance.transform.position, out distancePointSegment);

        
        myText.text = "Distance point-plane : " + distancePointPlane.ToString("F2") + 
            "\nDistance point-segment : " + distancePointSegment.ToString("F2");

    }

    public static float DistancePointSegment(Segment segment, Vector3 point, out float distance)
    {
        Vector3 u = segment.pt2 - segment.pt1;
        Vector3 OP = point - segment.pt1;
        distance = Vector3.Cross(u, OP).magnitude / u.magnitude;

        Debug.DrawLine(u, OP, Color.blue, 100);
        return distance;
    }

    public static float DistancePointPlane(Vector3 point, Vector3 planePoint, Plane plane, out float distance)
    {
        Vector3 u = plane.normal;
        float d = u.x * planePoint.x + u.y * planePoint.y + u.z * planePoint.z;
        distance = Mathf.Abs(u.x * point.x + u.y * point.y + u.z * point.z + Mathf.Abs(d)) / u.magnitude;
        return distance;
    }
}
