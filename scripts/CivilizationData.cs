using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Civilization", menuName = "Game/Civilization")]
public class CivilizationData : ScriptableObject
{
    public string civName;
    public bool canCommunicate;

    [Header("Боевые единицы")]
    public List<GameObject> enemyPrefabs;

    [Header("Паттерн спавна")]
    public EnemySpawnPattern spawnPattern;
}
