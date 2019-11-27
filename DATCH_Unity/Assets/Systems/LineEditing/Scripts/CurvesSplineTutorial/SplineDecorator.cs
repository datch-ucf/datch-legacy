using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineDecorator : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	

    public BezierSpline spline;

    public int frequency;

    public bool lookForward;

    public Transform[] items;

    public List<GameObject> sphereControlPoints = new List<GameObject>();

    public List<GameObject> lines = new List<GameObject>();

    public Vector3[] controlPoints;
    private void Awake()
    {
        if(frequency <= 0 || items == null || items.Length == 0)
        {
            return;
        }

        float stepSize = frequency * items.Length;

        if (spline.Loop || stepSize == 1)
        {
            stepSize = 1f / stepSize;
        }
        else
        {
            stepSize = 1f / (stepSize - 1);
        }
        for (int p = 0, f = 0; f < frequency; f++)
        {
            for (int i = 0; i < items.Length; i++, p++)
            {
                Transform item = Instantiate(items[1]) as Transform;
                lines.Add(item.gameObject);
                Vector3 position = spline.GetPoint(p * stepSize);
                item.transform.localPosition = position;
                if (lookForward)
                {
                    item.transform.LookAt(position + spline.GetDirection(p * stepSize));
                }
                item.transform.parent = transform;
            }
        }

        controlPoints = spline.GetAllControlPoints();
        transform.position = spline.gameObject.transform.position;
        transform.rotation = spline.gameObject.transform.rotation;
        for (int i = 0; i < controlPoints.Length; i++)
        {
            Transform item = Instantiate(items[0]) as Transform;
            sphereControlPoints.Add(item.gameObject);
            item.transform.parent = transform;
            item.transform.localPosition = controlPoints[i];
            //print("Line Position: " + item.transform.position);
            //print("Control Position: " + controlPoints[i]);
        }

    }

    // Update is called once per frame
    
    void Update()
    {




        for (int i = 0; i < sphereControlPoints.Count; i++)
        {
            controlPoints[i] = sphereControlPoints[i].transform.localPosition;
        }
        for (int i = 0; i < sphereControlPoints.Count; i++)
        {
            spline.EnforceMode(i);
            sphereControlPoints[i].transform.localPosition = controlPoints[i];
        }



        //if (Input.GetKeyDown(KeyCode.M))
        //{
        //    MovePoint();
        //}
        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    ResetLines();
        //}

        if (frequency <= 0 || items == null || items.Length == 0)
        {
            return;
        }

        float stepSize = frequency * items.Length;

        if (spline.Loop || stepSize == 1)
        {
            stepSize = 1f / stepSize;
        }
        else
        {
            stepSize = 1f / (stepSize - 1);
        }
        for (int p = 0, f = 0; f < frequency; f++)
        {
            for (int i = 0; i < items.Length; i++, p++)
            {
                //Transform item = Instantiate(items[1]) as Transform;
                //lines.Add(item.gameObject);
                Vector3 position = spline.GetPoint(p * stepSize);
                lines[p].transform.position = position;
                if (lookForward)
                {
                    lines[p].transform.LookAt(position + spline.GetDirection(p * stepSize));
                }
                //item.transform.parent = transform;
            }
        }

    }

    public void MovePoint()
    {
        for (int i = 0; i < sphereControlPoints.Count; i++)
        {
            controlPoints[i] = sphereControlPoints[i].transform.localPosition;
        }
        for (int i = 0; i < sphereControlPoints.Count; i++)
        {
            spline.EnforceMode(i);
            sphereControlPoints[i].transform.localPosition = controlPoints[i];
        }
    }
    
    public void ResetLines()
    {
        
        for (int i = 0; i < sphereControlPoints.Count; i++)
        {
            spline.EnforceMode(i);
            sphereControlPoints[i].transform.localPosition = controlPoints[i];
        }
        
    }
}
