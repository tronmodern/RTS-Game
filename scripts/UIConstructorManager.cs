using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIConstructorManager : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject buildingPanel;
    public Button deleteButton;
    public Button closeButton;
    public Button buildButton;
    public Button exitBuildingButton;
    public Slider hpBar;
    public TextMeshProUGUI healIcon;

    public Button constructorButton;
    public OrbitalConstructor constructorIsExist;

    [SerializeField] private Button dockButton;
    [SerializeField] private Button armoryButton;
    [SerializeField] private Button factoryButton;
    [SerializeField] private Button laboratoryButton;
    [SerializeField] private Button radarButton;
    [SerializeField] private Button turretButton;
    [SerializeField] private Button plasmaTurretButton;
    [SerializeField] private Button lazerTurretButton;

    private OrbitalConstructor currentConstructor;
    private OrbitalConstructor buildingModeConstructor;
    private bool openedFromDysStation = false;
    private IDeletableStantion currentStantion;
    private Building currentViewedBuilding;

    [SerializeField] private BuildingManager buildingManager;

    [SerializeField] private TextMeshProUGUI warningText;
    private bool isActiveWarning;

    public static event Action OnBuildingModeEntered;
    public static event Action OnBuildingModeExit;

    public static bool isInBuildingMode = false;
    public static UIConstructorManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;

        Building.OnAnyBuildingDestroyed += HandleBuildingDestroyed;
        SkillManager.Instance.OnDockMaxChanged += ConstructorButtonSwitcher;
        Building.OnBuildingHpChanged += HpBarUpdate;
        Building.OnBuildingHpChanged += HealIconUpdate;
    }
    public void ShowMenu(OrbitalConstructor orbitalConstructor)
    {
        openedFromDysStation = false;
        currentConstructor = orbitalConstructor;
        currentViewedBuilding = orbitalConstructor;

        UIStationManager.Instance.ShowMenu(menuPanel, orbitalConstructor);

        HpBarUpdate(currentViewedBuilding);
        HealIconUpdate(currentViewedBuilding);

        deleteButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
        buildButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(DeleteCurrentBuilding);
        closeButton.onClick.AddListener(HideMenu);
        buildButton.onClick.AddListener(EnterBuildingMenu);

        deleteButton.gameObject.SetActive(false);

        EnableAllButtons();
    }

    public void HpBarUpdate(Building building)
    {
        if (building == currentViewedBuilding)
        {
            hpBar.maxValue = building.maxHealth;
            hpBar.value = building.health;
        }     
    }

    public void HealIconUpdate(Building building)
    {
        if (healIcon == null) return;
        if (building != currentViewedBuilding) return;
        healIcon.gameObject.SetActive(building.isHealing);
    }

    private void HandleBuildingDestroyed(Building building)
    {
        if (!isInBuildingMode) return;

        ConstructorButtonSwitcher();
    }

    public void ShowMenuDysStation(DysStation dysStation)
    {
        openedFromDysStation = true;
        DisableAllUnecessary();
        EnterBuildingMenu();
    }

    private void DisableAllUnecessary()
    {
        constructorIsExist = FindFirstObjectByType<OrbitalConstructor>();

        dockButton.interactable = false;
        armoryButton.interactable = false;
        factoryButton.interactable = false;
        laboratoryButton.interactable = false;
        radarButton.interactable = false;
        turretButton.interactable = false;      
        plasmaTurretButton.interactable = false;
        lazerTurretButton.interactable = false;
    }
    private void EnableAllButtons()
    {          
        if (SkillManager.Instance.millitarySkill) armoryButton.interactable = true;
        factoryButton.interactable = true;
        turretButton.interactable = true;
    }

    public void ConstructorButtonSwitcher()
    {
        UIConstructorManager.Instance.constructorIsExist = FindFirstObjectByType<OrbitalConstructor>();

        if (UIConstructorManager.Instance.constructorIsExist != null)
        {
            constructorButton.interactable = false;
        }
        else constructorButton.interactable = true;

        RadarStation radar = FindFirstObjectByType<RadarStation>();

        if (radar != null) radarButton.interactable = false;
        else if (!openedFromDysStation) radarButton.interactable = true;

        LaborotoryStation lab = FindFirstObjectByType <LaborotoryStation>();

        if (lab != null) laboratoryButton.interactable = false;
        else if (!openedFromDysStation) laboratoryButton.interactable= true;
        
        if (MiningShipsManager.Instance.dockShips + MiningShipsManager.Instance.dockShipsInConstruction >= SkillManager.Instance.docksMaxCount)
            dockButton.interactable = false;
        else if (!openedFromDysStation) dockButton.interactable = true;

        Armory armory = FindFirstObjectByType<Armory>();

        if (armory != null && armory.isBuilt && SkillManager.Instance.plasmaSkill && !openedFromDysStation) plasmaTurretButton.interactable = true;
        else if (openedFromDysStation || armory == null) plasmaTurretButton.interactable = false;

        if (armory != null && armory.isBuilt && SkillManager.Instance.lazerSkill && !openedFromDysStation) lazerTurretButton.interactable = true;
        else if (openedFromDysStation || armory == null) lazerTurretButton.interactable = false;
    }

    public void HideMenu()
    {
        UIStationManager.Instance.HideCurrentMenu();
    }

    private void DeleteCurrentBuilding()
    {
        if (currentStantion != null)
        {
            currentStantion.Delete();
            deleteButton.gameObject.SetActive(false);
            StartCoroutine(WaitForConstrucorScan());
        }
    }

    private IEnumerator WaitForConstrucorScan()
    {
        yield return null;
        ConstructorButtonSwitcher();
    }

    public void EnterBuildingMenu()
    {
        
        HideMenu();
        isInBuildingMode = true;

        if (!openedFromDysStation)
            buildingModeConstructor = currentConstructor;
        else
            buildingModeConstructor = null;

        ConstructorButtonSwitcher();

        OnBuildingModeEntered?.Invoke();
        buildingPanel.SetActive(true);
        exitBuildingButton.gameObject.SetActive(true);

        exitBuildingButton.onClick.RemoveAllListeners();
        exitBuildingButton.onClick.AddListener(ExitBuildingMenu);
    }

    public void ExitBuildingMenu()
    {
        buildingPanel.SetActive(false);
        deleteButton.gameObject.SetActive(false);
        exitBuildingButton.gameObject.SetActive(false);
        isInBuildingMode = false;
        buildingModeConstructor = null;
        OnBuildingModeExit?.Invoke();
    }

    public bool IsInBuildingModeOf(OrbitalConstructor constructor)
    {
        return isInBuildingMode && buildingModeConstructor == constructor;
    }

    public void ShowDeleteButton(IDeletableStantion deletableStantion)
    {
        if (buildingManager.currentBuilding == null)
        {
            currentStantion = deletableStantion;

            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(DeleteCurrentBuilding);

            deleteButton.gameObject.SetActive(true);
        }
    }

    public void HideDeleteButton()
    {
        deleteButton.gameObject.SetActive(false);
    }

    public IEnumerator ShowWarning(string message)
    {
        if (!isActiveWarning)
        {
            isActiveWarning = true;
            warningText.text = message;
            warningText.gameObject.SetActive(true);

            yield return new WaitForSeconds(1.5f);

            warningText.gameObject.SetActive(false);
            isActiveWarning = false;
        }
    }
}
