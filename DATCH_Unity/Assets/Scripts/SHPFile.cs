using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;
using System;
using UnityEngine.UI;


public class SHPFile : MonoBehaviour
{
    public TextMesh worldText;
    public TextMesh parentPathText;
    private string persistentDataPath; // persistentDataPath is where you can read and write to on hololens

    [DllImport("shp")]
    public static extern float DisplayHelloFromDLL();
    #region DLL Imports

    [DllImport("shp", EntryPoint = "SHPOpen")]
    public static extern IntPtr SHPOpen([MarshalAs(UnmanagedType.LPStr)] string pszLayer, [MarshalAs(UnmanagedType.LPStr)] string pszAccess);

    [DllImport("shp")]
    public static extern void SHPGetInfo(IntPtr hSHP, IntPtr pnEntities, IntPtr pnShapeType,
                      IntPtr padfMinBound, IntPtr padfMaxBound); // pad are double pointers

    [DllImport("shp")]
    public static extern IntPtr SHPTypeName(int nSHPType);

    [DllImport("shp")]
    public static extern IntPtr SHPReadObject(IntPtr psSHP, int hEntity);

    [DllImport("shp")]
    public static extern void SHPDestroyObject(IntPtr psShape);


    [DllImport("shp")]
    public static extern void SHPClose(IntPtr hSHP);


    [DllImport("shp")]
    public static extern IntPtr SHPCreate([MarshalAs(UnmanagedType.LPStr)] string pszShapeFile, int nShapeType);


    [DllImport("shp")]
    public static extern IntPtr SHPCreateObject(int nSHPType, int nShapeId, int nParts,
                                                   IntPtr panPartStart, IntPtr panPartType,
                                                   int nVertices,
                                                   IntPtr padfX, IntPtr padfY,
                                                   IntPtr padfZ, IntPtr padfM);

    [DllImport("shp")]
    public static extern int SHPWriteObject(IntPtr hSHP, int iShape, IntPtr psObject);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] //C# structure
    public struct tagSHPObject
    {
        public int nSHPType;

        public int nShapeId;  /* -1 is unknown/unassigned */

        public int nParts;
        public IntPtr panPartStart;
        public IntPtr panPartType;

        public int nVertices;
        public IntPtr padfX;
        public IntPtr padfY;
        public IntPtr padfZ;
        public IntPtr padfM;

        public double dfXMin;
        public double dfYMin;
        public double dfZMin;
        public double dfMMin;

        public double dfXMax;
        public double dfYMax;
        public double dfZMax;
        public double dfMMax;

        public int bMeasureIsUsed;
        public int bFastModeReadObject;
    }

    // DBF functions
    [DllImport("shp")]
    public static extern IntPtr DBFCreate([MarshalAs(UnmanagedType.LPStr)] string pszFilename);

    [DllImport("shp", EntryPoint = "DBFOpen")]
    public static extern IntPtr DBFOpen([MarshalAs(UnmanagedType.LPStr)] string pszFilename, [MarshalAs(UnmanagedType.LPStr)] string pszAccess);

    [DllImport("shp")]
    public static extern void DBFClose(IntPtr psDBF);

    [DllImport("shp")]
    public static extern int DBFAddField(IntPtr psDBF, [MarshalAs(UnmanagedType.LPStr)] string pszFilename, DBFFieldType eType, int nWidth, int nDecimals);

    [DllImport("shp")]
    public static extern int DBFGetFieldCount(IntPtr psDBF);

    [DllImport("shp")]
    public static extern int DBFGetRecordCount(IntPtr psDBF);

    [DllImport("shp")]
    public static extern int DBFWriteNULLAttribute(IntPtr psDBF, int iRecord, int iField);

    [DllImport("shp")]
    public static extern int DBFWriteIntegerAttribute(IntPtr psDBF, int iRecord, int iField, int nValue);

    [DllImport("shp")]
    public static extern DBFFieldType DBFGetFieldInfo(IntPtr psDBF, int iField, [MarshalAs(UnmanagedType.LPStr)] string pszFieldName,
                                                            IntPtr pnWidth, IntPtr pnDecimals);

    [DllImport("shp")]
    public static extern int DBFWriteStringAttribute(IntPtr psDBF, int iRecord, int iField, [MarshalAs(UnmanagedType.LPStr)] string pszValue);

    [DllImport("shp")]
    public static extern int DBFWriteDoubleAttribute(IntPtr psDBF, int iRecord, int iField, double dValue);



    public enum DBFFieldType
    {
        FTString,
        FTInteger,
        FTDouble,
        FTLogical,
        FTInvalid
    }


    #endregion

    [DllImport("msvcrt.dll", SetLastError = false)]
    static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);

    public static SHPFile Instance;
    // Use this for initialization
    void Start()
    {
        if (Instance == null)
        {
            Instance = this.GetComponent<SHPFile>();
        }
        else
        {
            Destroy(this);
        }

        persistentDataPath = Application.persistentDataPath;

    }

    // Look through Application.persistentDataPath and search for all shp file types
    public List<string> GetFilenames()
    {
        List<String> fileNames = new List<string>();

        DirectoryInfo assetsPath = new DirectoryInfo(persistentDataPath);
        FileInfo[] assetsFileInfo = assetsPath.GetFiles();

        foreach (FileInfo file in assetsFileInfo)
        {
            if (file.Extension == ".shp")
            {
                fileNames.Add(file.Name.Remove(file.Name.Length - 4)); // remove .shp string from end
            }
        }

        return fileNames;
    }


    // Delete both shx and shp files 
    public void DeleteFileName(string fileName)
    {
        if (FileExists(fileName))
        {
            File.Delete(persistentDataPath + "/" + fileName + ".shx");
            File.Delete(persistentDataPath + "/" + fileName + ".shp");
        }
        else
        {
            Debug.Log("File not found");
        }
    }

    public bool FileExists(string fileName)
    {
        List<String> fileNames = new List<string>(GetFilenames());
        if (fileNames != null)
        {
            return fileNames.Contains(fileName);
        }
        else
        {
            return false;
        }
    }


    

    public unsafe void CreateDBF_File(string fileName, List<GameObject> meshes = null)
    {
        // Create your dbf file
        IntPtr hDBFCreate;
        hDBFCreate = DBFCreate(persistentDataPath + "/" + fileName);


        if (hDBFCreate == null)
        {
            print("Failed to create dbf");
            return;
        }

        print(DBFAddField(hDBFCreate, ("Id"), DBFFieldType.FTInteger, 1, 0));

        //int fieldCount = 0;
        //foreach(GameObject m in meshes)
        //{

        //    print(DBFAddField(hDBFCreate, ("M"), DBFFieldType.FTInteger, 1, 0));
        //    fieldCount++;
        //}
        // Idk what this is doing
        // DBFAddField(hDBFCreate, persistentDataPath + "/" + fileName, DBFFieldType.FTString, 0, 0);
        // DBFAddField(hDBFCreate, persistentDataPath + "/" + fileName, DBFFieldType.FTDouble, 0, 0);
        print("Field coutn " + DBFGetFieldCount(hDBFCreate));
        DBFClose(hDBFCreate);

        // add stuff to dbf file
        IntPtr hDBFAdd;
        int i = 0, iRecord = 0;
        hDBFAdd = DBFOpen(persistentDataPath + "/" + fileName, "r+b");


        if (hDBFAdd == null)
        {
            print("DBFOpen failed.");
            return;
        }
        

        //iRecord = DBFGetRecordCount(hDBFAdd);
        print(iRecord);
        print(DBFGetFieldCount(hDBFAdd));
        //for(i = 0; i < DBFGetFieldCount(hDBFAdd); i++)
        //{
        //    DBFWriteIntegerAttribute(hDBFAdd, iRecord, i, 0);
        //}    


        print(DBFGetRecordCount(hDBFAdd));
        for (i = 0; i < DBFGetFieldCount(hDBFAdd); i++)
        {
            //DBFWriteIntegerAttribute(hDBFAdd, iRecord, i, 0);
            foreach (GameObject m in meshes)
            {
                print("here");
                DBFWriteIntegerAttribute(hDBFAdd, iRecord, i, 0);
                iRecord++;

            }
        }
        
        print(DBFGetRecordCount(hDBFAdd));
        DBFClose(hDBFAdd);
    }

    public unsafe void ReadDBF_File(string fileName)
    {
        IntPtr hDBF;
        IntPtr panWidth;
        int  i = 0, iRecord = 0;
        char[] szFormat = new char[32];
        string pszFilename = null; // make sure this is right
        int nWidth = 0, nDecimals = 0;
        int bHeader = 0;
        int bRaw = 0;
        int bMultiLine = 0;
        char[] szTitle = new char[12];


        hDBF = DBFOpen(persistentDataPath + "/" + fileName, "r+b");

        panWidth = Marshal.AllocHGlobal(sizeof(int) * DBFGetFieldCount(hDBF));

        if (hDBF == null)
        {
            print("DBFOpen failed.");
            return;
        }

        if (DBFGetFieldCount(hDBF) == 0)
        {
            print("There are no fields in this table!\n");
            
        }

        Marshal.FreeHGlobal(panWidth);
    }


    // Set it up for use with the meshes
    public unsafe bool CreateShpFile(string fileName, List<GameObject> meshes = null)
    {
        #region Creating Shapefile file
        unsafe
        {
            IntPtr hSHPCreate;
            int nShapeTypeCreate = 15; // 15 would be POLYGONZ. You probably want to identify what type you want in later interations
            hSHPCreate = SHPCreate(persistentDataPath + "/" + fileName, nShapeTypeCreate);

            SHPClose(hSHPCreate);

        }
        #endregion


        #region Adding vertices to shapefile        

        // For each polygon you will loop through its verts and add them to xArr, yArr, and zArr.
        // then create a shape and add it to the hSHPadd variable
        // Each mesh represents a polygon       
        foreach (GameObject m in meshes)
        {
            IntPtr hSHPAdd = IntPtr.Zero;
            int nShapeTypeAdd = 0, nVerticesAdd = 0, nPartsAdd = 0, iAdd = 0, nVMaxAdd = 0;

            IntPtr panPartsAdd; // *panPartsAdd,
            int[] panPartsAddArr = new int[8];

            panPartsAdd = Marshal.AllocHGlobal(sizeof(int) * panPartsAddArr.Length);

            IntPtr padfX, padfY, padfZ, padfM = IntPtr.Zero; // padfM is not being used but need to be initialized                                                          

            List<double> xArr = new List<double>(),
                         yArr = new List<double>(),
                         zArr = new List<double>();

            Mesh mesh = m.GetComponent<MeshFilter>().mesh;
            //print("Mesh legth " +mesh.vertices.Length);
            foreach (Vector3 vert in mesh.vertices)
            {
                xArr.Add(vert.x);
                yArr.Add(vert.y);
                zArr.Add(vert.z);
            }

            padfX = Marshal.AllocHGlobal(sizeof(double) * xArr.Count);
            padfY = Marshal.AllocHGlobal(sizeof(double) * yArr.Count);
            padfZ = Marshal.AllocHGlobal(sizeof(double) * zArr.Count);
            //padfM = Marshal.AllocHGlobal(sizeof(double) * mArr.Length);

            IntPtr psObject = IntPtr.Zero; // this will be the individual polygon object
            string tuple = ""; // const char* tuple = "";
            string filename = ""; // const char* filename;


            //IntPtr testEntitiesAdd = Marshal.AllocHGlobal(Marshal.SizeOf(nEntities));
            IntPtr testShapeTypeAdd = Marshal.AllocHGlobal(Marshal.SizeOf(nShapeTypeAdd));
            //IntPtr testMinBoundAdd = Marshal.AllocCoTaskMem(sizeof(double) * adfMinBound.Length);
            //IntPtr testMaxBoundAdd = Marshal.AllocCoTaskMem(sizeof(double) * adfMaxBound.Length);

            hSHPAdd = SHPOpen(persistentDataPath + "/" + fileName, "r+b");

            SHPGetInfo(hSHPAdd, IntPtr.Zero, testShapeTypeAdd, IntPtr.Zero, IntPtr.Zero);

            nShapeTypeAdd = (int)Marshal.PtrToStructure(testShapeTypeAdd, typeof(int));

            nPartsAdd = 1;
            panPartsAddArr[0] = 0;

            Marshal.Copy(panPartsAddArr, 0, panPartsAdd, 8);
            Marshal.Copy(xArr.ToArray(), 0, padfX, xArr.Count);
            Marshal.Copy(yArr.ToArray(), 0, padfY, yArr.Count);
            Marshal.Copy(zArr.ToArray(), 0, padfZ, zArr.Count);
            //Marshal.Copy(mArr, 0, padfM, 1);

            //print("Vertex Count " + mesh.vertexCount);
            //print("Vertex Length " + mesh.vertices.Length);
            nVerticesAdd = mesh.vertexCount;

            // Keep creating and adding to the main shapefile
            psObject = SHPCreateObject(nShapeTypeAdd, -1, nPartsAdd, panPartsAdd, IntPtr.Zero, nVerticesAdd, padfX, padfY, padfZ, padfM);

            SHPWriteObject(hSHPAdd, -1, psObject);
            SHPDestroyObject(psObject);


            SHPClose(hSHPAdd);

            Marshal.FreeHGlobal(panPartsAdd);
            Marshal.FreeHGlobal(padfX);
            Marshal.FreeHGlobal(padfY);
            Marshal.FreeHGlobal(padfZ);
            Marshal.FreeHGlobal(padfM);
        }
        #endregion

        return true;
    }

    // This function reads in the shapefile
    // TODO: Should we get the exact position or can it stay rounded
    // Make sure that files are of POLYGONZ type, probably others but this one I know is the one we need for now
    public unsafe List<List<Vector3>> GetPolygons(string fileName)
    {
        // This will hold the polygons that you read from file
        List<List<Vector3>> polygons = new List<List<Vector3>>();

        unsafe
        {
            // Try to read the file you just created
            #region READING FROM THE FILE YOU JUST CREATED
            IntPtr hSHP;
            int nShapeType = 0, nEntities = 0, i = 0, iPart = 0, bValidate = 0, nInvalidCount = 0;
            int bHeaderOnly = 0;
            string pszPlus;
            double[] adfMinBound = new double[4], adfMaxBound = new double[4];
            int nPrecision = 15;


            hSHP = SHPOpen(persistentDataPath + "/" + fileName, "r+b");
            if (hSHP != null)
            {
                //print("it not null");

                IntPtr testEntities = Marshal.AllocHGlobal(sizeof(int));
                IntPtr testShapeType = Marshal.AllocHGlobal(sizeof(int));
                IntPtr testMinBound = Marshal.AllocHGlobal(sizeof(double) * adfMinBound.Length);
                IntPtr testMaxBound = Marshal.AllocHGlobal(sizeof(double) * adfMaxBound.Length);

                SHPGetInfo(hSHP, testEntities, testShapeType, testMinBound, testMaxBound);

#if UNITY_EDITOR
                nShapeType = (Int32)testShapeType;
                nEntities = (Int32)testEntities;
                nShapeType = (int)Marshal.PtrToStructure(testShapeType, typeof(int));
                nEntities = (int)Marshal.PtrToStructure(testEntities, typeof(int));
#endif

                // UWP Specific
#if NETFX_CORE
                    
                unsafe
                {                
                    //print("Shapefile type ptr to int structure: " + (int)Marshal.PtrToStructure(testShapeType, typeof(int))); // This works
                    //int* res = (int*)testShapeType;// this works too
                    //print("Printing res: " + *res);
                    //print("Shapefile type read int32: " + Marshal.ReadInt32(testShapeType));// this works
                    
                    nShapeType = (int)Marshal.PtrToStructure(testShapeType, typeof(int));
                    nEntities = (int)Marshal.PtrToStructure(testEntities, typeof(int));
                }
#endif
                Marshal.Copy(testMinBound, adfMinBound, 0, 4);
                Marshal.Copy(testMaxBound, adfMaxBound, 0, 4);

                worldText.text = nShapeType.ToString();

                // do not edit
                for (i = 0; i < nEntities; i++)
                {
                    int j;
                    IntPtr psShape;

                    psShape = SHPReadObject(hSHP, i);

                    if (psShape == null)
                    {
                        print("Unable to read shape " + i + ", terminating object reading.\n");
                        break;
                    }


                    tagSHPObject shpObject = new tagSHPObject();
                    // probably edit
                    shpObject = (tagSHPObject)Marshal.PtrToStructure(psShape, typeof(tagSHPObject));

                    if (shpObject.bMeasureIsUsed == 0)
                    {
                        //print("Shape: " + i);
                        //print("Vertices " + shpObject.nVertices);
                        //print("NParts: " + shpObject.nParts);
                    }

                    if (shpObject.nVertices != 0 && shpObject.bMeasureIsUsed == 0)
                    {

                        int arrSize = shpObject.nVertices;
                        double[] padfx = new double[arrSize];
                        Marshal.Copy(shpObject.padfX, padfx, 0, arrSize);

                        double[] padfy = new double[arrSize];
                        Marshal.Copy(shpObject.padfY, padfy, 0, arrSize);

                        double[] padfz = new double[arrSize];
                        Marshal.Copy(shpObject.padfZ, padfz, 0, arrSize);

                        // Will hold the temp verices for each polygon to add to the polygons list
                        List<Vector3> vertices = new List<Vector3>();

                        for (j = 0, iPart = 1; j < shpObject.nVertices; j++)
                        {
                            if (shpObject.bMeasureIsUsed == 0)
                            {
                                //print("(" + padfx[j] + "," + padfy[j] + "," + padfz[j] + ")");

                                Vector3 tempVert;
                                tempVert.x = (float)padfx[j]; // x
                                tempVert.y = (float)padfy[j]; // y
                                tempVert.z = (float)padfz[j]; // z

                                vertices.Add(tempVert);

                                //print("VERT X :" + tempVert.x);
                            }
                        }

                        if (vertices.Count != 0)
                            polygons.Add(vertices);
                    }
                    else
                    {
                        print("Shape has NO Vertices \n\n");
                    }

                    SHPDestroyObject(psShape);
                }

                SHPClose(hSHP);
                #endregion

            }
            else
            {
                print("File not found");
            }
        }

        return polygons;
    }

}
