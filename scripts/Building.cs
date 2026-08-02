using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class Building : ObstacleMarker
{
    public string buildingName;
    public float health;
    public float maxHealth;
    public float minHealth;
    public Vector3 position;
    public float buildTime;
    public bool isBuilt = false;
    public bool isUnderAttack;
    public float timeDifference;
    public bool isHealing;

    private float lastDamageTime;
    private float healTickTimer;


    public static event Action<Building> OnAnyBuildingDestroyed;
    public static event Action<Building> OnBuildingHpChanged;

    [SerializeField]
    public List<ResourceCost> constructionCost;

    protected virtual void Start()
    {
        SetMaxHealth(SkillManager.Instance.buildingMaxHealthMultiplier);

        MapModeManager.Instance.MapModeEntered += UpdateVisibility;
        MapModeManager.Instance.StarModeEntered += UpdateVisibility;
        SkillManager.Instance.OnBuildingMaxHealthChanged += SetMaxHealth;
    }

    protected virtual void Update()
    {
        timeDifference = Time.time - lastDamageTime;

        if (health >= maxHealth)
        {
            if (isHealing)
            {
                isHealing = false;
                OnBuildingHpChanged?.Invoke(this);
            }
            return;
        }

        if (timeDifference < 20f)
        {
            isHealing = false;
            return;
        }

        isHealing = true;
        Heal(); 
    }

    private void Heal()
    {
        healTickTimer += Time.deltaTime;

        if (healTickTimer >= 1f)
        {
            healTickTimer = 0f;

            health += maxHealth * 0.005f;
            health = Mathf.Min(health, maxHealth);

            OnBuildingHpChanged?.Invoke(this);
        }
    }

    protected virtual void OnDestroy()
    {
        if (MapModeManager.Instance != null)
        {
            MapModeManager.Instance.MapModeEntered -= UpdateVisibility;
            MapModeManager.Instance.StarModeEntered -= UpdateVisibility;
        }

        SkillManager.Instance.OnBuildingMaxHealthChanged -= SetMaxHealth;

        OnAnyBuildingDestroyed?.Invoke(this);

        if (UIStationManager.Instance != null)
            UIStationManager.Instance.OnBuildingDestroyed(this);
    }

    public virtual void Build()
    {
        isBuilt = true;
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        lastDamageTime = Time.time;
        OnBuildingHpChanged.Invoke(this);
    }

    private void UpdateVisibility(bool _)
    {
        bool isBase = CameraManager.Instance.target == CameraManager.Instance.defaultTarget;
        bool shouldBeVisible = !MapModeManager.Instance.isMapMode &&
                               (!MapModeManager.Instance.isStarMode || isBase);

        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = shouldBeVisible;
        }
    }

    public void SetMaxHealth(float newMultiplier)
    {
        maxHealth = minHealth * newMultiplier;
        OnBuildingHpChanged.Invoke(this);
    }
}