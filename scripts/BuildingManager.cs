using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private GameObject dockPrefab;
    [SerializeField] private GameObject constructorPrefab;
    [SerializeField] private GameObject armoryPrefab;
    [SerializeField] private GameObject factoryPrefab;
    [SerializeField] private GameObject laboratoryPrefab;
    [SerializeField] private GameObject radarPrefab;
    [SerializeField] private GameObject turret1Prefab;
    [SerializeField] private GameObject plasmaTurretPrefab;
    [SerializeField] private GameObject lazerTurretPrefab;
    [SerializeField] private Material hintMaterial;
    [SerializeField] private Material selectionMaterial;
    [SerializeField] private LayerMask mapLayerMask;
    [SerializeField] private GameObject buildingArea; // объект для слоя Map
    public float mapSize;
    [SerializeField] private SaveLoadManager saveLoadManager;
    [SerializeField] private TooltipManager tooltipManager;

    private Dictionary<string, GameObject> buildingPrefabs;
    private GameObject buildingPrefab; // текущий префаб
    private GameObject selectedBuilding;
    private GameObject tintedBuilding;

    private float[] ringRadii = { 140f, 160f, 180f, 200f, 220f, 240f, 260f, 280f };
    [SerializeField] private float angleStep = 30f;

    public GameObject currentBuilding; // Призрак здания
    public bool isPlacingBuilding = false;
    public GameObject buildingInitiator;
    private ObjectHighlighter objectHighlighter;
    public Button dockButton;
    public Button dysConstructorButton;
    public Button constructorButton;
    public Button armoryButton;
    public Button factoryButton;
    public Button laboratoryButton;
    public Button radarButton;
    public Button turretButton;
    public Button lazerTurretButton;
    public Button plasmaTurretButton;
    public Button exitBuildingButton;

    private void Start()
    {
        buildingArea.SetActive(false);

        buildingPrefabs = new Dictionary<string, GameObject>
        {
            { "dock(Clone)", dockPrefab},
            { "armory(Clone)", armoryPrefab},
            { "constructor(Clone)", constructorPrefab},
            { "fabric(Clone)", factoryPrefab},
            { "laboratory(Clone)", laboratoryPrefab},
            { "radar(Clone)", radarPrefab},
            { "turret1Prefab(Clone)", turret1Prefab},
            { "plasmaTurretPrefab(Clone)", plasmaTurretPrefab},
            { "lazerTurretPrefab(Clone)", lazerTurretPrefab}
        };

        AddButtonTooltip(dockButton, "Док-станция", "150 Титан, 75 Кремень");
        AddButtonTooltip(armoryButton, "Оружейный завод", "100 Титан, 30 Кремень");
        AddButtonTooltip(constructorButton, "Орбитальный конструктор", "100 Титан");
        AddButtonTooltip(factoryButton, "Фабрика", "100 Титан, 50 Кремень");
        AddButtonTooltip(laboratoryButton, "Лаборатория", "50 Титан, 100 Кремень");
        AddButtonTooltip(radarButton, "Радар", "75 Титан, 50 Свинец");
        AddButtonTooltip(turretButton, "Турель", "200 Титан, 200 Свинец");
        AddButtonTooltip(plasmaTurretButton, "Лазер. Турель", "200 Титан, 200 Свинец, 1 Убер Ядро");
        AddButtonTooltip(plasmaTurretButton, "Плазм. Турель", "200 Титан, 200 Свинец, 1 Убер Ядро");

        objectHighlighter = new ObjectHighlighter(hintMaterial, selectionMaterial);
        LoadGame();

        constructorButton.onClick.AddListener(() => SelectBuilding(constructorPrefab));
        dockButton.onClick.AddListener(() => SelectBuilding(dockPrefab));
        armoryButton.onClick.AddListener(() => SelectBuilding(armoryPrefab));
        factoryButton.onClick.AddListener(() => SelectBuilding(factoryPrefab));
        laboratoryButton.onClick.AddListener(() => SelectBuilding(laboratoryPrefab));
        radarButton.onClick.AddListener(() => SelectBuilding(radarPrefab));
        turretButton.onClick.AddListener(() => SelectBuilding(turret1Prefab));
        lazerTurretButton.onClick.AddListener(() => SelectBuilding(lazerTurretPrefab));
        plasmaTurretButton.onClick.AddListener(() => SelectBuilding(plasmaTurretPrefab));
    }

    private void Update()
    {
        if (!isPlacingBuilding)
        {
            HandleHighlighting();
        }

        if (isPlacingBuilding)
        {
            objectHighlighter.ResetSelection();
            FollowMousePosition();

            if (Input.GetMouseButtonDown(0))
            {
                PlaceBuilding();
            }

            CancelBuilding();
        }
    }

    public void SelectBuilding(GameObject selectedBuildingPrefab)
    {
        buildingPrefab = selectedBuildingPrefab;
        StartPlacement();
    }

    public void BuildingInitiatorHandler(GameObject constructor)
    {
        buildingInitiator = constructor;
    }

    private void StartPlacement()
    {
        if (currentBuilding == null)
        {
            buildingArea.SetActive(true);
            Cursor.visible = false;
            currentBuilding = Instantiate(buildingPrefab);
            var renderer = currentBuilding.GetComponentInChildren<Renderer>();
            renderer.material.color = new Color(0, 1, 0, 0.5f);
            isPlacingBuilding = true;
        }
    }

    private void PlaceBuilding()
    {
        var renderer = currentBuilding.GetComponent<Renderer>();

        if (renderer.material.color == new Color(0, 1, 0, 0.5f))
        {
            Building buildingComponent = currentBuilding.GetComponent<Building>();
            if (!ResourceManager.Instance.CanSpend(buildingComponent.constructionCost))
            {
                StartCoroutine(UIConstructorManager.Instance.ShowWarning("Недостаточно ресурсов"));
                return;
            }

            ResourceManager.Instance.SpendResource(buildingComponent.constructionCost);

            Vector3 pos = currentBuilding.transform.position;
            Quaternion rot = currentBuilding.transform.rotation;

            Destroy(currentBuilding);
            GameObject newBuilding = Instantiate(buildingPrefab, pos, rot);

            if (newBuilding.GetComponent<DockStation>() != null)
            {
                MiningShipsManager.Instance.dockShipsInConstruction++;
                UIConstructorManager.Instance.ConstructorButtonSwitcher();
            }

            StartCoroutine(BuildRoutine(newBuilding));


            if (currentBuilding == constructorPrefab)
            {
                UIConstructorManager.Instance.ConstructorButtonSwitcher();
            }

            buildingArea.SetActive(false);
            currentBuilding = null;
            isPlacingBuilding = false;
            Cursor.visible = true;  
        }
    }

    IEnumerator BuildRoutine(GameObject building)
    {
        var renderer = building.GetComponent<Renderer>();

        renderer.material.color = Color.yellow;

        Building b = building.GetComponent<Building>();
        b.isBuilt = false;
        b.tag = "Building";

        yield return new WaitForSeconds(b.buildTime * SkillManager.Instance.constructionTimeMultiplier);
        if (b != null)
        {
            renderer.material.color = Color.white;
            b.isBuilt = true;
            b.Build();

            if (b is DockStation)
            {
                MiningShipsManager.Instance.dockShipsInConstruction--;
            }
        }
        
    }

    private void FollowMousePosition()
    {
        if (currentBuilding == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, mapLayerMask))
        {
            Vector3 position = hit.point;
            float distanceToCenter = Vector3.Distance(Vector3.zero, position);

            Vector3 snappedPosition = GetSnappedPosition(position);

            currentBuilding.transform.position = snappedPosition;
            currentBuilding.transform.rotation = Quaternion.LookRotation(Vector3.zero - snappedPosition);

            Vector3 buildingSize = currentBuilding.GetComponent<Renderer>().bounds.size;
            Collider[] hitColliders = Physics.OverlapBox(snappedPosition, buildingSize / 2, Quaternion.identity);

            bool isColliding = false;
            foreach (var collider in hitColliders)
            {
                if (collider.CompareTag("Building") || collider.CompareTag("Obstacle") || collider.CompareTag("Star"))
                {
                    isColliding = true;
                    break;
                }
            }

            var renderer = currentBuilding.GetComponent<Renderer>();
            renderer.material.color = isColliding ? Color.red : new Color(0, 1, 0, 0.5f);
        }
    }

    public void CancelBuilding()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && currentBuilding != null)
        {
            Destroy(currentBuilding);
            buildingArea.SetActive(false);
            isPlacingBuilding = false;
            buildingPrefab = null;
            Cursor.visible = true; 
        }
    }

    public void CancelBuildingExternally(GameObject caller)
    {
        if (currentBuilding != null && buildingInitiator == caller)
        {
            Destroy(currentBuilding);
            buildingArea.SetActive(false);
            isPlacingBuilding = false;
            buildingPrefab = null;
            Cursor.visible = true;
        }
    }

    public void ExitCancelBuilding()
    {
        if (currentBuilding != null)
        {
            Destroy(currentBuilding);
            buildingArea.SetActive(false);
            isPlacingBuilding = false;
            buildingPrefab = null;
            Cursor.visible = true;
        }
    }

    private float GetNearestRingRadius(Vector3 position)
    {
        float minDistance = float.MaxValue;
        float nearestRadius = ringRadii[0];

        float distanceToCenter = Vector3.Distance(Vector3.zero, position);
        foreach (float radius in ringRadii)
        {
            float distance = Mathf.Abs(distanceToCenter - radius);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestRadius = radius;
            }
        }
        return nearestRadius;
    }

    private Vector3 GetSnappedPosition(Vector3 position)
    {
        float nearestRing = GetNearestRingRadius(position);

        float angle = Mathf.Atan2(position.z, position.x) * Mathf.Rad2Deg;
        angle = Mathf.Repeat(angle, 360);
        float snappedAngle = Mathf.Round(angle / angleStep) * angleStep;

        float radians = snappedAngle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(radians) * nearestRing, 0, Mathf.Sin(radians) * nearestRing);
    }

    public void SaveGame()
    {
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("Building"); // багуется если сохранить предустановленное
        List<GameObject> buildingList = new List<GameObject>(buildings);

        saveLoadManager.SaveBuildings(buildingList);
    }

    public void DeleteSave()
    {
        saveLoadManager.DeleteSaveFile();
    }

    private void LoadGame()
    {
        List<BuildingData> loadedBuildings = saveLoadManager.LoadBuildings();
        if (loadedBuildings != null)
        {
            foreach (BuildingData data in loadedBuildings)
            {
                if (buildingPrefabs.ContainsKey(data.type)) // проверяем, есть ли такой тип здания
                {
                    GameObject buildingPrefab = buildingPrefabs[data.type]; // получаем префаб для этого типа
                    if (buildingPrefab != null)
                    {
                        GameObject building = Instantiate(buildingPrefab, data.positon, data.rotation);
                        building.name = data.type;
                        building.tag = data.tag;

                        Building buildingComponent = building.GetComponent<Building>();
                        if (buildingComponent != null)
                        {
                            buildingComponent.health = data.health;
                            buildingComponent.buildTime = data.buildTime;
                            buildingComponent.isBuilt = data.isBuilt;
                            buildingComponent.isUnderAttack = data.isUnderAttack;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Префаб для здания не найден: " + data.type);
                }
            }
        }
    }

    private void AddButtonTooltip(Button button, string buildingName, string cost)
    {
        EventTrigger eventTrigger = button.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = button.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry pointerEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        pointerEnter.callback.AddListener((eventData) => tooltipManager.ShowTooltip(buildingName, cost, button.transform.position));

        EventTrigger.Entry pointerExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        pointerExit.callback.AddListener((eventData) => tooltipManager.HideToolTip());

        eventTrigger.triggers.Add(pointerEnter);
        eventTrigger.triggers.Add(pointerExit);
    }

    private void HandleHighlighting()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit) && !EventSystem.current.IsPointerOverGameObject();

        if (!UIConstructorManager.isInBuildingMode)
        {
            if (hitSomething)
            {
                GameObject target = hit.collider.gameObject;
                Transform transformTarget = target.transform;
                objectHighlighter.HighlightSelection(target);             
            }
            else
            {
                objectHighlighter.ResetSelection();
            }
        }
        else // режим строительства
        {
            if (hitSomething)
            {
                GameObject target = hit.collider.gameObject;

                if (selectedBuilding == null || selectedBuilding != target)
                {
                    if (tintedBuilding != target)
                    {
                        objectHighlighter.ResetHint();
                        tintedBuilding = target;
                        objectHighlighter.HighlightHint(target);
                    }
                }
            }
            else if (tintedBuilding != null)
            {
                objectHighlighter.ResetHint();
                tintedBuilding = null;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (hitSomething)
                {
                    GameObject target = hit.collider.gameObject;
                    if (selectedBuilding != target)
                    {
                        objectHighlighter.ResetSelection();
                        selectedBuilding = target;
                        objectHighlighter.HighlightSelection(target);
                    }   
                }
                else
                {
                    objectHighlighter.ResetSelection();
                    selectedBuilding = null;
                }

                if (!hitSomething || !hit.collider.CompareTag("Building"))
                {
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        UIConstructorManager.Instance.HideDeleteButton();
                    }
                }
            }
        }
    }
}
