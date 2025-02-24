using System.Collections.Generic;
using UnityEngine;

public class MonitorManager : MonoBehaviour
{
    [SerializeField] private CommandProcessor commandProcessor;
    [SerializeField] private CameraFeedController cameraController;
    [SerializeField] private MapCreator mapCreator;

    private int currentAccessLevel;
    private Dictionary<string, InteractiveMapObject> mapObjects = new Dictionary<string, InteractiveMapObject>();

    private void Start()
    {
        InitializeMonitor();
    }

    private void InitializeMonitor()
    {
        // Initialize all subsystems
        commandProcessor.Initialize(currentAccessLevel);
    }

    public void CreateMap()
    {

       
        mapCreator.CreateAllMapObjects(FindObjectsByType<BaseObjectData>(sortMode: FindObjectsSortMode.None));
    }

    public void RegisterMapObject(InteractiveMapObject mapObject)
    {
        if (!mapObjects.ContainsKey(mapObject.ObjectId))
        {
            mapObjects.Add(mapObject.ObjectId, mapObject);
        }
    }

    public InteractiveMapObject GetMapObjectById(string id)
    {
        if (mapObjects.TryGetValue(id, out InteractiveMapObject mapObject))
        {
            return mapObject;
        }
        return null;
    }

    public void UpdateAccessLevel(int newLevel)
    {
        currentAccessLevel = newLevel;
    }
}