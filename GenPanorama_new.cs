using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class GenPanorama_new : MonoBehaviour
{
    string parentObjName = "Content";
    string terrainName = "Terrain/terrain_01/terrain_near_01";
    string radius_file = "H:/cut_datas/begin_exp/new_radius_viking/0605/logexp_viking_amend_newradius_16_new.txt"; // 
    string location_file = "H:/test_split_soccer/log_1216_playerloc_4.txt";//"H:/test_split_soccer/location_viking_103019_16.txt"; //"H:/CubeMaps/human_study_locations/hs_viking_1.txt";//"C:/Users/spaul/Downloads/filter_exp13_gafps_133458.txt"; //"C:/Users/spaul/Downloads/filter_exp14_gafps_175082.txt"; //"E:/cut_datas/new_exp/location_cut_split/loc_remov_zero/viking_loc_12_locpat_nonzero.txt"; // 
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

    int loopNum;

    string fp = "";

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        cam = GameObject.Find(camUnityPath).GetComponent<Camera>();
        fp = "E:/test_split_fps/viking/";

        FindTerrainDiml();
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

        loopNum = 0;
        index = 1;
        captureframe = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (index + 6 <= location_array.Length)
        {
            //for capturing panoramic frames
            if (captureframe)
            {
                captureframe = false;
                StartCoroutine(running());
            }
        }
    }

    IEnumerator running()
    {
        int zIndex = 0;
        int xIndex = 0;
        bool isInCut = false;
        Vector3 pos = new Vector3((float)location_array[index], (float)location_array[index + 1], (float)location_array[index + 2]);
        //Vector3 loc = new Vector3();
        //pos.y += 50f;
        //AlignWithTerrain(pos, out loc); //rot is not needed
        //Debug.Log("\n the previous pos = " +pos +", modified loc = " +loc);

        if (index < location_array.Length)
        {
            //m_Rigidbody.MovePosition(pos);
            m_Rigidbody.MovePosition(pos);
            // Debug.Log("\n the position is = " + pos);
        }

        cam.nearClipPlane = 10f;
        cam.farClipPlane = 1000f;
        /*
        for (zIndex = 0; zIndex < cut; zIndex++)
        {
            for (xIndex = 0; xIndex < cut; xIndex++)
            {
                if (subBounds[(int)(zIndex * cut + xIndex)].Contains(pos)) //pos
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
            if ((float)radius_array[(int)(zIndex * cut * 5 + xIndex * 5 + 3)] >= 90f)
                cam.nearClipPlane = (float)radius_array[(int)(zIndex * cut * 5 + xIndex * 5 + 3)] - 40f;
            else
                cam.nearClipPlane = (float)radius_array[(int)(zIndex * cut * 5 + xIndex * 5 + 3)];
            cam.farClipPlane = 1000f;
            Debug.Log("*****modified loc " + pos + " radius " + (float)radius_array[(int)(zIndex * cut * 5 + xIndex * 5 + 3)] + ","+ cam.nearClipPlane + ", " + zIndex + ", " + xIndex);
            string filenameBase = ChangeFilename();
            yield return new WaitForSeconds(1f);
            GameObject.Find("CapturePanorama").GetComponent<CapturePanorama.CapturePanorama>().CaptureScreenshotAsync(filenameBase);
            //File.AppendAllText(fp + "logexp_panoramicindexing_0123_racing_2.txt", "[ position = (" + m_Rigidbody.position.x + "," + m_Rigidbody.position.y + "," + m_Rigidbody.position.z + ") ,index =" + loopNum + ", radius =" + cam.nearClipPlane + " ]\n");
        }
        */
        string filenameBase = ChangeFilename();
        yield return new WaitForSeconds(1f);
        GameObject.Find("Capture Panorama").GetComponent<CapturePanorama.CapturePanorama>().CaptureScreenshotAsync(filenameBase);
        index += 7;
        captureframe = true;
        yield return null;
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

    String ChangeFilename()
    {
        long timestamp = (long)location_array[index - 1]-1;
        string filenameBase = null;
        if (output_flag == 1)
        {
            // v1: put in the same folder
            filenameBase = String.Format("{0}_{1:D3}", "sj", timestamp);
            //filenameBase = "sj_" + timestamp.ToString();
            //filenameBase = "sj_" + loopNum.ToString();
            loopNum++;
        }
        else if (output_flag == 2)
        {
            // v2: for saving multiple folders in parallel
            filenameBase = "cj_" + timestamp.ToString();
        }
        return filenameBase;
    }


    void AlignWithTerrain(Vector3 randLoc, out Vector3 loc)
    {
        RaycastHit hit;
        Debug.Log("\n Inside AlignWithTerrain: the randLoc is = " + randLoc);
        if (Physics.Raycast(randLoc, Vector3.down, out hit))
        {
            loc = hit.point;
            //rot = new Vector3(hit.normal.x, 0f, hit.normal.z);
        }
        else
        {
            Debug.Log("FindRadius: cannot align with terrain for location " + randLoc + " for loopNum " + loopNum);
            loc = new Vector3();
            //rot = new Vector3();
        }
    }
}