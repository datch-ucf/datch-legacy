using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierVidTut : MonoBehaviour {

    public LineRenderer lineRend;

    public Transform point0;
    public Transform point1;
    public Transform point2;
    public Transform point3;

    private int numPoints = 50;
    private Vector3[] positions = new Vector3[50];

	// Use this for initialization
	void Start () {
        lineRend.positionCount = numPoints;
        //DrawLinearCurve();
        //DrawQuadraticCurve();
        //DrawCubicCurve();
	}
	
	// Update is called once per frame
	void Update () {
        //DrawLinearCurve();
        //DrawQuadraticCurve();
        DrawCubicCurve();
    }

    private void DrawLinearCurve()
    {
        for(int i = 0; i < numPoints; i++)
        {
            float t = i / (float)numPoints;
            positions[i] = CalculateLinearBezierPoint(t, point0.position, point1.position);
        }
        lineRend.SetPositions(positions);
    }

    private void DrawQuadraticCurve()
    {
        for (int i = 0; i < numPoints; i++)
        {
            float t = i / (float)numPoints;
            positions[i] = CalculateQuadraticBezierPoint(t, point0.position, point1.position, point2.position);
        }
        lineRend.SetPositions(positions);
    }

    private void DrawCubicCurve()
    {
        for (int i = 0; i < numPoints; i++)
        {
            float t = i / (float)numPoints;
            positions[i] = CalculateCubicBezierPoint(t, point0.position, point1.position, point2.position, point3.position);
        }
        lineRend.SetPositions(positions);
    }

    // Linear Interpolation. Linear Bezier Curve
    // P = P0 + t(P1 – P0)
    private Vector3 CalculateLinearBezierPoint(float t, Vector3 p0, Vector3 p1)
    {
        // You can use either of these formulas. It returns the same results
        //return (1 - t) * p0 + t * p1;
        return p0 + t * (p1 - p0);
    }

    // Quadratic Bezier Curve
    // B(t) = (1-t)^2 * P0 + 2(1-t)tP1 + t^2 * P2 , 0 < t < 1
    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        //Vector3 p = uu * p0 + 2 * u * t * p1 + tt * p2;
        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;
        return p;
    }


    // Cubic Bezier Curve
    // B(t) = (1-t)^3 * P0 + 3(1-t)^2*t*P1 + 3(1-t)t^2*P2 + t^3*P3 , 0 < t < 1
    private Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;
        return p;
    }
}
