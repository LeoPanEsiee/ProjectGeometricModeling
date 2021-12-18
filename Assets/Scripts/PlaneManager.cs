using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlaneManager : MonoBehaviour
{

    //Go
    [SerializeField]
    GameObject goSegment1;
    [SerializeField]
    GameObject goSegment2;
    [SerializeField]
    GameObject goPoint;
    [SerializeField]
    GameObject goPlane;
    [SerializeField]
    GameObject goTextBox;

    //Math
    Plane plane;
    Segment segment;
    Vector3 point;

    // Start is called before the first frame update
    void Start()
    {
        Debug.DrawLine(goSegment1.transform.position, goSegment2.transform.position, Color.red, 100);
        segment = new Segment(goSegment1.transform.position, goSegment2.transform.position);
        plane = new Plane(goPlane.transform.forward, Vector3.Dot(goPlane.transform.position, goPlane.transform.forward));
        point = goPoint.transform.position;
    }


    // Update is called once per frame
    void Update()
    {
        goPlane.transform.Rotate(20f * Time.deltaTime, 0, 0, Space.Self);
        plane.normal = goPlane.transform.forward;
        plane.d = Vector3.Dot(goPlane.transform.position, plane.normal);

        GameObject newSphere;

        Vector3 interPt, interNormal;
        if (InterSegmentPlane(segment, plane, out interPt, out interNormal))
        {
            newSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            newSphere.transform.position = interPt;
            Destroy(newSphere, .1f);
        }

        float distancePointPlane, distancePointSegment;
        Vector3 planePoint = goPlane.transform.position;
        DistancePointPlane(point, planePoint, plane, out distancePointPlane);
        DistancePointSegment(segment, point, out distancePointSegment);
        Text myText = goTextBox.GetComponent<Text>();
        myText.text = "Distance point-plane : " + distancePointPlane + "\nDistance point-segment : " + distancePointSegment;
    }

    public bool InterSegmentPlane(Segment seg, Plane plane, out Vector3 interPt, out Vector3 interNormal)
    {
        interPt = new Vector3(0, 0, 0);
        interNormal = new Vector3(0, 0, 0);
        //Vector3 O = new Vector3(0, 0, 0);
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

    public static float DistancePointSegment(Segment segment, Vector3 point, out float distance)
    {
        Vector3 u = segment.pt2 - segment.pt1;
        Vector3 OP = point - segment.pt1;
        distance = Vector3.Cross(u, OP).magnitude / u.magnitude;
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
