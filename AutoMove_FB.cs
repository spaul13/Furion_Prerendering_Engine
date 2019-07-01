using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;


public class AutoMove_FB : MonoBehaviour {

    public KeyCode traverseKey = KeyCode.T;
    public float delta = 0.03125f;  // This is a precise float number
    public Vector3 startPoint = new Vector3(-10.00f, 0f, -10.00f);
    public Vector3 endPoint = new Vector3(10.00f, 0f, 10.00f);
    public Vector3 currentPoint;
    public Vector3 startRotationEuler = new Vector3(0f, 0f, 0f);
    public Vector3 endRotationEuler = new Vector3(0f, 90f, 0f);
    public Vector3 currentRotationEuler;
    public Quaternion currentRotation;
    public Quaternion startRotation;
    public Quaternion endRotation;
    private Rigidbody m_Rigidbody;
    int loopNum;




    double[] radius_array;
    string parentObjName = "Content";
    string terrainName = "Terrain/terrain_02/terrain_near_01";
    string radius_file = "E:/cut_datas/begin_exp/new_radius_viking/0605/logexp_viking_amend_newradius_16_new.txt";  
    string camUnityPath = "ProbeCamera/MainCamera";
    float cut = 16f;
    float boundHeight = 100f;

    Camera cam;
    Vector3 terrainCenter;
    Vector3 terrainMax;
    Vector3 terrainMin;
    Vector3 cutUnit;
    bool capturepanorama;

    Bounds[] subBounds;
    List<Collider> colliders = new List<Collider>();
    List<Vector3> boundmax = new List<Vector3>();
    List<Vector3> boundmin = new List<Vector3>();

    //for the zigzag motion
    bool forward;
    Vector3 endingPoint; //different endPoint for different way of movement
	string fp = "";


    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        startRotation.eulerAngles = startRotationEuler;
        endRotation.eulerAngles = endRotationEuler;
        cam = GameObject.Find(camUnityPath).GetComponent<Camera>();
        loopNum = 0;
        capturepanorama = true;
        forward = true;
		fp = "E:/test_split_fps/viking/";

        FindTerrainDim();
        Debug.Log(terrainMax + ", " + terrainMin);
        cutUnit = new Vector3((terrainMax.x - terrainMin.x) / cut, (terrainMax.y - terrainMin.y) / cut, (terrainMax.z - terrainMin.z) / cut);
        subBounds = new Bounds[(int)(cut * cut)];
        for (int zIndex = 0; zIndex < cut; zIndex++)
        {
            for (int xIndex = 0; xIndex < cut; xIndex++)
            {
                Vector3 subBoundsMin = new Vector3(terrainMin.x + (float)xIndex * cutUnit.x, terrainMax.y, terrainMin.z + (float)zIndex * cutUnit.z);
                Vector3 subBoundsMax = new Vector3(terrainMin.x + (float)(xIndex + 1) * cutUnit.x, terrainMax.y, terrainMin.z + (float)(zIndex + 1) * cutUnit.z);
                subBounds[(int)(zIndex * cut + xIndex)] = new Bounds(new Vector3((subBoundsMin.x + subBoundsMax.x) / 2f, (subBoundsMin.y + subBoundsMax.y) / 2f, (subBoundsMin.z + subBoundsMax.z) / 2f)
                                                                   , new Vector3(subBoundsMax.x - subBoundsMin.x, subBoundsMax.y - subBoundsMin.y + boundHeight, subBoundsMax.z - subBoundsMin.z));
                
                /*boundmax.Add(subBoundsMax);
                boundmin.Add(subBoundsMin);*/
            }
        }
    }

    private void OnEnable()
    {
        // When the object is turned on, make sure it's not kinematic.
        m_Rigidbody.isKinematic = false;

        m_Rigidbody.MovePosition(startPoint);
    }
    // Use this for initialization
    void Start ()
    {
        GetRadiusFromFile();
    }

    // Update is called once per frame

    // Update is called once per frame
    void Update()
    {
            //StartCoroutine(change_radius());
            if(capturepanorama) StartCoroutine(TraverseRect());
    }


    //this function is used to move around through entire region render only Far BG on a zigzag motion
    IEnumerator TraverseRect()
    {
        //Debug.Log("Start position is " + m_Rigidbody.position);
        currentPoint = m_Rigidbody.position;
        int x, y, z;
        string filenameBase;
        if (!forward) endingPoint = new Vector3(endPoint.x, endPoint.y, startPoint.z);
        else endingPoint = new Vector3(endPoint.x, endPoint.y, endPoint.z);

        if (!currentPoint.Equals(endingPoint))
        {
            x = Convert.ToInt32(Math.Floor(currentPoint.x));
            y = Convert.ToInt32(Math.Floor(currentPoint.y));
            z = Convert.ToInt32(Math.Floor(currentPoint.z));
            //string filenameBase = String.Format("{0}_{1:D3}_{2:D3}_{3:D3}_{4:D3}", "sunlh", x, y, z, loopNum % 32);
            filenameBase = String.Format("{0}_{1:D3}", "sj", loopNum);
            Debug.Log("\n the loopNum = " +loopNum);
		    change_radius();
            Debug.Log("\n from the co-routine the near Plane of camera = " + cam.nearClipPlane);
            GameObject.Find("Capture Panorama").GetComponent<CapturePanorama.CapturePanorama>().CaptureScreenshotSync(filenameBase);
			//File.AppendAllText(fp + "logexp_zigzag_panoramicindexing_0831.txt", "[ position = (" + m_Rigidbody.position.x + "," + m_Rigidbody.position.y +  "," + m_Rigidbody.position.z + ") ,index =" + loopNum + ", radius =" +cam.nearClipPlane +" ]\n");
            //Debug.Log(currentPoint + "," + forward + "," + loopNum + "," + startPoint + "," + endPoint);
            if ((currentPoint.z == endPoint.z) && (forward))
            {
                Debug.Log("The camera has reached the upper z boundary of the selected area!");
                //currentPoint.z = startPoint.z;
                forward = false;
                MoveAlongX();
            }
            else if ((currentPoint.z == startPoint.z) && (loopNum != 0) && (!forward))
            {
                Debug.Log("The camera has reached the lower z boundary of the selected area!");
                forward = true;
                MoveAlongX();
            }
            else
            {
                MoveAlongZ();
            }
            loopNum++;


            yield return new WaitForSeconds(1f);  // stop and wait for 1 second
        }
        else
        {
            filenameBase = String.Format("{0}_{1:D3}", "sj", loopNum++);
            //GameObject.Find("Capture Panorama").GetComponent<CapturePanorama.CapturePanorama>().CaptureScreenshotSync(filenameBase);
            Debug.Log("The camera has gone through the selected area");
            capturepanorama = false;
        }
    }

    //we can make this function as the co-routine called before the TraverseRect()

    void change_radius()
    {
        int zIndex = 0;
        int xIndex = 0;
        bool isInCut = false;
        Vector3 pos = m_Rigidbody.position; //new Vector3((float)location_array[index], (float)location_array[index + 1], (float)location_array[index + 2]);
        //Debug.Log("\n m_Rigidbody position " + m_Rigidbody.position);
        Debug.Log("\n the pos = " + pos); 


        for (zIndex = 0; zIndex < cut; zIndex++)
        {
            for (xIndex = 0; xIndex < cut; xIndex++)
            {
                /*Rect rect = new Rect(boundmin[(int)(zIndex * cut + xIndex)].x, boundmin[(int)(zIndex * cut + xIndex)].z, boundmax[(int)(zIndex * cut + xIndex)].x, boundmin[(int)(zIndex * cut + xIndex)].z);
                if (rect.Contains(pos))
                {
                    isInCut = true;
                    break;
                }*/
                /*Debug.Log(zIndex + ", " + xIndex);
                Debug.Log(subBounds[(int)(zIndex * cut + xIndex)]);*/

                if (subBounds[(int)(zIndex * cut + xIndex)].Contains(pos))
                {
                    isInCut = true;
                    break;
                }
            }
            if (isInCut) break;
        }
        if (!isInCut)
        {
            Debug.Log("Error for pos: " + pos + " with no fitting cuts!");
        }
        else
        {
            //Debug.Log("radius_array: len " + radius_array.Length + " , " + (zIndex * cut * 5 + xIndex * 5 + 3) + ", " + zIndex + ", " + xIndex);
            cam.nearClipPlane = (float)radius_array[(int)(zIndex * cut * 5 + xIndex * 5 + 3)];
            cam.farClipPlane = 1000f;
        }
        //yield return null;
    }


    // for soccer and viking_village game with only one terrain
    void FindTerrainDim()
    {
        GameObject terrainObj = GameObject.Find(parentObjName + "/" + terrainName);
        Component terrainMesh = terrainObj.GetComponent<MeshRenderer>();
        Bounds terrainBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        if (terrainMesh == null) // Haven't been tested yet
        {
            Component terrainT = terrainObj.GetComponent<Terrain>();
            if (terrainT == null) Debug.Log("FindRadius: terrain " + terrainName + " is neither type of Mesh nor type of Terrain!");
            else
            {
                terrainBounds = ((Terrain)terrainT).terrainData.bounds; // in the local space
                terrainBounds = new Bounds(terrainBounds.center + terrainObj.transform.position, terrainBounds.size);
            }
        }
        else terrainBounds = ((MeshRenderer)terrainMesh).bounds;

        if (terrainBounds.size.x == 0 && terrainBounds.size.y == 0 && terrainBounds.size.z == 0)
            Debug.Log("FindRadius: terrain " + terrainName + " has no bounding box!");
        else
        {
            terrainCenter = terrainBounds.center;
            terrainMax = terrainBounds.max;
            terrainMin = terrainBounds.min;
        }

        /* Only needed for pick random location
         * Component terrainMeshCollider = terrainObj.GetComponent<MeshCollider>();
        if (terrainMeshCollider == null) terrainObj.AddComponent<MeshCollider>();

        colliders.Add((Collider)terrainMeshCollider);*/
    }

    void MoveAlongZ()
    {
        if(forward) currentPoint.z += delta;
        else currentPoint.z -= delta;
        Debug.Log(forward);
        m_Rigidbody.MovePosition(currentPoint);
        Debug.Log("MoveAlongZ: Current position is " + m_Rigidbody.position);
    }

    void MoveAlongX()
    {
        currentPoint.x += delta;
        m_Rigidbody.MovePosition(currentPoint);
        Debug.Log("MoveAlongX: Current position is " + m_Rigidbody.position);
    }

    void GetRadiusFromFile()
    {
        StreamReader radius_reader = new System.IO.StreamReader(radius_file);
        var list = new List<double>();
        string[] ar_radius;
        string line;
        while ((line = radius_reader.ReadLine()) != null)
        {
            ar_radius = line.Split(',');
            for (int i = 0; i < ar_radius.Length; i++)
            {

                double tmp = 0f;
                double.TryParse(ar_radius[i], out tmp);
                list.Add(tmp);
            }
        }
        radius_array = list.ToArray();
    }

}
