using System.Collections;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.EventSystems;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    
    public Transform target;
    public Transform defaultTarget;

    public float rotationSensitivity = 10f, zoomSensitivity = 10f;
    public float minYAngle = 10f, maxYAngle = 80f;

    [SerializeField] private float minDistance = 80f, maxDistance = 800f;
    [SerializeField] private float rotationSmoothTime = 1f;
    [SerializeField] private float zoomSmoothTime = 0.4f;
    [SerializeField] private float targetChangeTime = 0.5f;

    public float distance = 150f;
    public float targetDistance = 180f;
    private float yaw = 0f;
    private float pitch = 20f;

    private float yawVelocity = 0f;
    private float pitchVelocity = 0f;
    private float zoomVelocity = 0f; 

    private float targetYaw = 0f;
    private float targetPitch = 20f;

    private Vector3 currentTargetPosition;
    private bool isChangingTarget = false;
    private Vector3 targetChangeVelocity;

    private bool wasMapMode;

    private bool isOrbitMode = true;
    public float moveSpeed = 20f;
    public float boostMultiplier = 2f;
    public float rotationSpeed = 5f;
    public float panSpeed = 0.5f;
    public float zoomSpeed = 100f;
    public float minZoomDistance = 10f;
    public float maxZoomDistance = 600f;
    private float targetZoomOffset = 0f;
    private float currentZoomVelocity = 0f;
    private float minHeight = -50f;
    private float maxHeight = 500f;
    private float currentPitch = 45f;

    public Material mapRingMaterial;
    public Material mapBorderMaterial;

    public MapBorder mapBorder;
    public BuildingManager buildingManager;
    
    private void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        StarSystem starSystem = target != null ? target.GetComponent<StarSystem>() : null;
        MapModeManager.Instance.SetStarMode(true, starSystem);

        if (target == null) Debug.LogError("Не указан таргет камеры");

        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
        targetYaw = yaw;
        targetPitch = pitch;
    }

    void Update()
    {
        ChangeCameraMode();
        AvoidObstacles();

        if (isOrbitMode)
        {
            OrbitModeHandler();
        }
        else
        {
            IsometricModeHandler();           
        }
    }

    private void IsometricModeHandler()
    {
        float speed = Input.GetKey(KeyCode.LeftShift) ? moveSpeed * boostMultiplier : moveSpeed;

        Vector3 input = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0f,
            Input.GetAxisRaw("Vertical")
        );

        Vector3 move = input.normalized * speed * Time.deltaTime;
        Vector3 targetPos = transform.position + move;

        targetPos.x = Mathf.Clamp(targetPos.x, -600, 600);
        targetPos.y = Mathf.Clamp(targetPos.y, minHeight, maxHeight);
        targetPos.z = Mathf.Clamp(targetPos.z, -600, 600);

        transform.position = targetPos;

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            targetZoomOffset += scrollInput * zoomSpeed;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            currentPitch += 40f * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E))
        {
            currentPitch -= 40f * Time.deltaTime;
        }
        currentPitch = Mathf.Clamp(currentPitch, 20f, 80f);

        transform.rotation = Quaternion.Euler(currentPitch, 0, 0f);

        if (Mathf.Abs(targetZoomOffset) > 0.01f)
        {
            float zoomDelta = Mathf.SmoothDamp(0f, targetZoomOffset, ref currentZoomVelocity, zoomSmoothTime);
            Vector3 zoomDirection = transform.forward * zoomDelta;

            Vector3 nextPos = transform.position + zoomDirection;

            // Ограничения по всем осям
            nextPos.x = Mathf.Clamp(nextPos.x, -1200, 1200);
            nextPos.y = Mathf.Clamp(nextPos.y, minHeight, maxHeight);
            nextPos.z = Mathf.Clamp(nextPos.z, -1200, 1200);

            transform.position = nextPos;
            targetZoomOffset -= zoomDelta;
        }

        if (Input.GetMouseButtonDown(2))
        {
            StartCoroutine(ReturnToDefault2_5D());
        }
    }

    private IEnumerator ReturnToDefault2_5D()
    {
        Vector3 targetPosition = new Vector3(0, 200, -150);
   

        while (Vector3.Distance(transform.position, targetPosition) > 10f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
            yield return null;
        }

        transform.position = targetPosition;
    }

    void AvoidObstacles()
    {
        float safetyRadius = 30.0f;
        float safeMoveSpeed = 300.0f;

        Collider[] hits = Physics.OverlapSphere(transform.position, safetyRadius);

        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject && hit.GetComponent<ObstacleMarker>() != null)
            {
                transform.position += Vector3.up * safeMoveSpeed * Time.deltaTime;
                break;
            }
        }
    }


    private void OrbitModeHandler()
    {
        if (Input.GetMouseButton(1))
        {
            targetYaw += Input.GetAxis("Mouse X") * rotationSensitivity;
            targetPitch -= Input.GetAxis("Mouse Y") * rotationSensitivity;
            targetPitch = Mathf.Clamp(targetPitch, minYAngle, maxYAngle);
        }

        yaw = Mathf.SmoothDamp(yaw, targetYaw, ref yawVelocity, rotationSmoothTime);
        pitch = Mathf.SmoothDamp(pitch, targetPitch, ref pitchVelocity, rotationSmoothTime);

        if (target != null)
        {
            if (target == defaultTarget || target.CompareTag("Star"))
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll != 0)
                {
                    targetDistance = Mathf.Clamp(targetDistance - scroll * zoomSensitivity, minDistance, maxDistance);
                }
                distance = Mathf.SmoothDamp(distance, targetDistance, ref zoomVelocity, zoomSmoothTime);
            }
            else if (target.CompareTag("Building"))
            {
                distance = Mathf.SmoothDamp(distance, 20f, ref zoomVelocity, zoomSmoothTime);
            }
        }
        else
            ResetTarget();

        if (isChangingTarget && target != null)
        {
            currentTargetPosition = Vector3.SmoothDamp(currentTargetPosition, target.position, ref targetChangeVelocity, targetChangeTime);
            if (Vector3.Distance(currentTargetPosition, target.position) < 0.001f)
            {
                isChangingTarget = false;
            }
        }
        else
        {
            currentTargetPosition = target.position;
        }
        AdaptiveZoomVelocity();
        AdaptiveUIVisibility();
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        transform.position = currentTargetPosition - (rotation * Vector3.forward * distance);
        transform.LookAt(currentTargetPosition);
        ChangeTargetHandler();
    }

    private void ChangeCameraMode()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && !MapModeManager.Instance.isMapMode)
        {
            if (isOrbitMode)
            {
                transform.rotation = Quaternion.Euler(45f, 0f, 0f);
                transform.position = new Vector3(0f, 200f, -150f);
                isOrbitMode = false;
            }
            else isOrbitMode = true;
        }   
    }

    private void ChangeTargetHandler()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit) && !EventSystem.current.IsPointerOverGameObject();

        if (!UIConstructorManager.isInBuildingMode)
        {
            if (hitSomething)
            {
                GameObject targetObject = hit.collider.gameObject;
                Transform transformTarget = targetObject.transform;
                if (Input.GetMouseButtonDown(0))
                {
                    if (targetObject.CompareTag("Building") || targetObject.CompareTag("Star"))
                    {
                        ChangeTarget(transformTarget, targetObject);
                    }
                    else if (!MapModeManager.Instance.isMapMode && target != null && !target.CompareTag("Star"))
                    {
                        ResetTarget();
                    }
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0) && !MapModeManager.Instance.isMapMode && !target.CompareTag("Star"))
                {
                    ResetTarget();
                }
            }
        }
        else // режим строительства
        {
            ResetTarget();
        }
    }


    public void ChangeTarget(Transform newTarget, GameObject _newTarget)
    {
        if (newTarget == null || _newTarget == null) return;

        if (_newTarget.CompareTag("Building") || _newTarget.CompareTag("Star"))
        {
            if (target != newTarget)
            {
                target = newTarget;
                isChangingTarget = true;

                if (_newTarget.CompareTag("Star") && MapModeManager.Instance.isMapMode)
                {
                    distance = 2000000f;

                    targetDistance = 200;
                }
            }
            else if (target == defaultTarget && MapModeManager.Instance.isMapMode)
            {
                isChangingTarget = true;

                targetDistance = 200;
            }
        }
    }

    public void ResetTarget()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            target = defaultTarget;
            isChangingTarget = true;
        }
    }

    private void AdaptiveZoomVelocity()
    {
        if (UIConstructorManager.isInBuildingMode)
        {
            HandleBuildingMode();
            return;
        }

        if (isOrbitMode)
        {
            maxDistance = 50000;
        }
        
        StarSystem starSystem = target != null ? target.GetComponent<StarSystem>() : null;
        
        AdjustZoomSensitivity();
        if (!MapModeManager.Instance.isMapMode && target != null)
        {
            
            if (targetDistance > 5000)
            {
                EnterMapMode();
            }
        }
        else
        {
            
            if (targetDistance < 17500)
            {
                ExitMapMode();
                if (target.CompareTag("Star")) MapModeManager.Instance.SetStarMode(true, starSystem);      
            }
            else if (targetDistance > 5000 && target != defaultTarget)
            {
                target = defaultTarget;
                EnterMapMode();
            }
        }
    }

    private void AdjustZoomSensitivity()
    {
        if (targetDistance < 200f) zoomSensitivity = 15;
        else if (targetDistance < 300f) zoomSensitivity = 50;
        else if (targetDistance < 400f) zoomSensitivity = 100;
        else if (targetDistance < 500f) zoomSensitivity = 120;
        else if (targetDistance < 600f) zoomSensitivity = 150;
        else if (targetDistance < 5000f) zoomSensitivity = 1000;
        else if (targetDistance < 20000f) zoomSensitivity = 3000;
        else zoomSensitivity = 10000;
    }

    private void HandleBuildingMode()
    {
        maxDistance = 600;
        if (targetDistance > 600 || distance > 600) targetDistance = maxDistance;
        zoomSensitivity = 100;
    }

    private void EnterMapMode()
    {
        targetDistance = 18000;
        targetPitch = 80f;
        pitch = 80f;
        minYAngle = -20f;
        rotationSmoothTime = 1f;
        StartCoroutine(ChangeMapFOV(1f));
        MapModeManager.Instance.SetMapMode(true);
        
    }

    private void ExitMapMode()
    {
        targetDistance = 1500;
        targetPitch = 25f;
        minYAngle = 10f;
        StartCoroutine(ChangeMapFOV(2f));
        MapModeManager.Instance.SetMapMode(false);
    }

    private void AdaptiveUIVisibility()
    {
        float defaultRingAlpha = 0.37f; 
        float targetRingAlpha = 0.01f;
        float defaultBorderAlpha = 0.40f;
        float targetBorderAlpha = 0.05f;

        Color colorRing = mapRingMaterial.color;
        Color colorBorder = mapBorderMaterial.color;

        if (MapModeManager.Instance.isMapMode)
        {
            float t = Mathf.Clamp01(pitch / 20);

            float newRingAlpha = Mathf.Lerp(targetRingAlpha, defaultRingAlpha, t);
            float newBorderAlpha = Mathf.Lerp(targetBorderAlpha, defaultBorderAlpha, t);

            
            colorRing.a = newRingAlpha;
            colorBorder.a = newBorderAlpha;
            mapRingMaterial.color = colorRing;
            mapBorderMaterial.color = colorBorder;
        }
        else if (wasMapMode)
        {
            colorRing.a = defaultRingAlpha;
            colorBorder.a = defaultBorderAlpha;
            mapRingMaterial.color = colorRing;
            mapBorderMaterial.color = colorBorder;
        }
        wasMapMode = MapModeManager.Instance.isMapMode;
    }

    private IEnumerator ChangeMapFOV(float time)
    {
        //float targetFOV = 160f;
        //float velocity = 0f;
        //// Увеличение FOV (60 → 160)
        //while (Mathf.Abs(Camera.main.fieldOfView - targetFOV) > 0.1f)
        //{
        //    Camera.main.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, targetFOV, ref velocity, 1f);
        //    yield return null;
        //}
        //Camera.main.fieldOfView = targetFOV;


        //// Резкое уменьшение (160 → 30)
        //targetFOV = 50f;
        //velocity = 0f;
        //while (Mathf.Abs(Camera.main.fieldOfView - targetFOV) > 0.1f)
        //{
        //    Camera.main.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, targetFOV, ref velocity, 1f);
        //    yield return null;
        //}
        //Camera.main.fieldOfView = targetFOV;



        //// Плавное возвращение FOV (30 → 60)
        //targetFOV = 60f;
        //velocity = 0f;
        //while (Mathf.Abs(Camera.main.fieldOfView - targetFOV) > 0.1f)
        //{
        //    Camera.main.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, targetFOV, ref velocity, 0.3f);
        //    yield return null;
        //    currentTargetPosition = Vector3.SmoothDamp(currentTargetPosition, target.position, ref targetChangeVelocity, targetChangeTime);
        //}
        //Camera.main.fieldOfView = targetFOV;

        //rotationSmoothTime = (target != defaultTarget) ? 0.37f : 1f;

        Camera.main.fieldOfView = 160;

        float targetFOV = 60f;
        float velocity = 0f;

        while (Mathf.Abs(Camera.main.fieldOfView - targetFOV) > 0.1f)
        {
            Camera.main.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, targetFOV, ref velocity, time);
            yield return null;
        }

        if (target != defaultTarget)
        {
            rotationSmoothTime = 0.37f;
        }
        else rotationSmoothTime = 1f;

        Camera.main.fieldOfView = targetFOV;
    }
}