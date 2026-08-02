using System.Collections.Generic;
using UnityEngine;

public class EnemySignalManager : MonoBehaviour
{
    public Transform baseTarget;
    public List<CivilizationData> civilization;
    public GameObject signalPrefab;
    public RectTransform signalsParent;
    private List<EnemySignal> activeSignals = new List<EnemySignal>();

    public void SpawnSignal(Vector3 startPos, CivilizationData civ, float travelTime)
    {
        GameObject obj = Instantiate(signalPrefab, startPos, Quaternion.identity);
        EnemySignal signal = obj.GetComponent<EnemySignal>();
        signal.InitSingal(civ, baseTarget, travelTime);
        signal.OnArrive += HandleSignalArrive;
        activeSignals.Add(signal);
    }

    private void HandleSignalArrive(EnemySignal signal)
    {
        SpawnEnemies(signal.civilization);
        activeSignals.Remove(signal);
        Destroy(signal.gameObject);
    }

    private void SpawnEnemies(CivilizationData civ)
    {
        if (civ.spawnPattern != null)
        {
            
            StartCoroutine(civ.spawnPattern.ExecuteSpawn(civ, baseTarget));
            Debug.Log($"Спавним {civ.name}");
        } 
        else 
            Debug.LogWarning($"У цивилизации {civ.civName} не задан паттерн спавна!");

    }
}