using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;



public class AutoMove : MonoBehaviour {

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
    public int bytelength;
    public Vector3 clientposition;
    public bool ismoving;
    byte[] data;

    //capturing frames for any given location and angle
    public Vector3 positionr;
    public Vector3 rotationr;
	int loopNum = 0;



    private Rigidbody m_Rigidbody;
    //public bool isAtStartup = true;
    NetworkServer myserver;
    public const short RegisterHostMsgId = 888;
    public class RegisterHostMessage : MessageBase
    {
        public Vector3 modifiedposition;
        public int istouching;
    }

    Camera cam;
    GameObject camGos;
    RenderTexture frameRenderTexture = null;
    Texture2D tex;
    int cameraWidth = 1920; //2048; 
    int cameraHeight = 1080; //1024;
    uint[] cameraPixels = null;


    // Use this for initialization
    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>(); //rigidbody is assigned with the probe_camera which is taking care of
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

    void Start() {
        Debug.Log("This script aims to move the camera automatically!");
        camGos = GameObject.Find("ProbeCamera");
        cam = GameObject.Find("ProbeCamera/MainCamera").GetComponent<Camera>();
        Debug.Log("\n from start, the current camera position is = " + cam.transform.position);
        //cam.enabled = false;
        frameRenderTexture = new RenderTexture(cameraWidth, cameraHeight, /*depth*/24, RenderTextureFormat.ARGB32);
        Debug.Log("\n CameraWidth =" + cameraWidth);
        cameraPixels = new uint[cameraWidth * cameraHeight + 1];
       // Time.captureFramerate = 120;
        //myserver = new NetworkServer();
        /*NetworkServer.Reset();
        NetworkServer.Listen(34005);*/
        //frameRenderTexture = RenderTexture.active;
        //cam.targetTexture = frameRenderTexture;
    }


    public void OnPlayerReadyMessage(NetworkMessage netMsg)
    {
        // TODO: create player and call PlayerIsReady()
    }
    public void Deserialize(NetworkReader reader)
    {
        clientposition = reader.ReadVector3();
        ismoving = reader.ReadBoolean();
    }


    // Update is called once per frame
    void Update() {
       /* bool traverseKeyPressed = Input.GetKeyDown(traverseKey);
        if (traverseKeyPressed)
        {
            StartCoroutine(TraverseRect());
            //StartCoroutine(TraverseCircle());
        }*/

        //setting up for any Camera position and rotation
        /*camGos.transform.position = positionr;
        camGos.transform.rotation = Quaternion.Euler(rotationr);*/

       


        bool traverseKeyPressed = Input.GetKeyDown(traverseKey);
        if (traverseKeyPressed)  //logical OR for unity if ( NetworkServer.localClientActive ||
        {


            //Debug.Log("Start position is " + m_Rigidbody.position);
            Debug.Log("\n the current location is =" + camGos.transform.position + "Current rotation is =" + camGos.transform.rotation);

            Camera[] cameras = GetCaptureCameras();
            cam.CopyFrom(cameras[0]); //hhhhhhhhhh
            cam.targetTexture = frameRenderTexture;
			float startt = Time.realtimeSinceStartup;
            tex = RTImage(cam);
            Debug.Log("\n time to render by the server" + (Time.realtimeSinceStartup - startt));
            float endt = Time.realtimeSinceStartup;
            SaveFrame(tex, loopNum);
			Debug.Log("\n time to capture and save by the server" + (Time.realtimeSinceStartup - endt));
            Debug.Log("\n Size is " + tex.width + " by " + tex.height + " and the time to last to current is =" + Time.unscaledDeltaTime);
			/*
             * Vector3 pos = m_Rigidbody.position;
			pos.z += 2f;
			m_Rigidbody.MovePosition (pos);
			Debug.Log ("\n the current position of rigid body = " + m_Rigidbody.position);
            */
			loopNum++;
        }
            /*
             //StartCoroutine(TraverseRect());


             //NetworkManager netman = new NetworkManager();
             //if (netman.IsClientConnected())
            // {
              /*   Debug.Log("\n  client is connected");
                 ConnectionConfig config = new ConnectionConfig();
                 int mChannelId = config.AddChannel(QosType.Reliable);
                 NetworkReader reader = new NetworkReader(data); //new instance of network reader
                 Deserialize(reader);

                 RegisterHostMessage msg = new RegisterHostMessage();
                 NetworkServer.RegisterHandler(MsgType.Ready, OnPlayerReadyMessage); //Internal networking system message for clients to tell server they are ready.




                 //modified position of the users
                 Vector3 ts = msg.modifiedposition;
                 if (ismoving)
                     msg.modifiedposition = new Vector3(ts.x, ts.y, ts.z += (Time.deltaTime * 5)); //playerMoveSpeed is 5 now


            // }

             //myClient.Send(RegisterHostMsgId, msg);

             //NetworkServer.SendBytesToReady(tex, bytelength, mChannelId);


             //StartCoroutine(TraverseRect());
             //StartCoroutine(TraverseCircle());
             // }
         }*/
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
        currentPoint = m_Rigidbody.position; // + new Vector3(1,0,0);
        //m_Rigidbody.position = m_Rigidbody.position + new Vector3(1,0,0);
        int loopNum = 0;
        /*string formatString = "    {0,10}_" +
                            "{1,10}_" +
                            "{2,10}";*/
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

            filenameBase = String.Format("{0}_{1:D3}", "sunlh", loopNum);//);"{0}_{1:D3}"
            //string filenameBase = String.Format("{0}_{1:x_y_z}", "sunlh", currentPoint.ToString("0.0000000"));
            float startt= Time.realtimeSinceStartup;
            GameObject.Find("Capture Panorama").GetComponent<CapturePanorama.CapturePanorama>().CaptureScreenshotSync(filenameBase);
            Debug.Log("\n the file name base is =" + filenameBase);
            Debug.Log("Time to take panorama screenshot from automove: " + (Time.realtimeSinceStartup - startt) + " sec");
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
        //filenameBase = String.Format(formatString, loopNum++, currentPoint.x, currentPoint.z);
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
        byte[] bytes = tex.EncodeToPNG();
        //byte[] bytes = tex.EncodeToJPG(); //Here I have to encode it using H264 video codec
        Debug.Log("\n in bytes, the length of the frame will be" + bytes.Length);
        Debug.Log("\n Size is " + tex.width + " by " + tex.height);
        bytelength = bytes.Length;
        //byte[] bytes = tex.EncodeToH264();
        //byte[] bytes = tex.EncodeToJPG(100);
        UnityEngine.Object.Destroy(tex);
        string filenameBase = String.Format("{0}_{1:D3}", "E:/CubeMaps/viking_test_0701/viking_desktop_0127_", loopNum);
        //string filenameBase = String.Format("{0}_{1:D3}", Application.dataPath + "/../FinalFrame", loopNum);
        Debug.Log("\n filename Base" + filenameBase);
        File.WriteAllBytes(filenameBase +".png", bytes);
        //File.WriteAllBytes(filenameBase +".jpg", bytes);
    }

    Texture2D RTImage(Camera cam)
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;
		float rendert = Time.realtimeSinceStartup;
		cam.Render();
		Debug.Log("\n Original Rendering by the server = " + (Time.realtimeSinceStartup - rendert));
        
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

