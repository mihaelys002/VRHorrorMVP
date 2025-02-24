using Codice.Client.Common;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapCreator : MonoBehaviour
{
    [SerializeField] private Transform doorsContainer;
    [SerializeField] private Transform turretsContainer;
    [SerializeField] private Transform camerasContainer;
    [SerializeField] private Transform trapsContainer;

    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private GameObject turretPrefab;
    [SerializeField] private GameObject cameraPrefab;
    [SerializeField] private GameObject trapPrefab;

    [SerializeField] private MonitorManager monitorManager;
    [SerializeField] private CameraFeedController feedController;

    public void CreateAllMapObjects(BaseObjectData[] buildingData)
    {
        CreateDoors(buildingData.OfType<DoorData>());
        CreateCameras(buildingData.OfType<SecurityCameraData>());
    }



    private void CreateDoors(IEnumerable<DoorData> doorDataList)
    {
        foreach (var doorData in doorDataList)
        {
            GameObject doorObj = Instantiate(doorPrefab, doorsContainer);
            DoorMapObject doorMapObject = doorObj.GetComponent<DoorMapObject>();
            doorMapObject.Initialize(doorData);
            monitorManager.RegisterMapObject(doorMapObject);
        }
    }

    private void CreateCameras(IEnumerable<SecurityCameraData> cameraDataList)
    {
        foreach (var cameraData in cameraDataList)
        {
            GameObject cameraObj = Instantiate(cameraPrefab, camerasContainer);
            CameraMapObject cameraMapObject = cameraObj.GetComponent<CameraMapObject>();
            cameraMapObject.Initialize(cameraData);
            monitorManager.RegisterMapObject(cameraMapObject);
        }
    }

    // Similar methods for other object types
}