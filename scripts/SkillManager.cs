using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    public int radarLevel = 1;
    public int docksMaxCount = 1;
    public bool millitarySkill = false;
    public bool lazerSkill = false;
    public bool plasmaSkill = false;
    public float miningRate = 0.5f;
    public float lazerDamage = 1f;
    public float railgunRange = 200f;
    public float buildingMaxHealthMultiplier = 1f;
    public float productionTime = 30f;
    public float constructionTimeMultiplier = 1f;

    public System.Action<float> OnTurretDamageChanged;
    public System.Action<float> OnBuildingMaxHealthChanged;
    public System.Action<float> OnTurretRangeChanged;
    public System.Action<float> OnProductionTimeChanged;
    public System.Action OnDockMaxChanged;

    void Awake()
    {
        Instance = this;
    }

    public void UpgradeTurretDamage(float newDamage)
    {
        lazerDamage = newDamage;
        OnTurretDamageChanged?.Invoke(lazerDamage);
    }

    public void UpgradeBuidingHealth(float newHealth)
    {
        buildingMaxHealthMultiplier = newHealth;
        OnBuildingMaxHealthChanged?.Invoke(buildingMaxHealthMultiplier);
    }    

    public void UpgradeRailgunRange(float newRange)
    {
        railgunRange = newRange;
        OnTurretRangeChanged?.Invoke(railgunRange);
    }

    public void UpgradeProdTime(float newTime)
    {
        productionTime = newTime;
        OnProductionTimeChanged?.Invoke(productionTime);
    }

    public void UpgradeConstTime(float newMultiplier)
    {
        constructionTimeMultiplier = newMultiplier;
    }

    public void UpgradeDockMax(int newMax)
    {
        docksMaxCount = newMax;
        OnDockMaxChanged?.Invoke();
    }
}
