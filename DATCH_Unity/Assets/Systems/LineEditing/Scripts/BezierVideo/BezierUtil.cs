using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BezierUtil
{
    float[] sampledLengths;
    Vector3[] points;

    public BezierUtil(Vector3[] points)
    {
        this.points = points;

        GenerateSamples();
    }

    void GenerateSamples()
    {
        Vector3 prevPoint = points[0];
        Vector3 pt;
        float total = 0;

        List<float> samples = new List<float>(10) { 0 };
        float step = 1.0f / 10.0f;
        for (float f = step; f < 1.0f; f += step)
        {
            pt = GetPoint(f);
            total += (pt - prevPoint).magnitude;
            samples.Add(total);

            prevPoint = pt;
        }

        pt = GetPoint(1);
        samples.Add(total + (pt - prevPoint).magnitude);

        sampledLengths = samples.ToArray();
    }

    /// <summary>
    /// Return the normal along a specific line
    /// </summary>
    /// <param name="t">Value between 0 and 1, indicating the time parameter</param>
    /// <param name="up">A normalized up vector</param>
    /// <returns></returns>
    Vector3 GetNormal(float t, Vector3 up)
    {
        return CalculateNormal(GetTangent(t), up);
    }

    Vector3 CalculateNormal(Vector3 tangent, Vector3 up)
    {
        Vector3 binormal = Vector3.Cross(up, tangent);
        return Vector3.Cross(tangent, binormal);
    }

    Vector3 CalculateTangent(float t, float t2, float it2)
    {
        return (points[0] * -it2 +
            points[1] * (t * (3 * t - 4) + 1) +
            points[2] * (-3 * t2 + t * 2) +
            points[3] * t2).normalized;
    }

    Vector3 CalculatePoint(float t, float t2, float t3, float it, float it2, float it3)
    {
        return points[0] * (it3) +
            points[1] * (3 * it2 * t) +
            points[2] * (3 * it * t2) +
            points[3] * t3;
    }

    public Vector3 GetTangent(float t)
    {
        float t2 = t * t;
        float it = (1 - t);
        float it2 = it * it;

        return CalculateTangent(t, t2, it2);
    }

    public Vector3 GetPoint(float t, out Vector3 tangent, out Vector3 normal, out Quaternion orientation)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        float it = (1 - t);
        float it2 = it * it;
        float it3 = it * it * it;

        tangent = CalculateTangent(t, t2, it2);
        normal = CalculateNormal(tangent, Vector3.up);
        orientation = Quaternion.LookRotation(tangent, normal);

        return CalculatePoint(t, t2, t3, it, it2, it3);
    }

    public Vector3 GetPoint(float t, out Vector3 tangent, out Vector3 normal)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        float it = (1 - t);
        float it2 = it * it;
        float it3 = it * it * it;

        tangent = CalculateTangent(t, t2, it2);
        normal = CalculateNormal(tangent, Vector3.up);

        return CalculatePoint(t, t2, t3, it, it2, it3);
    }

    public Vector3 GetPoint(float t, out Vector3 tangent)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        float it = (1 - t);
        float it2 = it * it;
        float it3 = it * it * it;

        tangent = CalculateTangent(t, t2, it2);
        return CalculatePoint(t, t2, t3, it, it2, it3);
    }

    public Vector3 GetPoint(float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        float it = (1 - t);
        float it2 = it * it;
        float it3 = it * it * it;

        return CalculatePoint(t, t2, t3, it, it2, it3);
    }

    public Quaternion GetOrientation(float t)
    {
        Vector3 tangent, normal;
        GetPoint(t, out tangent, out normal);

        return Quaternion.LookRotation(tangent, normal);
    }

    // A slightly faster method
    //public Vector3 GetPoint(float t)
    //{
    //    float t2 = t * t;
    //    float t3 = t2 * t;

    //    return points[0] * (-t3 + 3 * t2 - 3 * t + 1) +
    //        points[1] * (3 * t3 - 6 * t2 + 3 * t) +
    //        points[2] * (-3 * t3 + 3 * t2) +
    //        points[3] * t3;
    //}

    // This is the slowest method of lerping a spline
    //public Vector3 GetPoint(float t)
    //{
    //    Vector3 a = Vector3.Lerp(points[0], points[1], t);
    //    Vector3 b = Vector3.Lerp(points[1], points[2], t);
    //    Vector3 c = Vector3.Lerp(points[2], points[3], t);
    //    Vector3 d = Vector3.Lerp(a, b, t);
    //    Vector3 e = Vector3.Lerp(b, c, t);

    //    return Vector3.Lerp(d, e, t);
    //}

    public OrientedPoint GetOrientedPoint(float t)
    {
        Vector3 tangent, normal;
        Quaternion orientation;

        Vector3 point = GetPoint(t, out tangent, out normal, out orientation);

        return new OrientedPoint(point, orientation, sampledLengths.Sample(t));
    }

    List<int> meshLines = new List<int>();
    public Mesh ToMesh(Vector2[] verts, Vector2[] normals, float[] uCoords)
    {
        Mesh mesh = new Mesh();

        meshLines.Clear();
        for (int i = 0; i < verts.Length - 1; i++)
        {
            meshLines.Add(i);
            meshLines.Add(i + 1);
        }

        return mesh;
    }

    public IEnumerable<OrientedPoint> GeneratePath(float subDivisions)
    {
        float step = 1.0f / subDivisions;

        for (float f = 0; f < 1; f += step)
        {
            yield return GetOrientedPoint(f);
        }

        yield return GetOrientedPoint(1);
    }

    public void Extrude(Mesh mesh, ExtrudeShape shape, OrientedPoint[] path)
    {
        int vertsInShape = shape.verts.Length;
        int segments = path.Length - 1;
        int edgeLoops = path.Length;
        int vertCount = vertsInShape * edgeLoops;
        int triCount = shape.Lines.Length * segments * 2;
        int triIndexCount = triCount * 3;

        int[] triangleIndices = new int[triIndexCount];
        Vector3[] vertices = new Vector3[vertCount];
        Vector3[] normals = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];

        // Generate all of the vertices and normals
        for (int i = 0; i < path.Length; i++)
        {
            int offset = i * vertsInShape;
            for (int j = 0; j < vertsInShape; j++)
            {
                int id = offset + j;
                vertices[id] = path[i].LocalToWorld(shape.verts[j]);
                normals[id] = path[i].LocalToWorldDirection(shape.normals[j]);
                uvs[id] = new Vector2(shape.uCoords[j], path[i].vCoordinate);
            }
        }

        // Generate all of the triangles
        int ti = 0;
        for (int i = 0; i < segments; i++)
        {
            int offset = i * vertsInShape;
            for (int l = 0; l < shape.Lines.Length; l += 2)
            {
                int a = offset + shape.Lines[l];
                int b = offset + shape.Lines[l] + vertsInShape;
                int c = offset + shape.Lines[l + 1] + vertsInShape;
                int d = offset + shape.Lines[l + 1];
                triangleIndices[ti++] = a;
                triangleIndices[ti++] = b;
                triangleIndices[ti++] = c;
                triangleIndices[ti++] = c;
                triangleIndices[ti++] = d;
                triangleIndices[ti++] = a;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangleIndices;
    }
}

public class ExtrudeShape
{
    public Vector2[] verts;
    public Vector2[] normals;
    public float[] uCoords;

    IEnumerable<int> LineSegment(int i)
    {
        yield return i;
        yield return i + 1;
    }

    int[] lines;
    public int[] Lines
    {
        get
        {
            if (lines == null)
            {
                lines = Enumerable.Range(0, verts.Length - 1)
                    .SelectMany(i => LineSegment(i))
                    .ToArray();
            }

            return lines;
        }
    }
};

public struct OrientedPoint
{
    public Vector3 position;
    public Quaternion rotation;
    public float vCoordinate;

    public OrientedPoint(Vector3 position, Quaternion rotation, float vCoordinate = 0)
    {
        this.position = position;
        this.rotation = rotation;
        this.vCoordinate = vCoordinate;
    }

    public Vector3 LocalToWorld(Vector3 point)
    {
        return position + rotation * point;
    }

    public Vector3 WorldToLocal(Vector3 point)
    {
        return Quaternion.Inverse(rotation) * (point - position);
    }

    public Vector3 LocalToWorldDirection(Vector3 dir)
    {
        return rotation * dir;
    }
}

public static class FloatArrayExtensions
{
    public static float Sample(this float[] fArr, float t)
    {
        int count = fArr.Length;
        if (count == 0)
        {
            Debug.LogError("Unable to sample array - it has no elements.");
            return 0;
        }

        if (count == 1) return fArr[0];

        float f = t * (count - 1);
        int idLower = Mathf.FloorToInt(f);
        int idUpper = Mathf.FloorToInt(f + 1);

        if (idUpper >= count) return fArr[count - 1];
        if (idLower < 0) return fArr[0];

        return Mathf.Lerp(fArr[idLower], fArr[idUpper], f - idLower);
    }
}