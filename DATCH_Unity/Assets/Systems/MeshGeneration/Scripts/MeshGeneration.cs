using HoloToolkit.Examples.GazeRuler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class MeshGeneration : MonoBehaviour
{

    public int xSize, ySize;
    private Vector3[] vertices;
    private Mesh mesh;
    public static List<int> t = new List<int>();
    List<Triangle> tris = new List<Triangle>();
  
    void Start()
    {
        //Generate();
    }    

    public void DrawMesh()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Mesh Test";
        //Vector2[] uv = new Vector2[vertices.Length];
        List<Vector3> knownTriangles = new List<Vector3>();
        List<int> triangles = new List<int>();
        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(2);
        ///int numTriangles = vertices.Count - 2;

        knownTriangles.Add(new Vector3(0, 0, 0));
        knownTriangles.Add(new Vector3(2, 5, 0));
        knownTriangles.Add(new Vector3(1, 0, 0));



        float angle1 = Vector3.Angle(knownTriangles[0] - knownTriangles[1], knownTriangles[2] - knownTriangles[1]); ;
        float angle2 = Vector3.Angle(knownTriangles[1] - knownTriangles[2], knownTriangles[0] - knownTriangles[2]); ;
        float angle3 = Vector3.Angle(knownTriangles[2] - knownTriangles[0], knownTriangles[1] - knownTriangles[0]); ;
        print(angle1);
        print(angle2);
        print(angle3);

        float angleSum = Mathf.Round((angle1 + angle2 + angle3));
        print(angleSum);
        if ((angleSum) == 180.0f)
        {
            // You have a triangle
            print("This is a triangle");

            mesh.vertices = knownTriangles.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
        }





    }

    
    public void DrawMesh(List<Vector3> verts)
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Mesh Test";
        //verts.Reverse(); // Reverse the verts because that how the triangulation functions work.
        mesh.vertices = verts.ToArray();

        List<Vert> vertex = new List<Vert>();

        // Create new vert objects from the verts Vector3s
        foreach (Vector3 v in verts)
        {
            vertex.Add(new Vert(v));
        }

        t = new List<int>();
        tris = TriangulateConvexPolygon(vertex);
        //tris = TriangulateConcavePolygon(verts, vertexRoots);
        mesh.triangles = t.ToArray();

        FlipNormals(mesh);

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = true;
    }

    private void FlipNormals(Mesh mesh)
    {
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -1 * normals[i];
        }
        mesh.normals = normals;

        for (int m = 0; m < mesh.subMeshCount; m++)
        {
            int[] t = mesh.GetTriangles(m);
            for (int i = 0; i < t.Length; i += 3)
            {
                int temp = t[i];
                t[i] = t[i + 1];
                t[i + 1] = temp;
            }
            mesh.SetTriangles(t, m);
        }
    }

    public void SetMeshColor(Color color)
    {
        Material material = GetComponent<MeshRenderer>().material;

        Color tempColor = color; 
        tempColor.a = .68f; // lower alpha
        material.SetColor("_TintColor", tempColor);
        material.SetColor("_Color", tempColor);        
    }   

    public static List<Triangle> TriangulateConvexPolygon(List<Vert> convexHullpoints)
    {
        List<Triangle> triangles = new List<Triangle>();

        for (int i = 2; i < convexHullpoints.Count; i++)
        {
            Vert a = convexHullpoints[0];
            Vert b = convexHullpoints[i - 1];
            Vert c = convexHullpoints[i];
            t.Add(0);
            t.Add(i - 1);
            t.Add(i);
            triangles.Add(new Triangle(a, b, c));
        }

        return triangles;
    }


    private void Generate()
    {
        WaitForSeconds wait = new WaitForSeconds(0.05f);

        GetComponent<MeshFilter>().mesh = mesh = new Mesh(); // create a new mesh
        mesh.name = "Procedural Grid";

        // The amount of vertices depend of the size of the grid.
        // We need a vertex at the corners of ever quad, but adjacent guads can share the same vertex.
        // So we need one more vertex than we have tiles in each dimension
        vertices = new Vector3[(xSize + 1) * (ySize + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
        for (int i = 0, y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                vertices[i] = new Vector3(x, y);
                uv[i] = new Vector2((float)x / xSize, (float)y / ySize);
                tangents[i] = tangent;
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.tangents = tangents;

        // Triangles test
        // Each triangle consists of 3 points
        int[] triangles = new int[xSize * ySize * 6];
        //triangles[0] = 0;
        //triangles[1] = xSize + 1;
        //triangles[2] = 1;
        //triangles[3] = 1;
        //triangles[4] = xSize + 1;
        //triangles[5] = xSize + 2;
        // or we can reduce by doing this:
        //triangles[0] = 0;                
        //triangles[3] = triangles[2] = 1;
        //triangles[4] = triangles[1] = xSize + 1;
        //triangles[5] = xSize + 2;


        for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
                // mesh.triangles = triangles; // to see mesh generate one by one
            }
        }

        foreach (int t in triangles)
        {
            print(t);
        }
        //print(triangles.Length);
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        if (vertices == null)
            return;

        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }

    #region Concave Polygon Triangulation
    //This assumes that we have a polygon and now we want to triangulate it
    //The points on the polygon should be ordered counter-clockwise
    //This alorithm is called ear clipping and it's O(n*n) Another common algorithm is dividing it into trapezoids and it's O(n log n)
    //One can maybe do it in O(n) time but no such version is known
    //Assumes we have at least 3 points
    public static List<Triangle> TriangulateConcavePolygon(List<Vector3> points, List<GameObject> roots)// TODO: changed
    {
        //The list with triangles the method returns
        List<Triangle> triangles = new List<Triangle>();

        //If we just have three points, then we dont have to do all calculations
        if (points.Count == 3)
        {
            triangles.Add(new Triangle(points[0], points[1], points[2]));
            t.Add(0);
            t.Add(1);
            t.Add(2);
            return triangles;
        }



        //Step 1. Store the vertices in a list and we also need to know the next and prev vertex
        List<Vert> vertices = new List<Vert>();

        for (int i = 0; i < points.Count; i++)
        {
            vertices.Add(new Vert(points[i]));
        }

        //Find the next and previous vertex
        for (int i = 0; i < vertices.Count; i++)
        {
            int nextPos = ClampListIndex(i + 1, vertices.Count);

            int prevPos = ClampListIndex(i - 1, vertices.Count);

            vertices[i].prevVertex = vertices[prevPos];

            vertices[i].nextVertex = vertices[nextPos];
        }



        //Step 2. Find the reflex (concave) and convex vertices, and ear vertices
        for (int i = 0; i < vertices.Count; i++)
        {
            CheckIfReflexOrConvex(vertices[i]);
        }


        //Have to find the ears after we have found if the vertex is reflex or convex
        List<Vert> earVertices = new List<Vert>();

        for (int i = 0; i < vertices.Count; i++)
        {
            IsVertexEar(vertices[i], vertices, earVertices);
        }


        
        //Step 3. Triangulate!
        while (true)
        {
            //This means we have just one triangle left
            if (vertices.Count == 3)
            {
                //The final triangle
                triangles.Add(new Triangle(vertices[0], vertices[0].prevVertex, vertices[0].nextVertex));

                break;
            }
            if (earVertices.Count > 0)
            {
                //Make a triangle of the first ear
                Vert earVertex = earVertices[0];

                Vert earVertexPrev = earVertex.prevVertex;
                Vert earVertexNext = earVertex.nextVertex;

                Triangle newTriangle = new Triangle(earVertex, earVertexPrev, earVertexNext);

                triangles.Add(newTriangle);

                //Remove the vertex from the lists
                earVertices.Remove(earVertex);

                vertices.Remove(earVertex);

                //Update the previous vertex and next vertex
                earVertexPrev.nextVertex = earVertexNext;
                earVertexNext.prevVertex = earVertexPrev;

                //...see if we have found a new ear by investigating the two vertices that was part of the ear
                CheckIfReflexOrConvex(earVertexPrev);
                CheckIfReflexOrConvex(earVertexNext);

                earVertices.Remove(earVertexPrev);
                earVertices.Remove(earVertexNext);

                IsVertexEar(earVertexPrev, vertices, earVertices);
                IsVertexEar(earVertexNext, vertices, earVertices);



            }
        }

        Debug.Log(triangles.Count);
        return triangles;
    }

    //Check if a vertex if reflex or convex, and add to appropriate list
    private static void CheckIfReflexOrConvex(Vert v)
    {
        v.isReflex = false;
        v.isConvex = false;

        //This is a reflex vertex if its triangle is oriented clockwise
        Vector2 a = v.prevVertex.GetPos2D_XZ();
        Vector2 b = v.GetPos2D_XZ();
        Vector2 c = v.nextVertex.GetPos2D_XZ();

        if (IsTriangleOrientedClockwise(a, b, c))
        {
            v.isReflex = true;
        }
        else
        {
            v.isConvex = true;
        }
    }

    //Check if a vertex is an ear
    private static void IsVertexEar(Vert v, List<Vert> vertices, List<Vert> earVertices)
    {
        //A reflex vertex cant be an ear!
        if (v.isReflex)
        {
            return;
        }

        //This triangle to check point in triangle
        Vector2 a = v.prevVertex.GetPos2D_XZ();
        Vector2 b = v.GetPos2D_XZ();
        Vector2 c = v.nextVertex.GetPos2D_XZ();

        bool hasPointInside = false;

        for (int i = 0; i < vertices.Count; i++)
        {
            //We only need to check if a reflex vertex is inside of the triangle
            if (vertices[i].isReflex)
            {
                Vector2 p = vertices[i].GetPos2D_XZ();

                //This means inside and not on the hull
                if (IsPointInTriangle(a, b, c, p))
                {
                    hasPointInside = true;

                    break;
                }
            }
        }

        if (!hasPointInside)
        {
            earVertices.Add(v);
        }
    }

    //Is a triangle in 2d space oriented clockwise or counter-clockwise
    //https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
    //https://en.wikipedia.org/wiki/Curve_orientation
    public static bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        bool isClockWise = true;

        float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

        if (determinant > 0f)
        {
            isClockWise = false;
        }

        return isClockWise;
    }

    //Clamp list indices
    //Will even work if index is larger/smaller than listSize, so can loop multiple times
    public static int ClampListIndex(int index, int listSize)
    {
        index = ((index % listSize) + listSize) % listSize;

        return index;
    }

    //From http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
    //p is the testpoint, and the other points are corners in the triangle
    public static bool IsPointInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
    {
        bool isWithinTriangle = false;

        //Based on Barycentric coordinates
        float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

        float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
        float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
        float c = 1 - a - b;

        //The point is within the triangle or on the border if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
        //if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
        //{
        //    isWithinTriangle = true;
        //}

        //The point is within the triangle
        if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
        {
            isWithinTriangle = true;
        }

        return isWithinTriangle;
    }
    #endregion
}

