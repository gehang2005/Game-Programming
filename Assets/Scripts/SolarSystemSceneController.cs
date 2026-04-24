using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SolarSystemSceneController : MonoBehaviour
{
    [Header("Scene References")]
    public Transform sun;
    public Transform earth;
    public Transform moon;
    public Light directionalLight;

    [Header("Orbit Motion")]
    public float sunSpinSpeed = 18f;
    public float earthSpinSpeed = 40f;
    public float moonSpinSpeed = 24f;
    public float earthOrbitSpeed = 12f;
    public float moonOrbitSpeed = 40f;

    [Header("Camera")]
    public Vector3 mainViewPosition = new Vector3(0f, 3.1f, -22f);
    public Vector3 mainViewLookOffset = new Vector3(0f, 0.4f, 0f);
    public float cameraMoveSpeed = 3.5f;
    public float closeViewPadding = 1.1f;
    public float closeViewHeight = 0.75f;
    public float orbitDragSensitivity = 0.42f;
    public float mainViewPanSpeed = 5.5f;
    public float minOrbitPitch = 10f;
    public float maxOrbitPitch = 75f;

    [Header("Background")]
    public float skyboxExposure = 0.72f;

    [Header("Feedback")]
    public float pulseScale = 1.16f;
    public float pulseSpeed = 5f;

    private struct BodyInfo
    {
        public string title;
        public string fact;
        public float tone;

        public BodyInfo(string title, string fact, float tone)
        {
            this.title = title;
            this.fact = fact;
            this.tone = tone;
        }
    }

    private Camera sceneCamera;
    private AudioSource audioSource;
    private Vector3 earthOrbitOffset;
    private Vector3 moonOrbitOffset;
    private Transform selectedBody;
    private float mainViewDistance;
    private float mainViewYaw;
    private float mainViewPitch;
    private bool isDraggingMainView;
    private Vector3 mouseDownPosition;
    private Vector3 currentMainViewLookOffset;
    private string panelTitle = "Mini Space Tour";
    private string panelFact = "Click the Sun, Earth, or Moon to zoom in and learn one quick fact.";

    private readonly Dictionary<Transform, BodyInfo> bodyInfos = new Dictionary<Transform, BodyInfo>();
    private readonly Dictionary<Transform, Vector3> baseScales = new Dictionary<Transform, Vector3>();
    private readonly Dictionary<Transform, Material> bodyMaterials = new Dictionary<Transform, Material>();

    private void Start()
    {
        sceneCamera = GetComponent<Camera>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        ResolveSceneReferences();
        CacheBodies();
        ConfigureLighting();
        ConfigureSkybox();
        InitializeMainViewOrbit();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ResetView(true);
    }

    private void Update()
    {
        if (!HasAllBodies())
        {
            return;
        }

        AnimateBodies();
        HandleInput();
        UpdateCamera();
        UpdateHighlight();
    }

    private void ResolveSceneReferences()
    {
        if (sun == null)
        {
            GameObject sunObject = GameObject.Find("Sun");
            sun = sunObject != null ? sunObject.transform : null;
        }

        if (earth == null)
        {
            GameObject earthObject = GameObject.Find("Earth");
            earth = earthObject != null ? earthObject.transform : null;
        }

        if (moon == null)
        {
            GameObject moonObject = GameObject.Find("Moon");
            moon = moonObject != null ? moonObject.transform : null;
        }

        if (directionalLight == null)
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    break;
                }
            }
        }

        if (HasAllBodies())
        {
            earthOrbitOffset = earth.position - sun.position;
            moonOrbitOffset = moon.position - earth.position;
        }
    }

    private void CacheBodies()
    {
        bodyInfos.Clear();
        baseScales.Clear();
        bodyMaterials.Clear();

        CacheBody(sun, new BodyInfo("Sun", "The Sun is a star. It gives our planet light and warmth every day.", 392f));
        CacheBody(earth, new BodyInfo("Earth", "Earth is our home planet. It has oceans, clouds, and lots of life.", 523.25f));
        CacheBody(moon, new BodyInfo("Moon", "The Moon travels around Earth and helps make the night sky glow.", 659.25f));
    }

    private void CacheBody(Transform body, BodyInfo info)
    {
        if (body == null)
        {
            return;
        }

        Renderer renderer = body.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        bodyInfos[body] = info;
        baseScales[body] = body.localScale;

        Material runtimeMaterial = renderer.material;
        bodyMaterials[body] = runtimeMaterial;
    }

    private void ConfigureLighting()
    {
        RenderSettings.ambientLight = new Color(0.12f, 0.15f, 0.22f);
        RenderSettings.fog = false;

        if (directionalLight == null)
        {
            return;
        }

        directionalLight.color = new Color(1f, 0.96f, 0.82f);
        directionalLight.intensity = 1.15f;
        directionalLight.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
    }

    private void ConfigureSkybox()
    {
        sceneCamera.clearFlags = CameraClearFlags.Skybox;

        Material currentSkybox = RenderSettings.skybox;
        if (currentSkybox == null || currentSkybox.mainTexture == null)
        {
            return;
        }

        Shader panoramicShader = Shader.Find("Skybox/Panoramic");
        if (panoramicShader == null)
        {
            return;
        }

        Material panoramicSkybox = new Material(panoramicShader);
        panoramicSkybox.name = "RuntimeSpaceSkybox";
        panoramicSkybox.SetTexture("_MainTex", currentSkybox.mainTexture);
        panoramicSkybox.SetFloat("_Exposure", skyboxExposure);
        panoramicSkybox.SetFloat("_Rotation", 0f);

        RenderSettings.skybox = panoramicSkybox;
        DynamicGI.UpdateEnvironment();
    }

    private bool HasAllBodies()
    {
        return sun != null && earth != null && moon != null;
    }

    private void AnimateBodies()
    {
        sun.Rotate(Vector3.up, sunSpinSpeed * Time.deltaTime, Space.Self);
        earth.Rotate(Vector3.up, earthSpinSpeed * Time.deltaTime, Space.Self);
        moon.Rotate(Vector3.up, moonSpinSpeed * Time.deltaTime, Space.Self);

        earthOrbitOffset = Quaternion.Euler(0f, earthOrbitSpeed * Time.deltaTime, 0f) * earthOrbitOffset;
        earth.position = sun.position + earthOrbitOffset;

        moonOrbitOffset = Quaternion.Euler(0f, moonOrbitSpeed * Time.deltaTime, 0f) * moonOrbitOffset;
        moon.position = earth.position + moonOrbitOffset;
    }

    private void HandleInput()
    {
        if (selectedBody == null)
        {
            HandleMainViewPan();
        }

        if (selectedBody == null && Input.GetMouseButtonDown(0))
        {
            mouseDownPosition = Input.mousePosition;
            isDraggingMainView = false;
        }

        if (selectedBody == null && Input.GetMouseButton(0))
        {
            Vector3 dragDelta = Input.mousePosition - mouseDownPosition;
            if (!isDraggingMainView && dragDelta.sqrMagnitude > 16f)
            {
                isDraggingMainView = true;
            }

            if (isDraggingMainView)
            {
                mainViewYaw += Input.GetAxis("Mouse X") * orbitDragSensitivity * 100f * Time.deltaTime;
                mainViewPitch = Mathf.Clamp(mainViewPitch - Input.GetAxis("Mouse Y") * orbitDragSensitivity * 100f * Time.deltaTime, minOrbitPitch, maxOrbitPitch);
            }
        }

        if (Input.GetMouseButtonUp(0) && !isDraggingMainView)
        {
            Ray ray = sceneCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && bodyInfos.ContainsKey(hit.transform))
            {
                SelectBody(hit.transform);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDraggingMainView = false;
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
        {
            ResetView(false);
        }
    }

    private void HandleMainViewPan()
    {
        float horizontal = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            horizontal -= 1f;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            horizontal += 1f;
        }

        float vertical = 0f;
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.UpArrow))
        {
            vertical += 1f;
        }
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.DownArrow))
        {
            vertical -= 1f;
        }

        float depth = 0f;
        if (Input.GetKey(KeyCode.W))
        {
            depth += 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            depth -= 1f;
        }

        if (Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f) && Mathf.Approximately(depth, 0f))
        {
            return;
        }

        Vector3 lookTarget = sun.position + currentMainViewLookOffset;
        Vector3 viewDirection = (lookTarget - transform.position).normalized;
        Vector3 panRight = Vector3.Cross(Vector3.up, viewDirection).normalized;
        Vector3 panUp = Vector3.up;
        Vector3 panForward = Vector3.ProjectOnPlane(viewDirection, Vector3.up).normalized;
        if (panForward.sqrMagnitude < 0.0001f)
        {
            panForward = Vector3.forward;
        }

        Vector3 panMotion = (panRight * horizontal + panUp * vertical + panForward * depth) * (mainViewPanSpeed * Time.deltaTime);

        currentMainViewLookOffset += panMotion;
    }

    private void InitializeMainViewOrbit()
    {
        if (sun == null)
        {
            return;
        }

        currentMainViewLookOffset = mainViewLookOffset;
        Vector3 lookTarget = sun.position + mainViewLookOffset;
        Vector3 viewOffset = mainViewPosition - lookTarget;
        mainViewDistance = viewOffset.magnitude;

        if (mainViewDistance <= 0.001f)
        {
            mainViewDistance = 10f;
            viewOffset = new Vector3(0f, 3f, -mainViewDistance);
        }

        mainViewYaw = Mathf.Atan2(viewOffset.x, viewOffset.z) * Mathf.Rad2Deg;
        float horizontalDistance = new Vector2(viewOffset.x, viewOffset.z).magnitude;
        mainViewPitch = Mathf.Atan2(viewOffset.y, horizontalDistance) * Mathf.Rad2Deg;
        mainViewPitch = Mathf.Clamp(mainViewPitch, minOrbitPitch, maxOrbitPitch);
    }

    private void SelectBody(Transform body)
    {
        selectedBody = body;

        BodyInfo info = bodyInfos[body];
        panelTitle = info.title;
        panelFact = info.fact;

        PlayTone(info.tone, 0.18f);
    }

    private void ResetView(bool immediate)
    {
        selectedBody = null;
        panelTitle = "Mini Space Tour";
        panelFact = "Click the Sun, Earth, or Moon to zoom in and learn one quick fact. Right click or press Escape to go back.";

        Vector3 desiredPosition;
        Quaternion desiredRotation;
        GetDesiredCameraPose(out desiredPosition, out desiredRotation);

        if (immediate)
        {
            transform.position = desiredPosition;
            transform.rotation = desiredRotation;
        }
        else
        {
            PlayTone(246.94f, 0.12f);
        }
    }

    private void UpdateCamera()
    {
        Vector3 desiredPosition;
        Quaternion desiredRotation;
        GetDesiredCameraPose(out desiredPosition, out desiredRotation);

        float t = 1f - Mathf.Exp(-cameraMoveSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, t);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, t);
    }

    private void GetDesiredCameraPose(out Vector3 desiredPosition, out Quaternion desiredRotation)
    {
        if (selectedBody == null)
        {
            Vector3 lookTarget = sun.position + currentMainViewLookOffset;
            Quaternion orbitRotation = Quaternion.Euler(mainViewPitch, mainViewYaw, 0f);
            Vector3 orbitOffset = orbitRotation * new Vector3(0f, 0f, -mainViewDistance);
            desiredPosition = lookTarget + orbitOffset;
            desiredRotation = Quaternion.LookRotation((lookTarget - desiredPosition).normalized, Vector3.up);
            return;
        }

        Renderer renderer = selectedBody.GetComponent<Renderer>();
        float radius = renderer != null ? renderer.bounds.extents.magnitude : 0.6f;
        Vector3 orbitDirection = selectedBody == sun ? Vector3.back : (selectedBody.position - sun.position).normalized;
        Vector3 cameraOffset = orbitDirection * (radius * 3f + closeViewPadding) + Vector3.up * Mathf.Max(closeViewHeight, radius * 0.9f);

        desiredPosition = selectedBody.position + cameraOffset;
        desiredRotation = Quaternion.LookRotation((selectedBody.position - desiredPosition).normalized, Vector3.up);
    }

    private void UpdateHighlight()
    {
        foreach (KeyValuePair<Transform, Vector3> entry in baseScales)
        {
            entry.Key.localScale = entry.Value;
        }
    }

    private void PlayTone(float frequency, float duration)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        AudioClip clip = AudioClip.Create("SpaceTone", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)sampleRate;
            float envelope = Mathf.Clamp01(1f - time / duration);
            float wobble = Mathf.Sin(2f * Mathf.PI * frequency * time);
            samples[i] = wobble * envelope * 0.18f;
        }

        clip.SetData(samples, 0);
        audioSource.PlayOneShot(clip);
    }

    private void OnGUI()
    {
        GUIStyle panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.alignment = TextAnchor.UpperLeft;
        panelStyle.fontSize = 18;
        panelStyle.wordWrap = true;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 22;
        titleStyle.fontStyle = FontStyle.Bold;

        GUIStyle bodyStyle = new GUIStyle(GUI.skin.label);
        bodyStyle.fontSize = 17;
        bodyStyle.wordWrap = true;

        Rect panelRect = new Rect(20f, 20f, 420f, 180f);
        GUI.Box(panelRect, string.Empty, panelStyle);
        GUI.Label(new Rect(36f, 34f, 388f, 30f), panelTitle, titleStyle);
        GUI.Label(new Rect(36f, 74f, 388f, 92f), panelFact, bodyStyle);

        if (GUI.Button(new Rect(36f, 150f, 160f, 34f), "Back To Main View"))
        {
            ResetView(false);
        }

        GUI.Label(new Rect(20f, Screen.height - 38f, 980f, 24f), "Drag with left mouse to orbit the main view. Use A/D for left-right, Q/E for up-down, and W/S for forward-back. Click a space object to explore it. Right click or press Escape to return.");
    }
}