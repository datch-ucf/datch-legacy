//// Copyright (c) Microsoft Corporation. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

//using UnityEngine;
//using System.Collections;
//using HoloToolkit.Unity;
//using System.Collections.Generic;
//using System;
//using HoloToolkit.Unity.InputModule;
//using UnityEngine.Windows.Speech;
//using System.Linq;

//namespace HoloToolkit.Examples.GazeRuler
//{
//    /// <summary>
//    /// manager all measure tools here
//    /// </summary>
//    public class MeasureManager1 : Singleton<MeasureManager1>, IHoldHandler, IInputClickHandler, IInputHandler, ISourceStateHandler
//    {
//        private IGeometry manager;
//        public GeometryMode Mode;

//        // set up prefabs
//        public GameObject LinePrefab;
//        public GameObject PointPrefab;
//        public GameObject ModeTipObject;
//        public GameObject TextPrefab;

//        public Color defaultColor;


//        KeywordRecognizer keywordRecognizer;

//        // Defines which function to call when a keyword is recognized
//        delegate void KeywordAction(PhraseRecognizedEventArgs args);
//        Dictionary<string, KeywordAction> keywordCollection = new Dictionary<string, KeywordAction>();

//        private bool edit = false;
//        private bool drawing = false;

//        private void Start()
//        {
//            InputManager.Instance.PushFallbackInputHandler(gameObject);

//            // inti measure mode
//            switch (Mode)
//            {
//                case GeometryMode.Polygon:
//                    manager = PolygonManager.Instance;
//                    break;
//                default:
//                    manager = LineManager.Instance;
//                    break;
//            }

//            // Sets the default color of your lines. Doing this because once you exit the app, it keeps the last color you chose
//            Renderer lineRend = LinePrefab.GetComponent<Renderer>();
//            Renderer pointRend = PointPrefab.GetComponent<Renderer>();

//            lineRend.sharedMaterial.SetColor("_Color", defaultColor);
//            pointRend.sharedMaterial.SetColor("_Color", defaultColor);

//            keywordCollection.Add("Edit", Edit);
//            keywordCollection.Add("Draw", Draw);
//            keywordCollection.Add("Clear All", ClearAllShapes); // clears all finisehd shaped in the stack
//            keywordCollection.Add("Reset Shape", ResetShape);
//            keywordCollection.Add("Close Shape", OnPolygonClose);

//            keywordRecognizer = new KeywordRecognizer(keywordCollection.Keys.ToArray());
//            keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
//            keywordRecognizer.Start();
//        }

//        void Update()
//        {
//            // Debugging
//            //if (Input.GetKeyDown(KeyCode.J))
//            //{
//            //    OnSelect();
//            //    print("adding point");
//            //}

//            //// deletes the last shape in the stack
//            //if (Input.GetKeyDown(KeyCode.K))
//            //{
//            //    DeleteLine();
//            //}

//            //// cleares all polygons and lines
//            //if (Input.GetKeyDown(KeyCode.L))
//            //{
//            //    ClearAll();
//            //}

//            if (Input.GetKeyDown(KeyCode.O))
//            {
//                OnPolygonClose();
//            }

//            ////deletes the last unfinished shape you were drawing
//            //if (Input.GetKeyDown(KeyCode.R))
//            //{
//            //    ResetShape();
//            //}

//            //if (Input.GetKeyDown(KeyCode.U))
//            //{
//            //    Undo();
//            //}

//            //if (drawing && !edit)
//            //{
//            //    OnSelect();
//            //    print("paint");
//            //}
//            //print(drawing);

//        }

//        // place spatial point
//        public void OnSelect()
//        {
//            if (!edit)
//            {
//                manager.AddPoint(LinePrefab, PointPrefab, TextPrefab);
//            }
//            else
//            {
//                Undo();
//            }
//        }

//        // delete latest line or geometry which is the whole shape
//        public void DeleteLine()
//        {
//            manager.Delete();
//        }

//        // delete all lines or geometry
//        public void ClearAll()
//        {
//            manager.Clear();
//        }

//        // starts the shape draw from the start
//        public void ResetShape()
//        {
//            manager.Reset();
//        }

//        public void Undo()
//        {
//            manager.UndoLine();
//        }

//        // if current mode is geometry mode, try to finish geometry
//        public void OnPolygonClose()
//        {
//            IPolygonClosable client = PolygonManager.Instance;
//            client.ClosePolygon(LinePrefab, TextPrefab);
//        }

//        // change measure mode
//        public void OnModeChange()
//        {
//            try
//            {
//                manager.Reset();
//                if (Mode == GeometryMode.Line)
//                {
//                    Mode = GeometryMode.Polygon;
//                    manager = PolygonManager.Instance;
//                }
//                else
//                {
//                    Mode = GeometryMode.Line;
//                    manager = LineManager.Instance;
//                }
//            }
//            catch (Exception ex)
//            {
//                Debug.Log(ex.Message);
//            }
//            ModeTipObject.SetActive(true);
//        }

//        public void OnHoldStarted(HoldEventData eventData)
//        {
//            if (!edit)
//            {
//                //drawing = true;
//            }
//            else
//            {
//                DeleteLine();
//            }
//            //DeleteLine();
//        }

//        public void OnHoldCompleted(HoldEventData eventData)
//        {
//            // Nothing to do

//        }

//        public void OnHoldCanceled(HoldEventData eventData)
//        {
//            // Nothing to do

//        }

//        public void OnInputClicked(InputClickedEventData eventData)
//        {
//            //FocusDetails? focusDetails = FocusManager.Instance.TryGetFocusDetails(eventData);
//            //if(focusDetails != null)
//            //{

//            //}
//            OnSelect();
//            //eventData.Use(); // Mark the event as used, so it doesn't fall through to other handlers.
//        }

//        public void OnInputUp(InputEventData eventData)
//        {
//            drawing = false;
//            print("UP");
//        }

//        public void OnInputDown(InputEventData eventData)
//        {
//            drawing = true;
//            print("DOWN");
//        }

//        private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
//        {
//            KeywordAction keywordAction;

//            if (keywordCollection.TryGetValue(args.text, out keywordAction))
//            {
//                keywordAction.Invoke(args);
//            }
//        }

//        private void Edit(PhraseRecognizedEventArgs args)
//        {
//            edit = true;
//        }

//        private void Draw(PhraseRecognizedEventArgs args)
//        {
//            edit = false;
//        }

//        private void ClearAllShapes(PhraseRecognizedEventArgs args)
//        {
//            ClearAll();
//        }

//        private void ResetShape(PhraseRecognizedEventArgs args)
//        {
//            manager.Reset();
//        }

//        private void OnPolygonClose(PhraseRecognizedEventArgs args)
//        {
//            OnPolygonClose();
//        }

//        public void OnSourceDetected(SourceStateEventData eventData)
//        {
//            // Nothing to do
//        }

//        public void OnSourceLost(SourceStateEventData eventData)
//        {
//            drawing = false;
//        }

//        public GameObject GetPointPrefab()
//        {
//            return PointPrefab;
//        }

//        public GameObject GetLinePrefab()
//        {
//            return LinePrefab;
//        }
//    }

//    public class Point
//    {
//        public Vector3 Position { get; set; }

//        public GameObject Root { get; set; }
//        public bool IsStart { get; set; }
//    }


//    public enum GeometryMode
//    {
//        Line,
//        Triangle,
//        Rectangle,
//        Cube,
//        Polygon
//    }
//}
