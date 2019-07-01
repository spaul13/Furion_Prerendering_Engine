using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;

public class AutoMove_cube : MonoBehaviour
{

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

    Camera cam;
    GameObject camGos;
    RenderTexture frameRenderTexture = null;
    Texture2D tex;
    int cameraWidth = 2048;
    int cameraHeight = 1024;
    uint[] cameraPixels = null;

    string camUnityPath = "ProbeCamera/MainCamera";
    public GameObject cube;

    Vector3 ts;


    // Use this for initialization
    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        startRotation.eulerAngles = startRotationEuler;
        endRotation.eulerAngles = endRotationEuler;
    }

    private void OnEnable()
    {
        // When the object is turned on, make sure it's not kinematic.
        m_Rigidbody.isKinematic = false;

        m_Rigidbody.MovePosition(startPoint);
    }

    private void OnDisable()
    {
        // When the object is turned off, set it to kinematic so it stops moving.
        m_Rigidbody.isKinematic = true;
    }

    void Start()
    {
        Debug.Log("This script aims to move the camera automatically!");
        camGos = new GameObject("ManCamera");
        // camGos.hideFlags = HideFlags.HideAndDontSave;
        camGos.AddComponent<Camera>();
        cam = camGos.GetComponent<Camera>();
        cam.enabled = false;
        //cam = GameObject.Find(camUnityPath).GetComponent<Camera>();
        frameRenderTexture = new RenderTexture(cameraWidth, cameraHeight, /*depth*/24, RenderTextureFormat.ARGB32);
        Material m = GameObject.Find("Cube").GetComponent<MeshRenderer>().material;
        m.mainTexture = frameRenderTexture;
        cameraPixels = new uint[cameraWidth * cameraHeight + 1];
        cube = GameObject.Find("Cube");
        //ts = camGos.transform.position;
        ts = GameObject.Find(camUnityPath).transform.position;
        camGos.transform.position = ts;
    }

    // Update is called once per frame
    void Update()
    {

        ts.z -= 0.001f;
        camGos.transform.position = ts;
        cube.SetActive(false);
        cam.targetTexture = frameRenderTexture;
        Texture2D tex = RTImage(cam);
        cube.SetActive(true);

    }

   

    void FixedUpdate()
    {
        // Called every fixed framerate, typically 0.02s
        //Move();
        //Debug.Log("Current position is " + m_Rigidbody.position);
    }

    IEnumerator TraverseRect()
    {
        Debug.Log("Start position is " + m_Rigidbody.position);
        currentPoint = m_Rigidbody.position;
        int loopNum = 0;
        int x, y, z;
        string filenameBase;
        while (!currentPoint.Equals(endPoint))
        {
            // Each loop represents a specific position in the selected area
            // TODO: call the CapturePanorama.cs for each loop
            //string filenameBase = String.Format("{0}_{1:D3}", "sunlh", loopNum);
            x = Convert.ToInt32(Math.Floor(currentPoint.x));
            y = Convert.ToInt32(Math.Floor(currentPoint.y));
            z = Convert.ToInt32(Math.Floor(currentPoint.z));
            //string filenameBase = String.Format("{0}_{1:D3}_{2:D3}_{3:D3}_{4:D3}", "sunlh", x, y, z, loopNum % 32);
            filenameBase = String.Format("{0}_{1:D3}", "sunlh", loopNum);
            //string filenameBase = String.Format("{0}_{1:x_y_z}", "sunlh", currentPoint.ToString("0.0000000"));
            //CapturePanorama.CapturePanorama.(CaptureScreenshotSync(filenameBase);
            GameObject.Find("Capture Panorama").GetComponent<CapturePanorama.CapturePanorama>().CaptureScreenshotSync(filenameBase);
            if (currentPoint.z == endPoint.z)
            {
                Debug.Log("The camera has reached the upper boundary of the selected area!");
                currentPoint.z = startPoint.z;
                MoveAlongX();
            }
            else
            {
                MoveAlongZ();
            }
            loopNum++;

            yield return new WaitForSeconds(1f);  // stop and wait for 1 second

            /*
            if (loopNum%100 == 0)
            {
                yield return 1;
            }
            */
        }
        filenameBase = String.Format("{0}_{1:D3}", "sunlh", loopNum++);
        GameObject.Find("Capture Panorama").GetComponent<CapturePanorama.CapturePanorama>().CaptureScreenshotSync(filenameBase);
        Debug.Log("The camera has gone through the selected area");
    }

    IEnumerator TraverseCircle()
    {

        //Debug.Log("Start rotation is " + m_Rigidbody.rotation);
        //currentRotation = m_Rigidbody.rotation;
        //currentRotationEuler = currentRotation.eulerAngles;
        m_Rigidbody.MoveRotation(startRotation);
        currentRotation = startRotation;
        currentRotationEuler = currentRotation.eulerAngles;
        Debug.Log("Start rotation is " + currentRotationEuler);
        yield return new WaitForSeconds(1f);  // stop and wait for 1 second, so that it will not capture the previous frame
        int loopNum = 0;
        while (!currentRotation.Equals(endRotation))
        {
            //RotateClockwise();
            Camera[] cameras = GetCaptureCameras();
            cam.CopyFrom(cameras[0]);
            cam.targetTexture = frameRenderTexture;
            tex = RTImage(cam);
            SaveFrame(tex, loopNum);
            RotateClockwise();
            loopNum++;
            yield return new WaitForSeconds(2f);  // stop and wait for 2 second
        }
    }

    void MoveAlongZ()
    {
        currentPoint.z += delta;
        //Debug.Log(currentPoint.z);
        m_Rigidbody.MovePosition(currentPoint);
        Debug.Log("Current position is " + m_Rigidbody.position);
    }

    void MoveAlongX()
    {
        currentPoint.x += delta;
        m_Rigidbody.MovePosition(currentPoint);
        Debug.Log("Current position is " + m_Rigidbody.position);
    }

    void RotateClockwise()
    {
        currentRotationEuler.y += 1f;
        currentRotation.eulerAngles = currentRotationEuler;
        m_Rigidbody.MoveRotation(currentRotation);
        Debug.Log("Current rotarion is " + currentRotationEuler);
    }

    void SaveFrame(Texture2D tex, int loopNum)
    {
        //byte[] bytes = tex.EncodeToPNG();
        byte[] bytes = tex.EncodeToJPG();
        //byte[] bytes = tex.EncodeToJPG(100);
        UnityEngine.Object.Destroy(tex);
        string filenameBase = String.Format("{0}_{1:D3}", Application.dataPath + "/../FinalFrame", loopNum);
        //File.WriteAllBytes(filenameBase +".png", bytes);
        File.WriteAllBytes(filenameBase + ".jpg", bytes);
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