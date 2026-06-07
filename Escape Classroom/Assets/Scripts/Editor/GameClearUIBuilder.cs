using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor tool: builds the Game Clear end-screen UI in the current open scene.
/// Menu: Tools → Build Game Clear UI
/// </summary>
public static class GameClearUIBuilder
{
    [MenuItem("Tools/Build Game Clear UI")]
    public static void Build()
    {
        var scene = EditorSceneManager.GetActiveScene();

        // Remove any previously built end screen
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name == "GameClearCanvas")
                Object.DestroyImmediate(go);
        }

        // Ensure EventSystem exists in this scene (required for button clicks)
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("[GameClearUIBuilder] Created EventSystem in scene.");
        }

        // ── Colors ──────────────────────────────────────────────────────
        Color black     = new Color(0f,    0f,    0f,    1f);
        Color white     = new Color(0.95f, 0.95f, 0.95f, 1f);
        Color yellow    = new Color(1f,    0.85f, 0.25f, 1f);
        Color grey      = new Color(0.70f, 0.70f, 0.70f, 1f);
        Color btnBorder = new Color(1f,    1f,    1f,    0.15f);

        // ── Root Canvas ──────────────────────────────────────────────────
        var canvasGO = new GameObject("GameClearCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10; // above everything

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Root CanvasGroup — controls the black overlay alpha
        var rootCG = canvasGO.AddComponent<CanvasGroup>();
        rootCG.alpha           = 0f;
        rootCG.interactable    = false;
        rootCG.blocksRaycasts  = false;

        // ── Black background ─────────────────────────────────────────────
        var bg = MakeImage("Background", canvasGO.transform, black);
        Stretch(bg.GetComponent<RectTransform>());

        // ── Message group (fades in after black) ─────────────────────────
        var msgGroupGO = new GameObject("MessageGroup");
        msgGroupGO.transform.SetParent(canvasGO.transform, false);
        var msgRT = msgGroupGO.AddComponent<RectTransform>();
        msgRT.anchorMin        = new Vector2(0.5f, 0.5f);
        msgRT.anchorMax        = new Vector2(0.5f, 0.5f);
        msgRT.pivot            = new Vector2(0.5f, 0.5f);
        msgRT.sizeDelta        = new Vector2(900f, 420f);
        msgRT.anchoredPosition = new Vector2(0f, 60f);
        var msgCG = msgGroupGO.AddComponent<CanvasGroup>();
        msgCG.alpha = 0f;

        // Icon / emoji line
        var iconTxt = MakeTMP("Icon", msgGroupGO.transform,
            "* * *", 28, FontStyles.Normal, yellow);
        var iconRT = iconTxt.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0f, 1f); iconRT.anchorMax = new Vector2(1f, 1f);
        iconRT.pivot     = new Vector2(0.5f, 1f);
        iconRT.sizeDelta = new Vector2(0f, 80f);
        iconRT.anchoredPosition = new Vector2(0f, 0f);
        iconTxt.alignment = TextAlignmentOptions.Center;

        // Main message
        var mainTxt = MakeTMP("MainMessage", msgGroupGO.transform,
            "You wasted another day.", 52, FontStyles.Bold, white);
        var mainRT = mainTxt.GetComponent<RectTransform>();
        mainRT.anchorMin = new Vector2(0f, 1f); mainRT.anchorMax = new Vector2(1f, 1f);
        mainRT.pivot     = new Vector2(0.5f, 1f);
        mainRT.sizeDelta = new Vector2(0f, 70f);
        mainRT.anchoredPosition = new Vector2(0f, -90f);
        mainTxt.alignment = TextAlignmentOptions.Center;
        ((TextMeshProUGUI)mainTxt).characterSpacing = 2f;

        // Sub message
        var subTxt = MakeTMP("SubMessage", msgGroupGO.transform,
            "Cherish your time.\nDon't waste it scrolling through short videos.", 
            26, FontStyles.Normal, grey);
        var subRT = subTxt.GetComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0f, 1f); subRT.anchorMax = new Vector2(1f, 1f);
        subRT.pivot     = new Vector2(0.5f, 1f);
        subRT.sizeDelta = new Vector2(0f, 100f);
        subRT.anchoredPosition = new Vector2(0f, -175f);
        subTxt.alignment = TextAlignmentOptions.Center;
        ((TextMeshProUGUI)subTxt).lineSpacing = 8f;

        // Divider
        var divGO = MakeImage("Divider", msgGroupGO.transform, new Color(1f, 1f, 1f, 0.15f));
        var divRT = divGO.GetComponent<RectTransform>();
        divRT.anchorMin = new Vector2(0.5f, 1f); divRT.anchorMax = new Vector2(0.5f, 1f);
        divRT.pivot     = new Vector2(0.5f, 1f);
        divRT.sizeDelta = new Vector2(500f, 1f);
        divRT.anchoredPosition = new Vector2(0f, -290f);

        // Stat line (how many days wasted counter — just flavour text)
        var statTxt = MakeTMP("StatLine", msgGroupGO.transform,
            "CLASSES WASTED TODAY: 5 / 5", 18, FontStyles.Normal, new Color(0.5f, 0.5f, 0.5f, 1f));
        var statRT = statTxt.GetComponent<RectTransform>();
        statRT.anchorMin = new Vector2(0f, 1f); statRT.anchorMax = new Vector2(1f, 1f);
        statRT.pivot     = new Vector2(0.5f, 1f);
        statRT.sizeDelta = new Vector2(0f, 30f);
        statRT.anchoredPosition = new Vector2(0f, -306f);
        statTxt.alignment = TextAlignmentOptions.Center;
        ((TextMeshProUGUI)statTxt).characterSpacing = 3f;

        // ── Button group ─────────────────────────────────────────────────
        var btnGroupGO = new GameObject("ButtonGroup");
        btnGroupGO.transform.SetParent(canvasGO.transform, false);
        var btnGroupRT = btnGroupGO.AddComponent<RectTransform>();
        btnGroupRT.anchorMin        = new Vector2(0.5f, 0.5f);
        btnGroupRT.anchorMax        = new Vector2(0.5f, 0.5f);
        btnGroupRT.pivot            = new Vector2(0.5f, 0.5f);
        btnGroupRT.sizeDelta        = new Vector2(600f, 60f);
        btnGroupRT.anchoredPosition = new Vector2(0f, -260f);
        var btnCG = btnGroupGO.AddComponent<CanvasGroup>();
        btnCG.alpha = 0f;

        var hlg = btnGroupGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 30f;
        hlg.childAlignment          = TextAnchor.MiddleCenter;
        hlg.childControlWidth       = false;
        hlg.childControlHeight      = false;
        hlg.childForceExpandWidth   = false;
        hlg.childForceExpandHeight  = false;

        var playAgainBtn = MakeEndButton("PlayAgainBtn", btnGroupGO.transform,
            "PLAY AGAIN", new Color(0.2f, 0.2f, 0.2f, 0.9f), white, yellow);
        var quitBtn = MakeEndButton("QuitBtn", btnGroupGO.transform,
            "QUIT", new Color(0.15f, 0.15f, 0.15f, 0.9f), new Color(0.9f, 0.3f, 0.3f, 1f), new Color(1f, 0.5f, 0.5f, 1f));

        // ── GameClearScreen component ─────────────────────────────────────
        var screenComp = canvasGO.AddComponent<GameClearScreen>();
        var so = new SerializedObject(screenComp);
        so.FindProperty("canvasGroup").objectReferenceValue        = rootCG;
        so.FindProperty("messageText").objectReferenceValue        = mainTxt;
        so.FindProperty("subMessageText").objectReferenceValue     = subTxt;
        so.FindProperty("playAgainButton").objectReferenceValue    = playAgainBtn.GetComponent<Button>();
        so.FindProperty("quitButton").objectReferenceValue         = quitBtn.GetComponent<Button>();
        so.FindProperty("gameSceneName").stringValue               = "SampleScene";
        so.ApplyModifiedProperties();

        // Button OnClick
        BindButton(playAgainBtn, screenComp, "OnPlayAgain");
        BindButton(quitBtn,      screenComp, "OnQuit");

        // Start deactivated — GameClearScreen.Show() will activate it
        canvasGO.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[GameClearUIBuilder] Game Clear UI built and saved.");
        EditorUtility.DisplayDialog("Done",
            "Game Clear UI built in the current scene.\n\n" +
            "Next steps:\n" +
            "1. Select 'GameClearCanvas' and drag it into the 'Game Clear Screen' field of GameEndTrigger.\n" +
            "2. Place a box collider near the bunk bed, add GameEndTrigger, and assign references.",
            "OK");
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static Image MakeImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static TMP_Text MakeTMP(string name, Transform parent, string text,
        int size, FontStyles style, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text       = text;
        tmp.fontSize   = size;
        tmp.fontStyle  = style;
        tmp.color      = color;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static GameObject MakeEndButton(string name, Transform parent,
        string label, Color bgColor, Color textColor, Color hoverColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var img = go.AddComponent<Image>();
        img.color = bgColor;
        img.raycastTarget = true;

        var btn = go.AddComponent<Button>();
        var nav = btn.navigation;
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;
        btn.transition = Selectable.Transition.None;

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(240f, 52f);

        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth  = 240f;
        le.preferredHeight = 52f;
        le.minWidth  = 240f;
        le.minHeight = 52f;

        var txt = MakeTMP("Label", go.transform, label, 22, FontStyles.Bold, textColor);
        var trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        trt.anchoredPosition = Vector2.zero;
        ((TextMeshProUGUI)txt).alignment = TextAlignmentOptions.Center;
        ((TextMeshProUGUI)txt).characterSpacing = 3f;

        var hover = go.AddComponent<MenuButtonHover>();
        hover.normalColor = textColor;
        hover.hoverColor  = hoverColor;

        return go;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.sizeDelta        = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    private static void BindButton(GameObject btnGO, GameClearScreen ctrl, string method)
    {
        var btn = btnGO.GetComponent<Button>();
        if (btn == null) return;
        var so   = new SerializedObject(btn);
        var list = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        list.InsertArrayElementAtIndex(0);
        var call = list.GetArrayElementAtIndex(0);
        call.FindPropertyRelative("m_Target").objectReferenceValue          = ctrl;
        call.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue   = ctrl.GetType().AssemblyQualifiedName;
        call.FindPropertyRelative("m_MethodName").stringValue               = method;
        call.FindPropertyRelative("m_Mode").intValue                        = 1;
        call.FindPropertyRelative("m_CallState").intValue                   = 2;
        so.ApplyModifiedProperties();
    }
}
