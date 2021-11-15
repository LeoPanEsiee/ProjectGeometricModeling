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
}
