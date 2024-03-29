﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;


//change the coroutine runningl() to running() in order to change from fixed radius to the adaptive radius and change the radius_intend to change the fixed radius value

public class GenPanorama : MonoBehaviour
{
    string parentObjName = "Content";
    string terrainName = "Terrain/terrain_01/terrain_near_01";
    string radius_file = "H:/cut_datas/begin_exp/new_radius_viking/0605/logexp_viking_amend_newradius_16_new.txt"; // 
    string location_file = "H:/test_split_soccer/log_1216_playerloc_1.txt";//"H:/test_split_soccer/location_viking_103019_16.txt"; //"H:/CubeMaps/human_study_locations/hs_viking_1.txt";//"C:/Users/spaul/Downloads/filter_exp13_gafps_133458.txt"; //"C:/Users/spaul/Downloads/filter_exp14_gafps_175082.txt"; //"E:/cut_datas/new_exp/location_cut_split/loc_remov_zero/viking_loc_12_locpat_nonzero.txt"; // 
    string camUnityPath = "ProbeCamera/MainCamera";
    float cut = 16f;
    float boundHeight = 100f;
    int output_flag = 1;
    float radius_intend = 8f;

    double[] radius_array;
    double[] location_array;
    int index;
    bool captureframe;
    Rigidbody m_Rigidbody;


    Vector3 terrainCenter;
    Vector3 terrainMax;
    Vector3 terrainMin;
    Vector3 cutUnit;

    Bounds[] subBounds;

    Camera cam;
    public KeyCode traverseKey = KeyCode.T;
    int checkpointer;

    Camera cam1;
    GameObject camGos;
    RenderTexture frameRenderTexture = null;
    Texture2D tex;
    int cameraWidth = 1920;
    int cameraHeight = 1080;
    uint[] cameraPixels = null;

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        cam = GameObject.Find(camUnityPath).GetComponent<Camera>();

        FindTerrainDim();
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
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        GetRadiusFromFile();
        ReadFile();

        index = 1;
        captureframe = true;


        camGos = new GameObject("ManCamera");
        camGos.hideFlags = HideFlags.HideAndDontSave;
        camGos.AddComponent<Camera>();
        cam1 = camGos.GetComponent<Camera>();
        cam1.enabled = false;

        frameRenderTexture = new RenderTexture(cameraWidth, cameraHeight, /*depth*/24, RenderTextureFormat.ARGB32);
        cameraPixels = new uint[cameraWidth * cameraHeight + 1];
    }

    // Update is called once per frame
    void Update()
    {
        //bool traverseKeyPressed = Input.GetKeyDown(traverseKey);
        //if (traverseKeyPressed)
        {
            if (index + 6 <= location_array.Length)
            {
                //for capturing panoramic frames
                //checkpointer = GameObject.Find("Capture Panorama").GetComponent<CapturePanorama.CapturePanorama>().checkpoint;
                //if ((captureframe) && ((checkpointer == 1) || ((int)location_array[index - 1] == 1)))
                {
                    //GameObject.Find("Capture Panorama").GetComponent<CapturePanorama.CapturePanorama>().checkpoint = 0;
                    //captureframe = false;
                    //to store the panoramic frame
                    //StartCoroutine(runningl());
                    //In order to capture the normal frame
                    StartCoroutine(runningframe());
                }
            }
        }
    }


    //for a fixed radius capturing the normal frame
    IEnumerator runningframe()
    {
        int zIndex = 0;
        int xIndex = 0;
        bool isInCut = false;
        Vector3 pos = new Vector3((float)location_array[index], (float)location_array[index + 1], (float)location_array[index + 2]);
        if (index < location_array.Length)
        {
            m_Rigidbody.MovePosition(pos);
            Debug.Log("\n the position is = " + pos);
        }


        // Debug.Log("radius_array: len " + radius_array.Length + " , " + (zIndex * cut * 5 + xIndex * 5 + 3) + ", " + zIndex + ", " + xIndex);
        //cam.nearClipPlane = 0.01f;
        //cam.farClipPlane = 1000f;
        yield return new WaitForSeconds(0.1f);
        //string filenameBase = ChangeFilename();
        //yield return new WaitForSeconds(1f);
        Camera[] cameras = GetCaptureCameras();
        cam1.CopyFrom(cameras[0]);
        cam1.targetTexture = frameRenderTexture;
        tex = RTImage(cam1);
        SaveFrame(tex, (int)(location_array[index - 1]-1));
        index += 7;
    }
    //for a fixed radius
    IEnumerator runningl()
    {
        int zIndex = 0;
        int xIndex = 0;
        bool isInCut = false;
        Vector3 pos = new Vector3((float)location_array[index], (float)location_array[index + 1], (float)location_array[index + 2]);
        if (index < location_array.Length)
        {
            m_Rigidbody.MovePosition(pos);
            Debug.Log("\n the position is = " + pos);
        }


        // Debug.Log("radius_array: len " + radius_array.Length + " , " + (zIndex * cut * 5 + xIndex * 5 + 3) + ", " + zIndex + ", " + xIndex);
        cam.nearClipPlane = radius_intend;
        cam.farClipPlane = 1000f;
        string filenameBase = ChangeFilename();
        Debug.Log("\n current file name = " + filenameBase);
        yield return new WaitForSeconds(1f);
        GameObject.Find("Capture Panorama").GetComponent<CapturePanorama.CapturePanorama>().CaptureScreenshotAsync(filenameBase);
        index += 7;
        captureframe = true;
        yield return null;
    }
    //for adaptive radius
    IEnumerator running()
    {
        int zIndex = 0;
        int xIndex = 0;
        bool isInCut = false;
        Vector3 pos = new Vector3((float)location_array[index], (float)location_array[index + 1], (float)location_array[index + 2]);
        if (index < location_array.Length)
        {
            m_Rigidbody.MovePosition(pos);
            Debug.Log("\n the position is = " + pos);
        }

        for (zIndex = 0; zIndex < cut; zIndex++)
        {
            for (xIndex = 0; xIndex < cut; xIndex++)
            {
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
            // Debug.Log("radius_array: len " + radius_array.Length + " , " + (zIndex * cut * 5 + xIndex * 5 + 3) + ", " + zIndex + ", " + xIndex);
            cam.nearClipPlane = 0.01f;// (float)radius_array[(int)(zIndex * cut * 5 + xIndex * 5 + 3)];
            string filenameBase = ChangeFilename();
            yield return new WaitForSeconds(1f);
            GameObject.Find("CapturePanorama").GetComponent<CapturePanorama.CapturePanorama>().CaptureScreenshotAsync(filenameBase);
        }
        index += 7;
        captureframe = true;
        yield return null;
    }

    String ChangeFilename()
    {
        int timestamp = (int)location_array[index - 1] - 1;
        //long timestamp = (long)location_array[index - 1];
        string filenameBase = null;
        Debug.Log("\n from filenameBase, index = " + index + ", location array[index-1]  = " + location_array[index - 1]);
        if (output_flag == 1)
        {
            // v1: put in the same folder
            filenameBase = "sj_" + timestamp.ToString();
            //filenameBase = String.Format("{0}_{1:D3}", "sj", timestamp);
        }
        else if (output_flag == 2)
        {
            // v2: for saving multiple folders in parallel
            filenameBase = "cj_" + timestamp.ToString();
        }
        return filenameBase;
    }

    // for soccer game with only one terrain
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

    void FindTerrainDiml()
    {
        GameObject terrainObj;
        Component terrainMesh;
        GameObject parentObj = GameObject.Find(parentObjName);
        Bounds terrainBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        Component[] comps = parentObj.GetComponentsInChildren<MeshFilter>();
        foreach (Component comp in comps)
        {
            if (!comp.gameObject.name.Contains(terrainName)) continue;
            // Debug.Log(comp.gameObject.name);
            terrainObj = comp.gameObject;
            terrainMesh = terrainObj.GetComponent<MeshRenderer>();
            if (terrainMesh == null) // Haven't been tested yet
            {
                Component terrainT = terrainObj.GetComponent<Terrain>();
                if (terrainT == null) Debug.Log("FindRadius: terrain " + terrainName + " is neither type of Mesh nor type of Terrain!");
                else
                {
                    terrainBounds = ((Terrain)terrainT).terrainData.bounds; // in the local space
                    if (terrainBounds.size.x == 0 && terrainBounds.size.y == 0 && terrainBounds.size.z == 0)
                        terrainBounds = new Bounds(terrainBounds.center + terrainObj.transform.position, terrainBounds.size);
                    else
                        terrainBounds.Encapsulate(new Bounds(terrainBounds.center + terrainObj.transform.position, terrainBounds.size));
                }
            }
            else
            {
                if (terrainBounds.size.x == 0 && terrainBounds.size.y == 0 && terrainBounds.size.z == 0)
                    terrainBounds = ((MeshRenderer)terrainMesh).bounds;
                else
                    terrainBounds.Encapsulate(((MeshRenderer)terrainMesh).bounds);
            }

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

    }

    void ReadFile()
    {
        StreamReader reader = new System.IO.StreamReader(location_file);
        var list = new List<double>();
        string[] ar;
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            ar = line.Split(',');
            for (int i = 0; i < ar.Length; i++)
            {
                double tmp = 0f;
                double.TryParse(ar[i], out tmp);
                list.Add(tmp);
            }
        }
        location_array = list.ToArray();
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

    

    void SaveFrame(Texture2D tex, int loopNum)
    {
        byte[] bytes = tex.EncodeToPNG();
        //byte[] bytes = tex.EncodeToJPG();
        //byte[] bytes = tex.EncodeToJPG(100);
        UnityEngine.Object.Destroy(tex);
        string filenameBase = String.Format("{0}_{1:D3}", "H:/CubeMaps/asplos_final/final_frame_1/sj", loopNum);
        File.WriteAllBytes(filenameBase + ".png", bytes);
        //File.WriteAllBytes(filenameBase + ".jpg", bytes);
    }

    Texture2D RTImage(Camera cam)
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;
        cam.Render();
        Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = currentRT;
        return image;
    }

    public virtual Camera[] GetCaptureCameras()
    {
        Camera[] cameras = Camera.allCameras;

        var finalCameras = new List<Camera>();
        foreach (Camera c in cameras)
        {
            finalCameras.Add(c);
        }

        return finalCameras.ToArray();
    }

}