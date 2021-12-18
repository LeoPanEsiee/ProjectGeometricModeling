using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereManager : MonoBehaviour
{
    
    //Go
    [SerializeField]
    GameObject pt1;
    [SerializeField]
    GameObject pt2;

    [SerializeField]
    GameObject goSphere;


    //Math
    Sphere sphere1;
    Segment s1;

    GameObject newSphere;

    //bool touche = false;

    // Start is called before the first frame update
    void Start()
    {
        Debug.DrawLine(pt1.transform.position, pt2.transform.position, Color.red, 100);

        sphere1 = new Sphere(goSphere.transform.position, goSphere.transform.localScale.x);
        s1 = new Segment(pt1.transform.position, pt2.transform.position);
    }

    private void Update()
    {
        Vector3 interPt1, interPt2;


        sphere1.center = goSphere.transform.position;
        sphere1.radius = goSphere.transform.localScale.x;



        if (InterSegmentSphere(s1, sphere1, out interPt1, out interPt2))
        {
            Debug.Log("CAAA FONCTIONNE");
            createIntersectionSphere(interPt1);
            createIntersectionSphere(interPt2);

        }
    }


    private void createIntersectionSphere(Vector3 position)
    {
        newSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        newSphere.transform.localScale = new Vector3(1, 1, 1);
        newSphere.transform.position = position;
        newSphere.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        Destroy(newSphere, .05f);
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
        else
        {
            float x1 = (-b + Mathf.Sqrt(delta))/ (2 * a);
            float x2 = (-b - Mathf.Sqrt(delta)) / (2 * a);

            //Debug.Log(x1 + " ||| " + x2);
            if (x1 < 0 || x1 > 1 || x2 < 0 || x2 > 1)
                return false;

            interPt1 = seg.pt1 + x1 * AB;
            interPt2 = seg.pt1 + x2 * AB;

            Debug.Log(interPt1);
            Debug.Log(interPt2);

            return true;

        }

    }
    
}
