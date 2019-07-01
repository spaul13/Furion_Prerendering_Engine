using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System;

public class Display_capture : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    private GameObject SM;
    // Use this for initialization
    string location_file = "E:/CubeMaps/human_study_locations/hs_viking_1.txt";
    double[] location_array;
    int index;
    bool captureframe;
    string camUnityPath = "ProbeCamera/MainCamera";
    Rigidbody m_Rigidbody;
    Camera cam;
    int output_flag = 1;
    int counter = 0;
    String str1 = "E:/CubeMaps/hs_graeae_farbe_videos/viking_graeae_1_new_mp4/sj_";
    int checkpointer;

    void Start()
    {

        m_Rigidbody = GameObject.Find("ProbeCamera").GetComponent<Rigidbody>();
        cam = GameObject.Find(camUnityPath).GetComponent<Camera>();

        ReadFile();
        SM = GameObject.Find("SphereMovie");
        videoPlayer = SM.GetComponent<VideoPlayer>();

        index = 1;
        captureframe = true;

    }

    // Update is called once per frame
    void Update()
    {

        if (index + 6 <= location_array.Length)
        {
            //for capturing panoramic frames
            checkpointer = GameObject.Find("Capture Panorama").GetComponent<CapturePanorama.CapturePanorama>().checkpoint;
            if ((captureframe) && ((checkpointer == 1) || ((int)location_array[index - 1] == 0)))
            {
                captureframe = false;
                GameObject.Find("Capture Panorama").GetComponent<CapturePanorama.CapturePanorama>().checkpoint = 0; //changed to zero
                Debug.Log("\n the current modulo = " + ((int)location_array[index - 1]) % 3 + "," + counter);
                if ((((int)location_array[index - 1]) % 3 == 0)) //(counter==5) || 
                {
                    if (videoPlayer.isPlaying)
                    {
                        Debug.Log("\n Checking this one to stop the video");
                        videoPlayer.Stop();
                    }
                    counter = 0;
                    int filestr = (int)(((int)location_array[index - 1]));
                    videoPlayer.url = str1 + filestr.ToString() + ".mp4";
                    Debug.Log("\n the current URL is = " + videoPlayer.url);
                    videoPlayer.Prepare();
                    Debug.Log("\n Preparing...");
                    videoPlayer.Play();
                    Debug.Log("\n Playing...");
                }
                if ((videoPlayer.isPlaying) || (counter > 0))
                {
                    Debug.Log("\n Check Playing... to call moveCapture() " + videoPlayer.url);
                    StartCoroutine(moveCapture());
                }
            }
        }

    }


    IEnumerator moveCapture()
    {

        Vector3 pos = new Vector3((float)location_array[index], (float)location_array[index + 1], (float)location_array[index + 2]);
        if (index < location_array.Length)
        {
            m_Rigidbody.MovePosition(pos);
            counter = counter + 1;
            Debug.Log("\n the position is = " + pos);
        }
        pos = new Vector3((float)location_array[index], (float)location_array[index + 1], (float)location_array[index + 2]);
        SM.gameObject.transform.position = pos;
        Debug.Log("\n Inside MoveCapture");
        cam.nearClipPlane = 0.03f; //using same radius for all locations
        cam.farClipPlane = 10f;
        string filenameBase = ChangeFilename();
        yield return new WaitForSeconds(2f);
        GameObject.Find("Capture Panorama").GetComponent<CapturePanorama.CapturePanorama>().CaptureScreenshotAsync(filenameBase);
        Debug.Log("\n After calling capture panorama...");
        index += 7;
        captureframe = true;

        yield return null;
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

    String ChangeFilename()
    {
        long timestamp = (long)location_array[index - 1];
        string filenameBase = null;
        if (output_flag == 1)
        {
            // v1: put in the same folder
            filenameBase = String.Format("{0}_{1:D3}", "sj", timestamp);
        }
        else if (output_flag == 2)
        {
            // v2: for saving multiple folders in parallel
            filenameBase = "cj_" + timestamp.ToString();
        }
        return filenameBase;
    }
}
