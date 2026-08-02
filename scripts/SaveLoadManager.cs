using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveLoadManager : MonoBehaviour
{
    private BuildingHandler buildingDataHandler;

    void Start()
    {
        buildingDataHandler = new BuildingHandler();
    }

    public void SaveBuildings(List<GameObject> buildingObjects)
    {
        buildingDataHandler.Save(buildingObjects);
    }

    public List<BuildingData> LoadBuildings()
    {
        return buildingDataHandler.Load();
    }

    public void DeleteSaveFile()
    {
        buildingDataHandler.Delete();
    }
}
