using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTrackingScript : MonoBehaviour
{
    private float step;

    private LineRenderer line;
    private Transform p0;
    private Transform p1;
    private Transform p2;

    

    // Start is called before the first frame update
    void Start()
    {
        
        line = GetComponent<LineRenderer>();
        
    }

    // Update is called once per frame
    void Update()
    {
        step = 5.0f * Time.deltaTime;

       
        
        //isIndexFingerPinching = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);

        //if 
        //{
        //    line.enabled = true;

            

        //    // New lines added below this point
        //    end.SetActive(true);
        //    end.transform.position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        //    end.transform.rotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
        //    p0 = transform;
        //    p2 = end.transform;
        //    p1 = sceneCamera.transform;
        //    p1.position += sceneCamera.transform.forward * 0.8f;

        //    DrawCurve(p0.position, p1.position, p2.position);
        //    // New lines added above this point
        //}
        //else
        //{
        //    end.SetActive(false);
        //    line.enabled = false;
        //}
        


    }


    void DrawCurve(Vector3 point_0, Vector3 point_1, Vector3 point_2)
    {
        line.positionCount = 200;
        Vector3 B = new Vector3(0, 0, 0);
        float t = 0f;

        for (int i = 0; i < line.positionCount; i++)
        {
            t += 0.005f;
            B = (1 - t) * (1 - t) * point_0 + 2 * (1 - t) * t * point_1 + t * t * point_2;
            line.SetPosition(i, B);
        }
    }
}
