using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Editor tool: automatically builds the complete Main Menu UI in MainMenu.unity.
/// Menu: Tools → Build Main Menu UI
/// </summary>
public static class MainMenuBuilder
{
    private const string MENU_SCENE_PATH = "Assets/Scenes/MainMenu.unity";
    private const string GAME_SCENE_NAME = "SampleScene";

    [MenuItem("Tools/Build Main Menu UI")]
    public static void BuildMainMenuUI()
    {
        // 1. Open MainMenu scene
        var scene = EditorSceneManager.OpenScene(MENU_SCENE_PATH, OpenSceneMode.Single);

        // 2. Remove any previously built UI to avoid duplicates
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name == "MainMenuCanvas" || go.name == "MainMenuManager"
                || go.name == "Canvas" || go.name == "EventSystem")
                Object.DestroyImmediate(go);
        }

        // Remove any remaining duplicate EventSystems
        var allES = Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
        for (int i = 1; i < allES.Length; i++)
            Object.DestroyImmediate(allES[i].gameObject);

        // ── Color palette ──────────────────────────────────────────────
        Color panelBg       = new Color(0f,    0f,    0f,    0.72f);
        Color dimOverlay    = new Color(0f,    0f,    0f,    0.45f);
        Color white         = new Color(0.92f, 0.92f, 0.92f, 1f);
        Color grey          = new Color(0.65f, 0.65f, 0.65f, 1f);
        Color accent        = new Color(1f,    0.85f, 0.25f, 1f);
        Color quitColor     = new Color(0.95f, 0.25f, 0.25f, 1f);
        Color dividerColor  = new Color(1f,    1f,    1f,    0.18f);
        Color transparent   = new Color(0f,    0f,    0f,    0f);

        // ── Canvas ──────────────────────────────────────────────────────
        GameObject canvasGO = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem — only create if none exists
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ── Background RawImage (full screen, assign your screenshot here) ─
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var rawImg = bgGO.AddComponent<RawImage>();
        rawImg.color = new Color(1f, 1f, 1f, 0f); // invisible until a texture is assigned
        rawImg.raycastTarget = false;
        StretchFull(bgGO.GetComponent<RectTransform>());

        // ── Dark overlay (full screen) ──────────────────────────────────
        var dimGO = CreateUIImage("DimOverlay", canvasGO.transform, dimOverlay);
        StretchFull(dimGO.GetComponent<RectTransform>());

        // ── Left panel ──────────────────────────────────────────────────
        var panelGO = CreateUIImage("LeftPanel", canvasGO.transform, panelBg);
        var panelRT = panelGO.GetComponent<RectTransform>();
        // Fixed width 520, vertical stretch
        panelRT.anchorMin = new Vector2(0f, 0f);
        panelRT.anchorMax = new Vector2(0f, 1f);
        panelRT.pivot     = new Vector2(0f, 0.5f);
        panelRT.sizeDelta = new Vector2(520f, 0f);
        panelRT.anchoredPosition = Vector2.zero;

        // 面板内容用 Vertical Layout Group
        var vlg = panelGO.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(55, 40, 80, 60);
        vlg.spacing = 0f;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth  = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth  = false;
        vlg.childForceExpandHeight = false;

        // ── Game title (large)
        var titleCN = CreateTMPText("TitleMain", panelGO.transform, "ESCAPE\nCLASSROOM", 54, FontStyles.Bold, white);
        SetPreferredSize(titleCN.GetComponent<RectTransform>(), 430f, 130f);
        titleCN.GetComponent<RectTransform>().sizeDelta = new Vector2(430f, 130f);
        ((TextMeshProUGUI)titleCN).characterSpacing = 6f;
        ((TextMeshProUGUI)titleCN).lineSpacing = -10f;

        // ── Subtitle (small)
        var titleEN = CreateTMPText("TitleSub", panelGO.transform, "A CLASSROOM MYSTERY", 16, FontStyles.Normal, grey);
        titleEN.GetComponent<RectTransform>().sizeDelta = new Vector2(430f, 28f);
        ((TextMeshProUGUI)titleEN).characterSpacing = 4f;

        // ── 分隔线
        var dividerGO = CreateUIImage("Divider", panelGO.transform, dividerColor);
        var divRT = dividerGO.GetComponent<RectTransform>();
        divRT.sizeDelta = new Vector2(380f, 2f);
        AddLayoutElement(dividerGO, 380f, 2f, topPad: 36f, botPad: 44f);

        // ── 按钮列表
        float btnW = 430f, btnH = 52f;
        float btnSpacing = 24f;

        var btnStart = CreateMenuButton("BtnStart", panelGO.transform, "START GAME",   btnW, btnH, white,    accent,                          btnSpacing);
        var btnHow   = CreateMenuButton("BtnHow",   panelGO.transform, "HOW TO PLAY",  btnW, btnH, white,    accent,                          btnSpacing);
        var btnQuit  = CreateMenuButton("BtnQuit",  panelGO.transform, "QUIT",         btnW, btnH, quitColor, new Color(1f, 0.5f, 0.5f, 1f), btnSpacing);

        // ── How To Play panel (popup overlay) ───────────────────────────
        var howPanelGO = CreateUIImage("HowToPlayPanel", canvasGO.transform, new Color(0f, 0f, 0f, 0.88f));
        StretchFull(howPanelGO.GetComponent<RectTransform>());

        // 内容盒子（居中）
        var boxGO = CreateUIImage("ContentBox", howPanelGO.transform, new Color(0.1f, 0.1f, 0.1f, 0.95f));
        var boxRT = boxGO.GetComponent<RectTransform>();
        boxRT.anchorMin = new Vector2(0.5f, 0.5f);
        boxRT.anchorMax = new Vector2(0.5f, 0.5f);
        boxRT.pivot = new Vector2(0.5f, 0.5f);
        boxRT.sizeDelta = new Vector2(680f, 480f);
        boxRT.anchoredPosition = Vector2.zero;

        // ── Panel title
        var howTitle = CreateTMPText("HowTitle", boxGO.transform, "— HOW TO PLAY —", 28, FontStyles.Bold, accent);
        var howTitleRT = howTitle.GetComponent<RectTransform>();
        howTitleRT.anchorMin = new Vector2(0f, 1f);
        howTitleRT.anchorMax = new Vector2(1f, 1f);
        howTitleRT.pivot = new Vector2(0.5f, 1f);
        howTitleRT.sizeDelta = new Vector2(0f, 60f);
        howTitleRT.anchoredPosition = new Vector2(0f, -30f);
        howTitle.alignment = TextAlignmentOptions.Center;

        ((TextMeshProUGUI)howTitle).characterSpacing = 3f;

        // ── Body text
        string howText =
            "At the start of each class, carefully observe the classroom\n" +
            "to see if there is an <color=#FFDD44>anomaly</color>.\n\n" +
            "  \u2022  Anomaly detected  \u2192  Leave through the <color=#FFDD44>BACK DOOR</color>\n" +
            "  \u2022  No anomaly          \u2192  Leave through the <color=#FFDD44>FRONT DOOR</color>\n\n" +
            "A <color=#FF4444>wrong</color> choice resets your progress.\n" +
            "Pass <color=#FFDD44>5 classes</color> in a row to win!\n\n" +
            "<size=17><color=#888888>W / A / S / D   Move        Shift   Run        Mouse   Look</color></size>";

        var howBody = CreateTMPText("HowBody", boxGO.transform, howText, 22, FontStyles.Normal, white);
        howBody.enableWordWrapping = true;
        var howBodyRT = howBody.GetComponent<RectTransform>();
        howBodyRT.anchorMin = new Vector2(0f, 0.18f);
        howBodyRT.anchorMax = new Vector2(1f, 0.85f);
        howBodyRT.pivot = new Vector2(0.5f, 0.5f);
        howBodyRT.sizeDelta = new Vector2(-60f, 0f);
        howBodyRT.anchoredPosition = Vector2.zero;
        howBody.alignment = TextAlignmentOptions.TopLeft;

        // ── Close button
        var closeBtn = CreateMenuButton("CloseBtn", boxGO.transform, "CLOSE", 160f, 46f, white, accent, 0f);
        var closeBtnRT = closeBtn.GetComponent<RectTransform>();
        closeBtnRT.anchorMin = new Vector2(0.5f, 0f);
        closeBtnRT.anchorMax = new Vector2(0.5f, 0f);
        closeBtnRT.pivot     = new Vector2(0.5f, 0f);
        closeBtnRT.sizeDelta = new Vector2(160f, 46f);
        closeBtnRT.anchoredPosition = new Vector2(0f, 22f);

        howPanelGO.SetActive(false);

        // ── MainMenuManager + script wiring ─────────────────────────────
        var managerGO = new GameObject("MainMenuManager");
        var ctrl = managerGO.AddComponent<MainMenuController>();

        // Use SerializedObject to set private serialized fields
        var so = new SerializedObject(ctrl);
        so.FindProperty("gameSceneName").stringValue  = GAME_SCENE_NAME;
        so.FindProperty("howToPlayPanel").objectReferenceValue = howPanelGO;
        so.ApplyModifiedProperties();

        // Button OnClick 绑定
        BindButton(btnStart, ctrl, "StartGame");
        BindButton(btnHow,   ctrl, "ShowHowToPlay");
        BindButton(btnQuit,  ctrl, "QuitGame");
        BindButton(closeBtn, ctrl, "HideHowToPlay");

        // ── Add MainMenu + Standard to Build Settings ───────────────────
        var buildScenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity",    true),
            new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true),
        };
        EditorBuildSettings.scenes = buildScenes;

        // ── Save scene ──────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[MainMenuBuilder] Main Menu UI built successfully! Scene saved: " + MENU_SCENE_PATH);
        EditorUtility.DisplayDialog("Done", "Main Menu UI has been built!\nScene saved. Build Settings updated.", "OK");
    }

    // ────────────────────────────────────────────────────────────────
    // 辅助方法
    // ────────────────────────────────────────────────────────────────

    private static GameObject CreateUIImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = true;
        return go;
    }

    private static TMP_Text CreateTMPText(string name, Transform parent, string text, int fontSize, FontStyles style, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.raycastTarget = false;
        return tmp;
    }

    /// <summary>Creates a transparent-background menu button. Returns the button GameObject.</summary>
    private static GameObject CreateMenuButton(string name, Transform parent, string label,
        float width, float height, Color normalColor, Color hoverColor, float topPad)
    {
        // 容器（Button）
        var btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        var btn = btnGO.AddComponent<Button>();

        var img = btnGO.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0f); // transparent background, but receives raycasts
        img.raycastTarget = true;

        // Disable built-in ColorTint — hover is handled by MenuButtonHover
        var nav = btn.navigation;
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;
        btn.transition = Selectable.Transition.None;

        var rt = btnGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height);

        var tmp = CreateTMPText("Label", btnGO.transform, label, 24, FontStyles.Bold, normalColor);
        ((TextMeshProUGUI)tmp).characterSpacing = 3f;
        var tmpRT = tmp.GetComponent<RectTransform>();
        tmpRT.anchorMin = Vector2.zero;
        tmpRT.anchorMax = Vector2.one;
        tmpRT.sizeDelta = Vector2.zero;
        tmpRT.anchoredPosition = Vector2.zero;
        ((TextMeshProUGUI)tmp).alignment = TextAlignmentOptions.MidlineLeft;

        // Hover effect on the button container (covers the full click area)
        var hover = btnGO.AddComponent<MenuButtonHover>();
        hover.normalColor = normalColor;
        hover.hoverColor  = hoverColor;

        // 间距
        var le = btnGO.AddComponent<LayoutElement>();
        le.preferredWidth  = width;
        le.preferredHeight = height;
        le.minHeight = height;
        le.minWidth  = width;
        if (topPad > 0f)
        {
            // 用空白占位实现间距
            var spacerGO = new GameObject(name + "_Spacer");
            spacerGO.transform.SetParent(parent, false);
            spacerGO.AddComponent<Image>().color = new Color(0, 0, 0, 0);
            var sle = spacerGO.AddComponent<LayoutElement>();
            sle.preferredHeight = topPad;
            sle.minHeight = topPad;
            spacerGO.transform.SetSiblingIndex(btnGO.transform.GetSiblingIndex());
        }

        return btnGO;
    }

    private static void AddLayoutElement(GameObject go, float w, float h, float topPad = 0f, float botPad = 0f)
    {
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth  = w;
        le.preferredHeight = h;
        le.minHeight = h;
    }

    private static void SetPreferredSize(RectTransform rt, float w, float h)
    {
        rt.sizeDelta = new Vector2(w, h);
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    private static void BindButton(GameObject btnGO, MainMenuController ctrl, string methodName)
    {
        var btn = btnGO.GetComponent<Button>();
        if (btn == null) return;

        var so = new SerializedObject(btn);
        var onClickProp = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        onClickProp.InsertArrayElementAtIndex(0);
        var call = onClickProp.GetArrayElementAtIndex(0);
        call.FindPropertyRelative("m_Target").objectReferenceValue = ctrl;
        call.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue = ctrl.GetType().AssemblyQualifiedName;
        call.FindPropertyRelative("m_MethodName").stringValue = methodName;
        call.FindPropertyRelative("m_Mode").intValue = 1; // void, no args
        call.FindPropertyRelative("m_CallState").intValue = 2; // Runtime Only
        so.ApplyModifiedProperties();
    }
}
