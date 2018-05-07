using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System;
using System.IO;
using System.Text;
using System.Globalization;

public class DataCollector : MonoBehaviour
{
    [SerializeField, Header("Data:")]
    private DataCollectionConfig config;
    [SerializeField]
    private CarData carData;

    [SerializeField]
    public GameObject CameraPanel;
    [SerializeField]
    public RawImage CameraRawImage;
    [SerializeField]
    public GameObject LidarPanel;
    [SerializeField]
    public RawImage LidarRawImage;
    [SerializeField]
    public GameObject LanePanel;

    [SerializeField, Space]
    private RenderTexture cameraInput;

    [SerializeField]
    private Transform lidar;
    [SerializeField]
    private float lidarDistance;
    [SerializeField, Range(0, 180)]
    private float
        lidarViewAngleX,
        lidarViewAngleY;
    [SerializeField]
    private int
        lidarWidth,
        lidarHeight;
    [Header("Detection tags"), Space]
    [SerializeField] string tagHuman = "Human";
    [SerializeField] string tagObstacle = "Obstacle";
    [Header("Lidar Labels"), Space]
    [SerializeField] string labelHuman = "Human";
    [SerializeField] string labelObstacle = "Obstacle";

    [SerializeField, HideInInspector]
    private Vector3[,] lidarRaycasts;

    [SerializeField, Space]
    private int cameraCollectionFrameRate = 5;
    [SerializeField, Space]
    private int lidarCollectionFrameRate = 5;

    private Coroutine collectingCameraData;
    private Coroutine collectingLidarData;


    private Texture2D camera;
    private bool innerLane = true;
    private TCPConnection myTCP;                                                // Object responsible for TCP connection   
    private string action = "";
    private Texture2D lidarOutput;
    private byte[] delimeter = new byte[20] { 0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff };

    public void Begin()
    {
        InitLidar();
        if (config.Lidar)
        {
            LidarPanel.SetActive(true);
            collectingLidarData = StartCoroutine(CollectingLidarData());
        }
        if (config.Camera)
        {
            CameraPanel.SetActive(true);
            collectingCameraData = StartCoroutine(CollectingCameraData());
        }

        if(!config.isDataCollection)
        {
            LanePanel.SetActive(true);
        }

        myTCP = gameObject.AddComponent<TCPConnection>();
        myTCP.setupSocket("127.0.0.1", 7777);
    }

    public void End()
    {
        if(collectingCameraData   !=null) StopCoroutine(collectingCameraData);
        if(collectingLidarData  !=null) StopCoroutine(collectingLidarData);
    }

    private IEnumerator CollectingLidarData()
    {
        string filename;
        RaycastHit hit;
        lidarOutput = new Texture2D(lidarWidth, lidarHeight, TextureFormat.RGB24, false);
        string labelAll = labelHuman + "_" + labelObstacle;

        Loop:
        if (Time.frameCount % lidarCollectionFrameRate == 0)
        {
            bool humanDetected = false, obstacleDetected = false;
            for (int x = 0; x < lidarWidth; x++)
            {
                for (int y = 0; y < lidarHeight; y++)
                {
                    if (y < lidarHeight / 2)
                    {
                        lidarOutput.SetPixel(lidarWidth - x - 1, lidarHeight - y - 1, new Color(0f, 1f, 0f));
                    }
                    else
                    {
                        lidarOutput.SetPixel(lidarWidth - x - 1, y - lidarHeight / 2, new Color(0f, 1f, 0f));
                    }

                    if (Physics.Raycast(lidar.position, lidar.TransformDirection(-lidarRaycasts[x, y]), out hit, lidarDistance))
                    {
                        if (!obstacleDetected && hit.collider.tag == tagObstacle)
                        {
                            obstacleDetected = true;
                        }
                        if (!humanDetected && hit.collider.tag == tagHuman)
                        {
                            humanDetected = true;
                        }
                        if(y < lidarHeight / 2)
                        {
                            lidarOutput.SetPixel(lidarWidth - x - 1, lidarHeight - y - 1, new Color(0f, (hit.distance / lidarDistance), 0f));
                        }
                        else
                        {
                            lidarOutput.SetPixel(lidarWidth - x - 1, y - lidarHeight / 2, new Color(0f, (hit.distance / lidarDistance), 0f));
                        }
                    }
                }
            }

            if (humanDetected)
            {
                carData.label = labelHuman;
            }
            else if (obstacleDetected)
            {
                carData.label = labelObstacle;
            }
            else
            {
                carData.label = "None";
            }

            lidarOutput.Apply();
            LidarRawImage.texture = lidarOutput;


            if (config.isDataCollection)
            {
                filename = DateTime.Now.ToFileTime().ToString();
                File.WriteAllBytes($"{config.lidarFolder}/{filename}_lidar.jpg", lidarOutput.EncodeToJPG());
                File.AppendAllText(config.lidarCSV, $"{filename}_lidar {carData.label}\n");
            }
        }

        yield return new WaitForFixedUpdate();
        goto Loop;
    }

    private IEnumerator CollectingCameraData()
    {
        if (camera == null)
        {
            camera = new Texture2D(cameraInput.width, cameraInput.height, TextureFormat.RGB24, false);
        }
        RenderTexture bufferTexture;
        Rect viewport = new Rect(0f,0f,cameraInput.width, cameraInput.height);
        string filename;
        
        Loop:
        if (Time.frameCount % cameraCollectionFrameRate == 0)
        {
            bufferTexture = RenderTexture.active;
            RenderTexture.active = cameraInput;
            camera.ReadPixels(viewport,0,0);
            RenderTexture.active = bufferTexture;

            camera.Apply();
            CameraRawImage.texture = camera;

            if (!config.isDataCollection)
            {
                myTCP.writeSocket(Combine(camera.GetRawTextureData(), delimeter, lidarOutput.GetRawTextureData()));

                action = "";
                while (action == "")
                {
                    SocketResponse();
                }
                
                Debug.Log("[SERVER] -> " + action);

                GameObject.Find("Lane").GetComponentInChildren<Text>().text = "      Lane: " + action.Split(new Char[] { '#' })[1]
                                                                            + "\nDetection: " + action.Split(new Char[] { '#' })[2];



                carData.angle = float.Parse(action.Split(new Char[] { '#' })[0], CultureInfo.InvariantCulture);
            }

            if (config.isDataCollection)
            {
                filename = DateTime.Now.ToFileTime().ToString();
                File.WriteAllBytes($"{config.cameraFolder}/{filename}_camera.jpg", camera.EncodeToJPG());
                File.AppendAllText(config.cameraCSV, $"{filename}_camera {(carData.angle.ToString()).Replace(',', '.')} {carData.lane}\n");
            }
        }
        yield return new WaitForEndOfFrame();
        goto Loop;
    }

    public void isCollection(bool value)
    {
        config.isDataCollection = value;
    }

    void SocketResponse()
    {
        action = myTCP.readSocket();
    }

    public static byte[] Combine(byte[] first, byte[] second, byte[] third)
    {
        byte[] ret = new byte[first.Length + second.Length + third.Length];
        Buffer.BlockCopy(first, 0, ret, 0, first.Length);
        Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
        Buffer.BlockCopy(third, 0, ret, first.Length + second.Length,
                         third.Length);
        return ret;
    }

    private void InitLidar()
    {
        lidarRaycasts = new Vector3[lidarWidth, lidarHeight];

        Vector3[] lidarXInitilial = new Vector3[lidarWidth];

        Vector3 max = new Vector3(lidarDistance, 0, 0);
        Vector3 min = new Vector3(-lidarDistance, 0, 0);
        float angle = (lidarViewAngleY * 0.5f) + 90f;

        float angleShift = lidarViewAngleY / lidarWidth;

        for (int i = 0; i < lidarWidth; i++)
        {
            lidarXInitilial[i] = Vector3.Slerp(max, min, angle / 180f);
            angle -= angleShift;
        }

        int halfOfHeight = lidarHeight / 2;
        angleShift  = lidarViewAngleX / lidarHeight;

        for (int x = 0; x < lidarWidth; x++)
        {
            max = new Vector3(0, -lidarDistance, 0);
            min = lidarXInitilial[x];

            angle = lidarViewAngleX * 0.5f;

            for (int y = 0; y < halfOfHeight; y++)
            {
                lidarRaycasts[x, y] = Vector3.Slerp(min, max, angle / 90f);
                angle -= angleShift;
            }

            max = new Vector3(0, lidarDistance, 0);

            angle = lidarViewAngleX * 0.5f;

            for (int y = halfOfHeight; y < lidarHeight; y++)
            {
                lidarRaycasts[x, y] = Vector3.Slerp(min, max, angle / 90f);
                angle -= angleShift;
            }
        }
    }

#if UNITY_EDITOR
    [Header("DEBUG")]
    [SerializeField] bool drawLidar = false;
    [SerializeField] Color hitColor = Color.green;

    public void OnDrawGizmos()
    {
        if (drawLidar)
        {
            //if (lidarRaycasts == null ||
            //    lidarWidth != lidarRaycasts.GetLength(0) ||
            //    lidarHeight != lidarRaycasts.GetLength(1))
            //{
            //}
            InitLidar();

            Gizmos.color = Color.red;
            Gizmos.DrawLine(lidar.position, lidar.TransformPoint(new Vector3(lidarDistance, 0, 0)));
            Gizmos.DrawLine(lidar.position, lidar.TransformPoint(new Vector3(-lidarDistance, 0, 0)));
            Gizmos.DrawLine(lidar.position, lidar.TransformPoint(new Vector3(0, lidarDistance, 0)));
            Gizmos.DrawLine(lidar.position, lidar.TransformPoint(new Vector3(0, -lidarDistance, 0)));
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(lidar.position, lidarDistance);


            RaycastHit hit;
            Gizmos.color = hitColor;
            for (int x = 0; x < lidarWidth; x++)
            {
                for (int y = 0; y < lidarHeight; y++)
                {
                    if (Physics.Raycast(lidar.position, lidar.TransformDirection(-lidarRaycasts[x, y]), out hit, lidarDistance))
                    {
                        Gizmos.DrawLine(lidar.position, lidar.position + lidar.TransformVector(-lidarRaycasts[x, y]).normalized * hit.distance);
                    }
                    else
                    {
                        Gizmos.DrawLine(lidar.position, lidar.TransformPoint(-lidarRaycasts[x,y]));
                    }
                }
            }
        }
    }
#endif
}
