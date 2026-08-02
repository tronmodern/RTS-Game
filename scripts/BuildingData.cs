using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuildingData
{
    public Vector3 positon;
    public Quaternion rotation;
    public string type;
    public string tag;


    public float health;
    public float buildTime;
    public bool isBuilt;
    public bool isUnderAttack;
    public float resourceOutput;
}

[Serializable]
public class BuildingSaveData
{
    public List<BuildingData> buildings = new List<BuildingData>();
}