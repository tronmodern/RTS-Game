using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Planet : MonoBehaviour, IHoverable
{
    public string planetName;

    private Vector3 orbitCenter;
    private float orbitRadius;
    private float orbitSpeed;
    private float miningRate = 0.1f;
    public bool isMining = false;
    private Dictionary<ResourceType, int> planetResources = new Dictionary<ResourceType, int>();

    public void Initialize(Vector3 center, float radius)
    {
        orbitCenter = center;
        orbitRadius = radius;
        SpeedSetter();
        transform.position = orbitCenter + new Vector3(orbitRadius, 0, 0);
        transform.RotateAround(orbitCenter, Vector3.up, Random.Range(100,1000));

        planetName = PlanetNameGenerator.GenerateName();
        GenerateResources();
    }

    private void Update()
    {
        transform.RotateAround(orbitCenter, Vector3.up, orbitSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up * 3 * Time.deltaTime);
    }

    private void UpdateUI()
    {
        UIPlanetManager.Instance.resourceText.text = planetName;
    }

    public void UpdateResourceText()
    {
        if (UIPlanetManager.Instance.currentPlanet == this)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var resource in planetResources)
            {
                sb.AppendLine($"{resource.Key}: {resource.Value}");
            }
            UIPlanetManager.Instance.planetResourceText.text = sb.ToString();
        }      
    }

    private void GenerateResources()
    {
        planetResources[ResourceType.Titanium] = Random.Range(4000, 5000);
        planetResources[ResourceType.Copper] = Random.Range(4000, 5000);
        planetResources[ResourceType.Flint] = Random.Range(4000, 5000);
        planetResources[ResourceType.Lead] = Random.Range(4000, 5000);
    }

    public Dictionary<ResourceType, int> GetResources()
    {
        return planetResources;
    }

    public void StartMining()
    {
        if (!MiningShipsManager.Instance.shipsReturned)
        {
            if (isMining)
            {
                StopMining();
                return;
            }

            if (!MiningShipsManager.Instance.TrySendShip(this))
            {
                Debug.Log("Нет доступных кораблей");
                return;
            }

            isMining = true;
            StartCoroutine(MineResourcesOverTime());
        }
        else Debug.Log("Невозможно отправить корабли");
        
    }

    private IEnumerator MineResourcesOverTime()
    {
        while (isMining)
        {
            miningRate = SkillManager.Instance.miningRate;

            MiningSystem.instance.MineResourceFromPlanet(this, ResourceType.Titanium, 4);
            MiningSystem.instance.MineResourceFromPlanet(this, ResourceType.Copper, 5);
            MiningSystem.instance.MineResourceFromPlanet(this, ResourceType.Flint, 3);
            MiningSystem.instance.MineResourceFromPlanet(this, ResourceType.Lead, 2);

            UpdateResourceText();

            yield return new WaitForSeconds(miningRate);
        }
    }

    public void StopMining()
    {
        if (!isMining) return;

        isMining = false;
        MiningShipsManager.Instance.ReturnShip(this);
    }

    public bool MineResource(ResourceType type, int amount)
    {
        if(planetResources.ContainsKey(type) && planetResources[type] >= amount)
        {
            planetResources[type] -= amount;
            return true;
        }
        return false;
    }

    public void Visibility(bool visible)
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        Collider[] colliders = GetComponentsInChildren<Collider>();
        TrailRenderer trail = GetComponentInChildren<TrailRenderer>();

        foreach (var renderer in renderers)
            renderer.enabled = visible;
                     

        foreach (var collider in colliders)
            collider.enabled = visible;

        SetTrailVisibility(trail, visible);
    }
    private void SetTrailVisibility(TrailRenderer trail, bool isVisible)
    {
        Color startColor = trail.startColor;
        Color endColor = trail.endColor;

        if (isVisible)
        {
            startColor.a = 1f;
            endColor.a = 0f;
        }
        else
        {
            startColor.a = 0f;
            endColor.a = 0f;
        }

        trail.startColor = startColor;
        trail.endColor = endColor;
    }


    private void SpeedSetter()
    {
        if (orbitRadius < 200) orbitSpeed = 5f;
        else if (orbitRadius < 300) orbitSpeed = 4f;
        else if (orbitRadius < 400) orbitSpeed = 3f;
        else if (orbitRadius < 500) orbitSpeed = 2f;
        else if (orbitRadius < 600) orbitSpeed = 1f;
        else if (orbitRadius < 700) orbitSpeed = 0.8f;
        else if (orbitRadius < 800) orbitSpeed = 0.6f;
        else if (orbitRadius < 900) orbitSpeed = 0.4f;
        else orbitSpeed = 0.2f;
        
    }

    public void AddTrail(GameObject planet)
    {
        TrailRenderer trail = planet.AddComponent<TrailRenderer>();

        trail.time = 10f;
        trail.startWidth = 7f;
        trail.endWidth = 1f;
        trail.minVertexDistance = 0.1f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = new Color(0, 150, 255, 0.8f);
        trail.endColor = new Color(1f, 1f, 150f, 0f);
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;
    }

    private void OnMouseDown()
    {
       if (UIPlanetManager.Instance.currentPlanet == this) return;

       UIPlanetManager.Instance.ShowMenu(this.transform, this);
       UIPlanetManager.Instance.planetNameText.text = "";
       UIPlanetManager.Instance.planetNameText.text = "Имя:  " + planetName;
    }
    private void OnEnable()
    {
        MiningShipsManager.Instance.OnDockForceRemoved += StopMiningExternally;
    }

    private void OnDisable()
    {
        MiningShipsManager.Instance.OnDockForceRemoved -= StopMiningExternally;
    }

    public void StopMiningExternally()
    {
        UIPlanetManager.Instance.mineButton.GetComponentInChildren<TextMeshProUGUI>().text = "SEND SHIP";
        Debug.Log("Добыча остановлена: док-станция/радар были уничтожены");
        StopMining();
        
    }

    public void OnHoverStay()
    {
        UIPlanetManager.Instance.ShowTitleMenu(transform, this.gameObject);
    }

    public void OnHoverEnter()
    {
        UpdateUI();
    }

    public void OnHoverExit()
    {
        UIPlanetManager.Instance.HideTitleMenu();
    }
}
