using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planes
{
    public Vector3 pos;

    public Vector3 normal;

    public Planes(Vector3 pos, Vector3 normal)
    {
        this.pos = pos;

        this.normal = normal;
    }
}