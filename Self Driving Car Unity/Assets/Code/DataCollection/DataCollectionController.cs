using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System.IO;
using System.Text;

public class DataCollectionController : MonoBehaviour {

    [SerializeField]
    private Button 
        test,
        start,
        reset;

    [SerializeField]
    private GameObject SettingsPanel;

    private bool settingsDropUnDown = false;

    [SerializeField, Space]
    private GameObject warningPanel;
    [SerializeField]
    private InputField
        lidarFolderField,
        lidarCSVField,
        cameraFolderField,
        cameraCSVField;

    [SerializeField, Space]
    private Toggle lidarToggle;
    [SerializeField]
    private Toggle cameraToggle;
    
    [SerializeField, Space]
    private DataCollectionConfig config;

    [SerializeField]
    private UnityEvent OnDataCollectionStart;
    [SerializeField]
    private UnityEvent OnDataCollectionStop;

    private void Start()
    {
        UpdateText();
    }

    public void ResetConfig()
    {
        string defaultPath = Application.persistentDataPath;

        SetLidarFolder($"{defaultPath}/Lidar");
        SetLidarCSV($"{defaultPath}/lidar.csv");
        SetCameraFolder($"{defaultPath}/Camera");
        SetCameraCSV($"{defaultPath}/camera.csv");

        IsListenLidar = true;
        IsListenCamera = true;

        UpdateText();
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            OnDataCollectionStop.Invoke();
        }
    }

    // UI part
    public void UpdateText()
    {
        lidarFolderField.text = config.lidarFolder;
        lidarCSVField.text = config.lidarCSV;

        cameraFolderField.text = config.cameraFolder;
        cameraCSVField.text = config.cameraCSV;

        lidarToggle.isOn = config.Lidar;
        cameraToggle.isOn = config.Camera;
    }

    public void SetLidarFolder(string folder)
    {
        config.lidarFolder = folder;
    }

    public void SetLidarCSV(string file)
    {
        config.lidarCSV = file;
    }

    public void SetCameraFolder(string folder)
    {
        config.cameraFolder = folder;
    }

    public void SetCameraCSV(string file)
    {
        config.cameraCSV = file;
    }

    public void CheckFolders()
    {
        bool exist = true;

        if (config.Lidar)
        {
            exist = Directory.Exists(config.lidarFolder);
        }
        if (config.Camera)
        {
            exist = exist && Directory.Exists(config.cameraFolder);
        }

        if (exist)
        {
            OnDataCollectionStart.Invoke();
        }
        else
        {
            warningPanel.SetActive(true);
        }
    }
    //

    // Logic
    public bool IsListenLidar {
        get
        {
            return config.Lidar;
        }
        set
        {
            config.Lidar = value;
        }
    }
    public bool IsListenCamera
    {
        get
        {
            return config.Camera;
        }
        set
        {
            config.Camera = value;
        }
    }

    public void dropupdow()
    {
        if(settingsDropUnDown)
        {
            settingsDropUnDown = false;
            SettingsPanel.transform.Translate(new Vector2(0, -(Screen.height / 2) * 0.9f));
        }
        else
        {
            settingsDropUnDown = true;
            SettingsPanel.transform.Translate(new Vector2(0, (Screen.height / 2) * 0.9f));
        }
    }

    public void StartTesting()
    {
        config.Lidar = true;
        config.Camera = true;
        OnDataCollectionStart.Invoke();
    }
}

[CreateAssetMenu(menuName = "Data/Configuration/DataCollection")]
public class DataCollectionConfig : ScriptableObject
{
    public bool isDataCollection;

    public bool Lidar;
    public string lidarFolder;
    public string lidarCSV;

    public bool Camera;
    public string cameraFolder;
    public string cameraCSV;
}
