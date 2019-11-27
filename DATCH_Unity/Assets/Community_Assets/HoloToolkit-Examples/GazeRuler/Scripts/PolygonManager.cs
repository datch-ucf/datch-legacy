// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using HoloToolkit.Unity;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Examples.InteractiveElements;
using System;
using System.Linq;
using UnityEngine.UI;
using System.Collections;

namespace HoloToolkit.Examples.GazeRuler
{
    /// <summary>
    /// manager all geometries in the scene
    /// </summary>
    public class PolygonManager : Singleton<PolygonManager>, IGeometry, IPolygonClosable
    {

        // save all geometries
        public List<Polygon> Polygons = new List<Polygon>();
        public Polygon CurrentPolygon;
        public Dictionary<Vertex, List<Vertex>> graph = new Dictionary<Vertex, List<Vertex>>();

        // This will be used to store the last lines and point for the shape you draw    
        private Vertex lastVertex = null;
        public GameObject worldRoot;
        private int vertexCounter = 0;
        // Use this for initialization
        private void Start()
        {
            CurrentPolygon = SetUpNewPolygon();
        }


        /// <summary>
        /// reset current unfinished geometry
        /// </summary>
        public void Reset()
        {
            if (CurrentPolygon != null && !CurrentPolygon.IsFinished)
            {
                Destroy(CurrentPolygon.Root);
                CurrentPolygon = SetUpNewPolygon();
            }
            Polygons = new List<Polygon>();
            graph = new Dictionary<Vertex, List<Vertex>>();
            MeshGenerationManager.Instance.DeleteAllMeshesInScene();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                print(CurrentPolygon.Vertices.Count);

                foreach (Vertex v in CurrentPolygon.Vertices)
                {
                    print(v.Root.name);
                }

                UpdateAllPointPositions();
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                print("Verticies Count " + CurrentPolygon.Vertices.Count);
                print("Lines Count " + CurrentPolygon.Lines.Count);

            }
        }


        /// <summary>
        ///  handle new point users place
        /// </summary>
        /// <param name="LinePrefab"></param>
        /// <param name="PointPrefab"></param>
        /// <param name="TextPrefab"></param>
        /// You pass in loaded point to true if you are loading points form shapefiles. Then you pass the position
        public void AddPoint(GameObject LinePrefab, GameObject PointPrefab, GameObject TextPrefab, bool loadedPoint = false, Vector3 pointPosition = new Vector3())
        {
            print("ADDING POINT!!!!");

            var hitPoint = GazeManager.Instance.HitPosition;
            var point = (GameObject)Instantiate(PointPrefab, hitPoint, Quaternion.identity);
            point.name = vertexCounter.ToString();
            vertexCounter++;
            Vertex newVertex = SetupNewVertex(point, hitPoint);

            // Handdraggable check
            if (point.GetComponent<HandDraggable>() != null)
            {
                point.GetComponent<HandDraggable>().enabled = true;
                //StartCoroutine(point.GetComponent<HandDraggable>().DisableAfterSpawn());
            }
            
            // Creating brand new polygon
            if (CurrentPolygon.IsFinished)
            {
                CurrentPolygon = SetUpNewPolygon();
                // graph
                if (!graph.ContainsKey(newVertex))
                {
                    graph.Add(newVertex, new List<Vertex>());
                }
                CurrentPolygon.Vertices.Add(newVertex);
                // Change the parent later. It will work differently when merging shapes and duplicating
                newVertex.Root.transform.parent = worldRoot.transform;
            }
            else
            {
                
                //// Check to see if you have clicked on a vertex and set lastVertex to the appropriate vertex
                //if (CurrentPolygon.Vertices.Count != 0)
                //{
                //    if (!clickedNewVertex)
                //    {
                //        print("setting last vertex");
                //        lastVertex = CurrentPolygon.Vertices[CurrentPolygon.Vertices.Count - 1];
                //    }
                //    else
                //    {
                //        // this check if you clicked on a new vertex and you are not the same as lastVertex
                //        if (lastVertex != tempLastVertex && tempLastVertex != null)
                //        {
                //            print("might go here 2");
                //            lastVertex = tempLastVertex;
                //            //vertices.Add(lastVertex);
                //            tempLastVertex = null;
                //        }
                //    }
                //}
                

                // graph
                graph.Add(newVertex, new List<Vertex>());                
                CurrentPolygon.Vertices.Add(newVertex);
                newVertex.Root.transform.parent = worldRoot.transform;

                if (graph.Count > 1 && CurrentPolygon.Vertices.Count > 1)
                {
                    if (CurrentPolygon.Lines.Count >= 3)
                    {
                        Line lineToDelete = CurrentPolygon.Lines[CurrentPolygon.Lines.Count - 1];
                        CurrentPolygon.Lines.Remove(lineToDelete);
                        Destroy(lineToDelete.Root);
                        graph[CurrentPolygon.Vertices[0]].Remove(CurrentPolygon.Vertices[CurrentPolygon.Vertices.Count - 1]);
                        graph[CurrentPolygon.Vertices[CurrentPolygon.Vertices.Count - 1]].Remove(CurrentPolygon.Vertices[0]);
                    }
                    
                    var index = CurrentPolygon.Vertices.Count - 1;
                    lastVertex = CurrentPolygon.Vertices[index - 1];
                    var centerPos = (newVertex.Position + lastVertex.Position) * 0.5f;
                    var direction = newVertex.Position - lastVertex.Position;
                    var distance = Vector3.Distance(newVertex.Position, lastVertex.Position);
                    var line = (GameObject)Instantiate(LinePrefab, centerPos, Quaternion.LookRotation(direction));
                    
                    // Set the properties of the new line created
                    float lineWeight = LineWeight.lineWeight;
                    line.transform.localScale = new Vector3(distance, lineWeight, lineWeight); // default .005f
                    line.transform.Rotate(Vector3.down, 90f);
                    // set world root as parent of line
                    line.transform.parent = worldRoot.transform;

                    // Add the neighbors to neighbor you connect with
                    graph[lastVertex].Add(newVertex);
                    graph[newVertex].Add(lastVertex);

                    // Previous Vertex
                    Line spawnedLine = SetupNewLine(line, distance, lastVertex, newVertex);

                    CurrentPolygon.Lines.Add(spawnedLine);

                    lastVertex = CurrentPolygon.Vertices[CurrentPolygon.Vertices.Count - 1];
                    
                }

                if (CurrentPolygon.Vertices.Count >= 3)
                {
                    ContinueTriangle();
                }
            }
        }

        #region Polygon properties setup
        public Line SetupNewLine(GameObject line, float distance, Vertex startVertex, Vertex endVertex)
        {
            Line newLine = new Line
            {
                //Start = lastPoint.Position,
                //End = hitPoint,
                Root = line,
                Distance = distance,
                startPoint = startVertex.Root,
                endPoint = endVertex.Root
            };
            return newLine;
        }


        public Vertex SetupNewVertex(GameObject point, Vector3 hitPoint)
        {
            Vertex newVert = new Vertex
            {
                Root = point,
                Position = hitPoint,
                Lines = new List<Line>(),
                ParentPolygon = CurrentPolygon.Root
            };

            return newVert;
        }

        public Polygon SetUpNewPolygon()
        {
            Polygon newPoly = new Polygon()
            {
                IsFinished = false,
                Root = new GameObject("Polygon"),

                Points = new List<Vector3>(),

                Lines = new List<Line>(),
                Vertices = new List<Vertex>()
            };
            return newPoly;
        }
        #endregion

        //
        public bool ContinueTriangle()
        {
            lastVertex = CurrentPolygon.Vertices[CurrentPolygon.Vertices.Count - 1];
            ClickToClose(CurrentPolygon.Vertices[0].Root);
            return true;
        }

        // TODO: OLD METHOD TO CLOSE. NOT USED ANYMORE
        /// <summary>
        /// finish current geometry
        /// </summary>
        /// <param name="LinePrefab"></param>
        /// <param name="TextPrefab"></param>
        public void ClosePolygon(GameObject LinePrefab, GameObject TextPrefab)
        {
            if (CurrentPolygon != null)
            {
                CurrentPolygon.IsFinished = true;
                var area = CalculatePolygonArea(CurrentPolygon);
                var index = CurrentPolygon.Points.Count - 1;
                var centerPos = (CurrentPolygon.Points[index] + CurrentPolygon.Points[0]) * 0.5f;
                var direction = CurrentPolygon.Points[index] - CurrentPolygon.Points[0];
                var distance = Vector3.Distance(CurrentPolygon.Points[index], CurrentPolygon.Points[0]);
                var line = (GameObject)Instantiate(LinePrefab, centerPos, Quaternion.LookRotation(direction));

                // Change individual line color
                //Renderer lineRend = line.GetComponent<Renderer>();
                //lineRend.material.SetColor("_Color", LineEditing.lineColor);

                // set world root as parent of line
                line.transform.parent = worldRoot.transform;

                // Set line properties
                float lineWeight = LineWeight.lineWeight;
                line.transform.localScale = new Vector3(distance, lineWeight, lineWeight); // default .005f
                line.transform.Rotate(Vector3.down, 90f);

                var vect = new Vector3(0, 0, 0);
                foreach (var point in CurrentPolygon.Points)
                {
                    vect += point;
                }
                var centerPoint = vect / (index + 1);
                var direction1 = CurrentPolygon.Points[1] - CurrentPolygon.Points[0];
                var directionF = Vector3.Cross(direction, direction1);

                // REMOVED: This is for the tip that gets created after you close a polygon. Useful to figure out the area? Could work for mesh creation.
                //var tip = (GameObject)Instantiate(TextPrefab, centerPoint, Quaternion.LookRotation(directionF));//anchor.x + anchor.y + anchor.z < 0 ? -1 * anchor : anchor));

                // unit is ㎡
                //tip.GetComponent<TextMesh>().text = area + "㎡";
                //tip.transform.parent = CurrentPolygon.Root.transform;


                // Previous Vertex
                Line spawnedLine = new Line
                {
                    Root = line,
                    Distance = distance,
                    startPoint = CurrentPolygon.Vertices[0].Root,
                    endPoint = CurrentPolygon.Vertices[index].Root
                };
                //CurrentPolygon.Vertices[index - 1].Lines.Add(spawnedLine);

                // the preveious vertext before the one you connect to
                // Current Vertex
                spawnedLine = new Line
                {
                    //Start = lastPoint.Position,
                    //End = hitPoint,
                    Root = line,
                    Distance = distance,
                    startPoint = CurrentPolygon.Vertices[index].Root,
                    endPoint = CurrentPolygon.Vertices[0].Root
                };

                //CurrentPolygon.Vertices[index].Lines.Add(spawnedLine);

                //CurrentPolygon.Lines.Add(spawnedLine);
                //LinesGo.Push(line);
                Polygons.Add(CurrentPolygon);
            }
        }


        // TODO: Test out the clearing of all shapes
        /// <summary>
        /// clear all geometries in the scene
        /// </summary>
        public void Clear()
        {
            GameObject[] Lines = GameObject.FindGameObjectsWithTag("Line");
            GameObject[] Vertices = GameObject.FindGameObjectsWithTag("Vertex");
            GameObject[] Meshes = GameObject.FindGameObjectsWithTag("GeneratedMesh");

            foreach (GameObject line in Lines)
            {
                Destroy(line);
            }

            foreach (GameObject vertex in Vertices)
            {
                Destroy(vertex);
            }

            foreach (GameObject mesh in Meshes)
            {
                Destroy(mesh);
            }

            Reset();
        }

        // TODO: Test out single shapes
        // delete latest geometry
        public void Delete()
        {
            if (Polygons != null && Polygons.Count > 0)
            {
                var lastPoly = Polygons[Polygons.Count - 1];
                Polygons.RemoveAt(Polygons.Count - 1);
                Destroy(lastPoly.Root);
            }
        }
        #region Area Caluculations
        /// <summary>
        /// Calculate an area of triangle
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        private float CalculateTriangleArea(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var a = Vector3.Distance(p1, p2);
            var b = Vector3.Distance(p1, p3);
            var c = Vector3.Distance(p3, p2);
            var p = (a + b + c) / 2f;
            var s = Mathf.Sqrt(p * (p - a) * (p - b) * (p - c));

            return s;
        }

        /// <summary>
        /// Calculate an area of geometry
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        private float CalculatePolygonArea(Polygon polygon)
        {
            var s = 0.0f;
            var i = 1;
            var n = polygon.Vertices.Count;
            for (; i < n - 1; i++)
                s += CalculateTriangleArea(polygon.Vertices[0].Position, polygon.Vertices[i].Position, polygon.Vertices[i + 1].Position);
            return 0.5f * Mathf.Abs(s);
        }
        #endregion

        // TODO: Redo this too!!
        public void ClickToClose(GameObject vertex)
        {
            Vertex newVertex = GetVertex(vertex);
            // Check if you are trying to connect to yourself
            if (newVertex == lastVertex)
            {
                print("cant connect to self");
                return;
            }
            
            var centerPos = (lastVertex.Position + newVertex.Position) * 0.5f;
            var direction = lastVertex.Position - newVertex.Position;
            var distance = Vector3.Distance(lastVertex.Position, newVertex.Position);
            var line = (GameObject)Instantiate(MeasureManager.Instance.LinePrefab, centerPos, Quaternion.LookRotation(direction));


            float lineWeight = LineWeight.lineWeight;
            line.transform.localScale = new Vector3(distance, lineWeight, lineWeight); // default .005f
            line.transform.Rotate(Vector3.down, 90f);
            // set world root as parent of line
            line.transform.parent = worldRoot.transform;

            // Set neigbors for each vertex you are creating a line between
            graph[lastVertex].Add(newVertex);
            graph[newVertex].Add(lastVertex);

            //Previous Vertex
            Line spawnedLine = SetupNewLine(line, distance, lastVertex, newVertex);

            CurrentPolygon.Lines.Add(spawnedLine);

            lastVertex = CurrentPolygon.Vertices[CurrentPolygon.Vertices.Count - 1];

            // Create the mesh after it's closed
            if (CurrentPolygon.mesh == null)
                CreatePolygonMesh();
            else
                UpdatePolygonMesh();

            //StartCoroutine(DeleteNullLines());
        }

        // Find and return a vertx from graph dictionary
        private Vertex GetVertex(GameObject vertex)
        {
            Vertex tempVertex = null;
            foreach (KeyValuePair<Vertex, List<Vertex>> entry in graph)
            {
                if (entry.Key.Root == vertex)
                {
                    tempVertex = entry.Key;
                    break;
                }
            }
            return tempVertex;
        }

        // This adds your current polygon to the polygons list
        public void ClosePolygon()
        {
            if (CurrentPolygon.Vertices.Count >= 3 && !CurrentPolygon.IsFinished)
            {
                CurrentPolygon.IsFinished = true;
                Polygons.Add(CurrentPolygon);
            }
        }
        
        public void SetNewVertex(GameObject vertex)
        {
            if (CurrentPolygon.Vertices.Exists(v => v.Root == vertex) && !CurrentPolygon.IsFinished)
            {
                print("No can do");
                return;
            }
            else
            {
                if (CurrentPolygon.IsFinished)
                {
                    CurrentPolygon = SetUpNewPolygon();                    
                    
                    lastVertex = GetVertex(vertex);

                    
                    CurrentPolygon.Vertices.Add(lastVertex);

                    if (!graph.ContainsKey(lastVertex))
                    {
                        graph.Add(lastVertex, new List<Vertex>());
                    }

                    // Don't paretnt it because this vertex is already part of a polygon
                    //lastVertex.Root.transform.parent = CurrentPolygon.Root.transform;
                }
                else
                {
                    Vertex newVertex = GetVertex(vertex);
                    CurrentPolygon.Vertices.Add(newVertex);

                    if (graph.Count > 1 && CurrentPolygon.Vertices.Count > 1)
                    {

                        if (CurrentPolygon.Lines.Count >= 3)
                        {
                            Line lineToDelete = CurrentPolygon.Lines[CurrentPolygon.Lines.Count - 1];
                            CurrentPolygon.Lines.Remove(lineToDelete);
                            Destroy(lineToDelete.Root);
                            graph[CurrentPolygon.Vertices[0]].Remove(CurrentPolygon.Vertices[CurrentPolygon.Vertices.Count - 1]);
                            graph[CurrentPolygon.Vertices[CurrentPolygon.Vertices.Count - 1]].Remove(CurrentPolygon.Vertices[0]);
                        }


                        var index = CurrentPolygon.Vertices.Count - 1;
                        lastVertex = CurrentPolygon.Vertices[index - 1];
                        var centerPos = (newVertex.Position + lastVertex.Position) * 0.5f;
                        var direction = newVertex.Position - lastVertex.Position;
                        var distance = Vector3.Distance(newVertex.Position, lastVertex.Position);
                        var line = (GameObject)Instantiate(MeasureManager.Instance.LinePrefab, centerPos, Quaternion.LookRotation(direction));                        

                        // Set the properties of the new line created
                        float lineWeight = LineWeight.lineWeight;
                        line.transform.localScale = new Vector3(distance, lineWeight, lineWeight); // default .005f
                        line.transform.Rotate(Vector3.down, 90f);
                        // set world root as parent of line
                        line.transform.parent = worldRoot.transform;

                        // Add the neighbors to neighbor you connect with
                        graph[lastVertex].Add(newVertex);
                        graph[newVertex].Add(lastVertex);

                        // Previous Vertex
                        Line spawnedLine = SetupNewLine(line, distance, lastVertex, newVertex);

                        CurrentPolygon.Lines.Add(spawnedLine);

                        lastVertex = CurrentPolygon.Vertices[CurrentPolygon.Vertices.Count - 1];
                        
                    }

                    if (CurrentPolygon.Vertices.Count >= 3)
                    {
                        ContinueTriangle();
                    }
                }


            }
        }

        // Probably don't need this anymore
        private bool DoesLineExist(Line line)
        {
            bool deleteLineRef = true;
            int existingLine = 0;
            if (Polygons != null && Polygons.Count > 0)
            {
                foreach (Polygon p in Polygons)
                {
                    foreach (Line l in p.Lines)
                    {
                        if (line.Root == l.Root)
                            existingLine++;
                    }
                    if (existingLine > 0)
                        break;
                }

                if (existingLine > 0)
                    deleteLineRef = false;
            }

            return deleteLineRef;
        }
        
        // TODO: FIX After proper mesh creation
        IEnumerator DeleteLine(GameObject line, bool clickedLine = false)
        {
            // Find the line in the Lines list
            Line lineToDelete = CurrentPolygon.Lines.Find(l => l.Root == line);// This is if we are undoing a point. Might want to consider if is a clicked line
            bool deleteLineRef = true;

            deleteLineRef = DoesLineExist(lineToDelete);

            if (lineToDelete != null)
            {
                int startVertexCounter = 0;
                int endVertexCounter = 0;
                foreach (Polygon p in Polygons)
                {
                    foreach (Vertex v in p.Vertices)
                    {

                        if (lineToDelete.startPoint == v.Root)
                            startVertexCounter++;
                        if (lineToDelete.endPoint == v.Root)
                            endVertexCounter++;
                    }
                }

                foreach (Vertex v in CurrentPolygon.Vertices)
                {
                    if (lineToDelete.startPoint == v.Root)
                        startVertexCounter++;
                    if (lineToDelete.endPoint == v.Root)
                        endVertexCounter++;
                }

                if (startVertexCounter == 1 && lineToDelete.startPoint != null && lineToDelete.startPoint != CurrentPolygon.Vertices[0].Root)
                {
                    Vertex verToDelete = null;
                    foreach (KeyValuePair<Vertex, List<Vertex>> entry in graph)
                    {
                        if (entry.Key.Root == lineToDelete.startPoint)
                        {
                            verToDelete = entry.Key;
                            break;
                        }
                    }

                    graph.Remove(verToDelete);
                    CurrentPolygon.Vertices.Remove(verToDelete);
                    Destroy(lineToDelete.startPoint);
                }

                if (endVertexCounter == 1 && lineToDelete.endPoint != null && lineToDelete.endPoint != CurrentPolygon.Vertices[0].Root)
                {
                    Vertex verToDelete = null;
                    foreach (KeyValuePair<Vertex, List<Vertex>> entry in graph)
                    {
                        if (entry.Key.Root == lineToDelete.endPoint)
                        {
                            verToDelete = entry.Key;
                            break;
                        }
                    }

                    graph.Remove(verToDelete);
                    CurrentPolygon.Vertices.Remove(verToDelete);
                    Destroy(lineToDelete.endPoint);
                }

                
                if (endVertexCounter != 1 && lineToDelete.endPoint != null)
                {
                    print("removing neighbor");
                    // remove the neighbor verts since you opened the shape up
                    foreach (KeyValuePair<Vertex, List<Vertex>> entry in graph)
                    {
                        if (entry.Key.Root == lineToDelete.startPoint)
                        {
                            Vertex neighbor = entry.Value.Find(v => v.Root == lineToDelete.endPoint);
                            entry.Value.Remove(neighbor);
                            break;
                        }
                    }

                    foreach (KeyValuePair<Vertex, List<Vertex>> entry in graph)
                    {

                        if (entry.Key.Root == lineToDelete.endPoint)
                        {
                            Vertex neighbor = entry.Value.Find(v => v.Root == lineToDelete.startPoint);
                            entry.Value.Remove(neighbor);
                            break;
                        }
                    }

                    CurrentPolygon.Vertices.Remove(lastVertex);
                }

                if (deleteLineRef)
                {
                    Line tempLine = lineToDelete;
                    CurrentPolygon.Lines.Remove(tempLine);
                    Destroy(tempLine.Root);
                }

                if (CurrentPolygon.Lines.Count > 1)
                {
                    lineToDelete = CurrentPolygon.Lines[CurrentPolygon.Lines.Count - 1];
                    deleteLineRef = true;
                    deleteLineRef = DoesLineExist(lineToDelete);
                    if (deleteLineRef)
                    {
                        Line tempLine = lineToDelete;
                        CurrentPolygon.Lines.Remove(tempLine);
                        Destroy(tempLine.Root);
                    }
                }
                //StartCoroutine(DeleteNullLines());
                yield return new WaitForEndOfFrame();
            }
        }
        
        // Undos the line you just created.
        public void UndoLine()
        {
            StartCoroutine(UndoLineWait());
        }

        IEnumerator UndoLineWait()
        {
            yield return new WaitForEndOfFrame();
            if (CurrentPolygon.IsFinished || CurrentPolygon.Vertices.Count == 0)
            {
                RemoveLastPolygon();
            }

            if (CurrentPolygon.Vertices.Count != 0)
            {
                lastVertex = CurrentPolygon.Vertices[CurrentPolygon.Vertices.Count - 1];
                if (lastVertex != null)
                {
                    if (CurrentPolygon.Lines.Count != 0)
                    {
                        StartCoroutine(DeleteLine(CurrentPolygon.Lines[CurrentPolygon.Lines.Count - 1].Root));
                    }
                    else
                    { 
                        CurrentPolygon.Vertices.Remove(lastVertex);

                        bool vertexExists = false;
                        foreach (Polygon p in Polygons)
                        {
                            foreach (Vertex v in p.Vertices)
                            {
                                if (v.Root == lastVertex.Root)
                                {
                                    vertexExists = true;
                                    break;
                                }
                            }

                            if (vertexExists)
                                break;
                        }
                        if (!vertexExists)
                        {
                            graph.Remove(lastVertex);
                            Destroy(lastVertex.Root);
                        }

                    }
                }
            }
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            if (CurrentPolygon.Vertices.Count >= 3)
            {
                if (CurrentPolygon.mesh == null)
                    CreatePolygonMesh();
                else
                    UpdatePolygonMesh();

                ContinueTriangle();
            }
            else
            {

                MeshGenerationManager.Instance.DeleteMesh(CurrentPolygon.mesh);
            }
        }

        // Updated the point position you are currently moving so that it can properly update positions in UpdateLineLength()
        public void UpdatePointPosition(GameObject point)
        {

            foreach (KeyValuePair<Vertex, List<Vertex>> entry in graph)
            {
                if (entry.Key.Root == point)
                {
                    entry.Key.Position = point.transform.position;
                    break;
                }
            }
        }

        public void UpdateAllPointPositions()
        {
            GameObject[] vertices = GameObject.FindGameObjectsWithTag("Vertex");

            foreach (GameObject v in vertices)
            {
                UpdatePointPosition(v);
            }
        }

        // Updates line positions and length when you move points around
        public void UpdateLineLength()
        {

            foreach (Polygon p in Polygons)
            {
                foreach (Line l in p.Lines)
                {
                    if (l.endPoint != null && l.startPoint != null)
                    {
                        var centerPos = (l.endPoint.transform.position + l.startPoint.transform.position) * 0.5f;
                        var direction = l.endPoint.transform.position - l.startPoint.transform.position;
                        var distance = Vector3.Distance(l.endPoint.transform.position, l.startPoint.transform.position);

                        float lineWeight = LineWeight.lineWeight;
                        l.Root.transform.localScale = new Vector3(distance, lineWeight, lineWeight); // default .005f
                        l.Root.transform.position = centerPos;
                        l.Root.transform.rotation = Quaternion.LookRotation(direction);
                        l.Root.transform.Rotate(Vector3.down, 90f);

                        l.Distance = distance;
                    }
                }
            }


            foreach (Line l in CurrentPolygon.Lines)
            {
                if (l.endPoint != null && l.startPoint != null)
                {
                    var centerPos = (l.endPoint.transform.position + l.startPoint.transform.position) * 0.5f;
                    var direction = l.endPoint.transform.position - l.startPoint.transform.position;
                    var distance = Vector3.Distance(l.endPoint.transform.position, l.startPoint.transform.position);

                    float lineWeight = LineWeight.lineWeight;
                    l.Root.transform.localScale = new Vector3(distance, lineWeight, lineWeight); // default .005f
                    l.Root.transform.position = centerPos;
                    l.Root.transform.rotation = Quaternion.LookRotation(direction);
                    l.Root.transform.Rotate(Vector3.down, 90f);

                    l.Distance = distance;
                }
            }

        }

        // Update Polygon mesh if you are moving points around
        public void UpdatePolygonMesh()
        {
            foreach (Polygon p in Polygons)
            {
                List<Vector3> verts = new List<Vector3>();
                foreach (Vertex v in p.Vertices)
                {
                    verts.Add(v.Position);
                }
                MeshGenerationManager.Instance.UpdateMesh(verts, p.mesh);
            }

            // You also want to update the current polygon as well
            List<Vector3> currVerts = new List<Vector3>();
            foreach (Vertex v in CurrentPolygon.Vertices)
            {
                currVerts.Add(v.Position);
            }
            MeshGenerationManager.Instance.UpdateMesh(currVerts, CurrentPolygon.mesh);
        }

        // When a Polygon is closed off, create a mesh for it
        public void CreatePolygonMesh()
        {
            // You also want to update the current polygon as well
            List<Vector3> currVerts = new List<Vector3>();
            foreach (Vertex v in CurrentPolygon.Vertices)
            {
                currVerts.Add(v.Position);
            }
            if (CurrentPolygon.mesh == null)
                CurrentPolygon.mesh = MeshGenerationManager.Instance.SetUpMeshObj(currVerts);
            else
                MeshGenerationManager.Instance.UpdateMesh(currVerts, CurrentPolygon.mesh);
        }

        // Remove the last Polygon reference from the Polygon list
        private bool RemoveLastPolygon()
        {
            if (Polygons != null && Polygons.Count > 0)
            {
                CurrentPolygon = Polygons[Polygons.Count - 1];
                Polygons.RemoveAt(Polygons.Count - 1);
                CurrentPolygon.IsFinished = false;
            }
            return true;
        }

        public List<Polygon> GetPolygons()
        {
            return Polygons;
        }
    }

    public class Polygon
    {
        public float Area { get; set; }

        public List<Vector3> Points { get; set; }

        public List<Vertex> Vertices { get; set; }

        public List<Line> Lines { get; set; }

        public GameObject Root { get; set; }

        public bool IsFinished { get; set; }

        public GameObject mesh { get; set; }
    }
}