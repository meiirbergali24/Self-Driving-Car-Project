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
        roadFolderField,
        roadCSVField;

    [SerializeField, Space]
    private Toggle lidarToggle;
    [SerializeField]
    private Toggle roadToggle;
    
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
        SetRoadFolder($"{defaultPath}/Road");
        SetRoadCSV($"{defaultPath}/road.csv");

        IsListenLidar = true;
        IsListenRoad = true;

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

        roadFolderField.text = config.roadFolder;
        roadCSVField.text = config.roadCSV;

        lidarToggle.isOn = config.Lidar;
        roadToggle.isOn = config.Road;
    }

    public void SetLidarFolder(string folder)
    {
        config.lidarFolder = folder;
    }

    public void SetLidarCSV(string file)
    {
        config.lidarCSV = file;
    }

    public void SetRoadFolder(string folder)
    {
        config.roadFolder = folder;
    }

    public void SetRoadCSV(string file)
    {
        config.roadCSV = file;
    }

    public void CheckFolders()
    {
        bool exist = true;

        if (config.Lidar)
        {
            exist = Directory.Exists(config.lidarFolder);
        }
        if (config.Road)
        {
            exist = exist && Directory.Exists(config.roadFolder);
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
    public bool IsListenRoad
    {
        get
        {
            return config.Road;
        }
        set
        {
            config.Road = value;
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
        config.Road = true;
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

    public bool Road;
    public string roadFolder;
    public string roadCSV;
}
