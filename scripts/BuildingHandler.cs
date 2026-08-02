using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BuildingHandler
{
    private string saveFilePath;

    public BuildingHandler()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "game_data.json");
    }

    public void Save(List<GameObject> buildings)
    {
        BuildingSaveData saveData = new BuildingSaveData();
        foreach (var building in buildings)
        {
            if (building != null)
            {
                Building buildingComponent = building.GetComponent<Building>();
                if (buildingComponent != null)
                {
                    BuildingData data = new BuildingData
                    {
                        positon = building.transform.position,
                        rotation = building.transform.rotation,
                        type = building.name,
                        tag = building.tag,

                        health = buildingComponent.health,
                        buildTime = buildingComponent.buildTime,
                        isBuilt = buildingComponent.isBuilt,
                        isUnderAttack = buildingComponent.isUnderAttack,
                    };
                    saveData.buildings.Add(data);
                }
            }
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Сохранение завершено! Путь: " + saveFilePath);
    }

    public List<BuildingData> Load()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            BuildingSaveData saveData = JsonUtility.FromJson<BuildingSaveData>(json);
            Debug.Log("Загрузка завершена!");
            return saveData.buildings;
        }
        else
        {
            Debug.LogWarning("Файл сохранений не найден!");
            return null;
        }
    }

    public void Delete()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Сохранение удалено.");
        }
        else
        {
            Debug.LogWarning("Файл сохранений не существует!");
        }
    }
}
