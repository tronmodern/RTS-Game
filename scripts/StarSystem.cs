using System.Collections.Generic;
using UnityEngine;

public class StarSystem : ObstacleMarker
{
    public List<GameObject> planetPrefabs;
    public int minPlanets = 3;
    public int maxPlanets = 7;
    public float orbitSpacing;
    public Vector3 starPosition;

    [SerializeField] private List<GameObject> spawnedPlanets = new List<GameObject>();

    private void Start()
    {
        GeneratePlanets();
        starPosition = this.transform.position;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * 1 * Time.deltaTime);
    }

    private void GeneratePlanets()
    {
        int planetCount = Random.Range(minPlanets, maxPlanets);
        List<float> orbits = new List<float>() { 100f };

        for (int i = 0; i < planetCount; i++)
        {
            if (planetPrefabs.Count == 0) return;

            float orbitSpacing;
            bool isValidOrbit;

            do
            {
                orbitSpacing = Random.Range(200f, 1400f);
                isValidOrbit = true;

                foreach (float existingOrbit in orbits)
                {
                    if (Mathf.Abs(orbitSpacing - existingOrbit) < 100f)
                    {
                        isValidOrbit = false;
                        break;
                    }
                }
            } while (!isValidOrbit);

            GameObject planetPrefab = planetPrefabs[Random.Range(0, planetPrefabs.Count)];
            GameObject planet = Instantiate(planetPrefab);
            planet.transform.SetParent(this.transform);

            planet.transform.position = transform.position + new Vector3(orbitSpacing, 0, 0);
            orbits.Add(orbitSpacing);

            Planet planetScript = planet.GetComponent<Planet>();
            planetScript.AddTrail(planet);
            planetScript.Visibility(false);
            planetScript.Initialize(transform.position, orbitSpacing);
            spawnedPlanets.Add(planet);
        }
    }

    public void Visibility(bool starVisible, bool planetsVisible)
    {
        foreach (var planet in spawnedPlanets)
        {
            planet.GetComponent<Planet>().Visibility(planetsVisible);
        }


        MeshRenderer[] starRenderers = GetComponents<MeshRenderer>();
        Collider starCollider = GetComponent<MeshCollider>();
        Collider starMapCollider = GetComponent<SphereCollider>();

        foreach (var renderer in starRenderers)
            renderer.enabled = starVisible;


        starCollider.enabled = starVisible;
        if (MapModeManager.Instance.isMapMode)
        {
            starMapCollider.enabled = true;
            starCollider.enabled = false;
        }
        else if (starVisible && !MapModeManager.Instance.isMapMode)
        {
            starMapCollider.enabled = false;
            starCollider.enabled = true;
        }
    }

    private void OnMouseEnter()
    {
        DistanceVisualizer.Instance.SetTarget(this.transform);
    }

    private void OnMouseExit()
    {
        DistanceVisualizer.Instance.SetTarget(null);
        DistanceVisualizer.Instance.Hide();
    }
}
