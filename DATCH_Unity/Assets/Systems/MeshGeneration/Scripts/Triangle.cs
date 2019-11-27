using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle
{
    //Corners
    public Vert v1;
    public Vert v2;
    public Vert v3;

    //If we are using the half edge mesh structure, we just need one half edge
    public HalfEdge halfEdge;

    public Triangle(Vert v1, Vert v2, Vert v3)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
    }

    public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        this.v1 = new Vert(v1);
        this.v2 = new Vert(v2);
        this.v3 = new Vert(v3);
    }

    public Triangle(HalfEdge halfEdge)
    {
        this.halfEdge = halfEdge;
    }

    //Change orientation of triangle from cw -> ccw or ccw -> cw
    public void ChangeOrientation()
    {
        Vert temp = this.v1;

        this.v1 = this.v2;

        this.v2 = temp;
    }
}