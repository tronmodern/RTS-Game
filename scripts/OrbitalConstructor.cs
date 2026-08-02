using UnityEngine;
using UnityEngine.EventSystems;

public class OrbitalConstructor : Building, IDeletableStantion
{
    private BuildingManager buildingManager;

    private void Awake()
    {
        buildingManager = FindFirstObjectByType<BuildingManager>();
    }

    public override void Build()
    {
        base.Build();
        Debug.Log(" Orbital Constructor is building...");   
    }

    public void OnMouseDown()
    {
        if (isBuilt == true && !UIConstructorManager.isInBuildingMode && !EventSystem.current.IsPointerOverGameObject())
        {
            Object.FindFirstObjectByType<UIConstructorManager>().ShowMenu(this);
            buildingManager.BuildingInitiatorHandler(this.gameObject);
        }
        
        if (isBuilt && UIConstructorManager.isInBuildingMode && !EventSystem.current.IsPointerOverGameObject())
        {
            UIConstructorManager.Instance.ShowDeleteButton(this);
        }
    }
    
    public void Delete()
    {
        Destroy(gameObject);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (buildingManager != null && buildingManager.isPlacingBuilding)
        {
            buildingManager.CancelBuildingExternally(this.gameObject);
        }

        if (UIConstructorManager.Instance != null)
        {
            if (UIConstructorManager.Instance.IsInBuildingModeOf(this))
            {
                UIConstructorManager.Instance.ExitBuildingMenu();
            }
        }    
            
    }
}
