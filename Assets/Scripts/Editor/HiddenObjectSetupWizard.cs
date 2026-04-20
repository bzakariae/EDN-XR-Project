using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

public class HiddenObjectSetupWizard : EditorWindow
{
    [MenuItem("Cuisine/Installer Jeu Cherche et Trouve")]
    public static void Run()
    {
        bool confirm = EditorUtility.DisplayDialog(
            "Installer le Jeu Cherche et Trouve",
            "Ce script va :\n\n" +
            "- Meubler la cuisine\n" +
            "- Creer l'UI qui se place devant le joueur\n" +
            "- Configurer la teleportation (Pointer le sol)\n" +
            "- Generer des objets a chercher\n\n" +
            "Continuer ?",
            "Oui, installer !", "Annuler");

        if (!confirm) return;

        CleanupOldStuff();
        SetupXRAndMovement();
        SetupTeleportationFloor();
        DecorateKitchen();
        GameObject canvasGO = BuildHiddenObjectCanvas(out HiddenObjectUI uiScript);
        List<Transform> spawnPoints = CreateSpawnPoints();
        CreateGameManager(uiScript, spawnPoints);

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Termine !", "Pret ! Mettez votre casque et lancez Play.", "OK");
    }

    static void CleanupOldStuff()
    {
        string[] oldObjects = { "[RecipeManager]", "[RecipeCanvas]", "[TableSpawnPoint]", "[RecipeZone_Bowl]", "[EventSystem]", "EventSystem" };
        foreach (string objName in oldObjects)
        {
            GameObject oldObj = GameObject.Find(objName);
            if (oldObj != null) Object.DestroyImmediate(oldObj);
        }

        var evSysList = FindObjectsOfType<EventSystem>();
        foreach(var es in evSysList)
        {
            if (es.name != "XR Interaction Setup" && es.name != "[XR Interaction Setup]")
                Object.DestroyImmediate(es.gameObject);
        }
    }

    static void SetupXRAndMovement()
    {
        const string XR_ORIGIN_PATH = "Assets/Samples/XR Interaction Toolkit/2.6.4/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
        const string XR_SETUP_PATH = "Assets/Samples/XR Interaction Toolkit/2.6.4/Starter Assets/Prefabs/XR Interaction Setup.prefab";

        if (GameObject.Find("XR Interaction Setup") == null && GameObject.Find("[XR Interaction Setup]") == null)
        {
            GameObject setupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(XR_SETUP_PATH);
            if (setupPrefab != null)
            {
                GameObject setupInstance = (GameObject)PrefabUtility.InstantiatePrefab(setupPrefab);
                setupInstance.name = "[XR Interaction Setup]";
            }
        }

        GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
        if (xrOrigin == null) xrOrigin = GameObject.Find("[XR Origin]");
        if (xrOrigin == null)
        {
            GameObject originPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(XR_ORIGIN_PATH);
            if (originPrefab != null)
            {
                xrOrigin = (GameObject)PrefabUtility.InstantiatePrefab(originPrefab);
                xrOrigin.name = "[XR Origin]";
                xrOrigin.transform.position = new Vector3(0, 0, -2f);
            }
        }
        
        // On ne modifie plus les composants internes de XR Origin, le prefab par defaut de Unity
        // gere deja tres bien la teleportation via l'ActionBasedControllerManager.
    }

    static void SetupTeleportationFloor()
    {
        GameObject floor = GameObject.Find("[TeleportFloor]");
        if (floor != null) Object.DestroyImmediate(floor);
        
        floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "[TeleportFloor]";
        floor.transform.position = new Vector3(0, 0, 0);
        floor.transform.localScale = new Vector3(2, 1, 2);
        
        Renderer r = floor.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.2f, 0.2f, 0.2f);
        r.sharedMaterial = mat;

        System.Type teleAreaType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.TeleportationArea, Unity.XR.Interaction.Toolkit");
        if (teleAreaType != null)
        {
            floor.AddComponent(teleAreaType);
        }
    }

    static void DecorateKitchen()
    {
        GameObject envParent = GameObject.Find("[KitchenEnvironment]");
        if (envParent != null) Object.DestroyImmediate(envParent);
        envParent = new GameObject("[KitchenEnvironment]");

        SpawnFurniture("refrigerator_1.prefab", new Vector3(-3, 0, 3), Quaternion.Euler(0, 90, 0), envParent);
        SpawnFurniture("oven.prefab", new Vector3(-1, 0, 3), Quaternion.Euler(0, 180, 0), envParent);
        SpawnFurniture("stove.prefab", new Vector3(1, 0, 3), Quaternion.Euler(0, 180, 0), envParent);
        SpawnFurniture("table_1_Long.prefab", new Vector3(0, 0, 0), Quaternion.identity, envParent);
        SpawnFurniture("chair_1.prefab", new Vector3(-1, 0, -1), Quaternion.identity, envParent);
        SpawnFurniture("chair_2.prefab", new Vector3(1, 0, 1), Quaternion.Euler(0, 180, 0), envParent);
    }

    static void SpawnFurniture(string prefabName, Vector3 pos, Quaternion rot, GameObject parent)
    {
        string path = "Assets/Toony Kitchen Ingredients Free/Prefabs/Furnitures/" + prefabName;
        GameObject p = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (p != null)
        {
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(p);
            obj.transform.position = pos;
            obj.transform.rotation = rot;
            obj.transform.SetParent(parent.transform);
            
            if (obj.GetComponent<Collider>() == null && obj.GetComponentInChildren<Collider>() == null)
                obj.AddComponent<BoxCollider>();
        }
    }

    static List<Transform> CreateSpawnPoints()
    {
        GameObject parent = GameObject.Find("[HiddenSpawns]");
        if (parent != null) Object.DestroyImmediate(parent);
        parent = new GameObject("[HiddenSpawns]");
        List<Transform> list = new List<Transform>();

        Vector3[] positions = new Vector3[]
        {
            new Vector3(0, 0.9f, 0),
            new Vector3(-0.5f, 0.9f, 0.5f),
            new Vector3(0.5f, 0.9f, -0.5f),
            new Vector3(-1, 0.9f, 3f),
            new Vector3(1, 0.9f, 3f),
            new Vector3(-3, 1.8f, 3f),
            new Vector3(-1, 0.5f, -1),
            new Vector3(1, 0.5f, 1),
            new Vector3(-2, 0.1f, 2),
            new Vector3(2, 0.1f, -2),
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject sp = new GameObject($"Spawn_{i}");
            sp.transform.SetParent(parent.transform);
            sp.transform.position = positions[i];
            list.Add(sp.transform);
        }

        return list;
    }

    static GameObject BuildHiddenObjectCanvas(out HiddenObjectUI uiScript)
    {
        GameObject oldCanvas = GameObject.Find("[HiddenObjectCanvas]");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);

        GameObject canvasGO = new GameObject("[HiddenObjectCanvas]");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10;
        
        System.Type trackedType = null;
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            trackedType = assembly.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster");
            if (trackedType != null) break;
        }
        if (trackedType != null) canvasGO.AddComponent(trackedType);
        else canvasGO.AddComponent<GraphicRaycaster>();

        RectTransform rt = canvasGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800, 600);
        canvasGO.transform.localScale = Vector3.one * 0.0025f;

        uiScript = canvasGO.AddComponent<HiddenObjectUI>();

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvasGO.transform, false);
        bg.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        FillRect(bg);

        // --- Main Menu ---
        GameObject mainMenu = new GameObject("MainMenuPanel");
        mainMenu.transform.SetParent(bg.transform, false);
        FillRect(mainMenu);
        
        MakeText(mainMenu.transform, "Title", "Jeu des Objets Caches", 60, FontStyles.Bold, new Vector2(0, 0.8f), Vector2.one);
        uiScript.btnFind5 = MakeButton(mainMenu.transform, "Btn5", "Trouver 5 Objets", new Vector2(0.2f, 0.5f), new Vector2(0.8f, 0.7f)).GetComponent<Button>();
        uiScript.btnFind10 = MakeButton(mainMenu.transform, "Btn10", "Trouver 10 Objets", new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.4f)).GetComponent<Button>();

        // --- Game HUD ---
        GameObject hud = new GameObject("GameHUDPanel");
        hud.transform.SetParent(bg.transform, false);
        FillRect(hud);
        hud.SetActive(false);

        uiScript.targetItemText = MakeText(hud.transform, "TargetText", "Cherchez : <item>", 50, FontStyles.Bold, new Vector2(0, 0.6f), Vector2.one);
        uiScript.scoreText = MakeText(hud.transform, "ScoreText", "Score : 0/5", 40, FontStyles.Normal, new Vector2(0, 0.2f), new Vector2(1, 0.5f));

        // --- Game Over ---
        GameObject over = new GameObject("GameOverPanel");
        over.transform.SetParent(bg.transform, false);
        FillRect(over);
        over.SetActive(false);

        uiScript.finalScoreText = MakeText(over.transform, "WinText", "Gagne !", 60, FontStyles.Bold, new Vector2(0, 0.5f), Vector2.one);
        uiScript.btnRestart = MakeButton(over.transform, "BtnRestart", "Rejouer", new Vector2(0.2f, 0.1f), new Vector2(0.8f, 0.3f)).GetComponent<Button>();

        uiScript.mainMenuPanel = mainMenu;
        uiScript.gameHudPanel = hud;
        uiScript.gameOverPanel = over;

        return canvasGO;
    }

    static void CreateGameManager(HiddenObjectUI ui, List<Transform> spawns)
    {
        GameObject old = GameObject.Find("[HiddenObjectManager]");
        if (old != null) Object.DestroyImmediate(old);

        GameObject go = new GameObject("[HiddenObjectManager]");
        HiddenObjectManager manager = go.AddComponent<HiddenObjectManager>();
        manager.uiManager = ui;
        manager.spawnPoints = spawns.ToArray();

        string[] prefabs = { 
            "egg.prefab", "butter.prefab", "dough.prefab", "fish.prefab", 
            "cheeseWheel.prefab", "chicken_Raw.prefab", "tomato.prefab", 
            "potato.prefab", "cucumber.prefab", "mushroom_Raw.prefab",
            "lettuceButterhead.prefab", "onion_White.prefab" 
        };
        manager.possibleItems = new List<HiddenObjectManager.HiddenItemDef>();

        foreach (string p in prefabs)
        {
            string path = "Assets/Toony Kitchen Ingredients Free/Prefabs/Ingredients/" + p;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                manager.possibleItems.Add(new HiddenObjectManager.HiddenItemDef {
                    itemName = p.Replace(".prefab", "").ToUpper(),
                    prefab = prefab
                });
            }
        }
    }

    static void FillRect(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static TextMeshProUGUI MakeText(Transform parent, string name, string text, int size, FontStyles style, Vector2 aMin, Vector2 aMax)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return tmp;
    }

    static GameObject MakeButton(Transform parent, string name, string text, Vector2 aMin, Vector2 aMax)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.8f);
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        
        BoxCollider bc = go.AddComponent<BoxCollider>();
        bc.size = new Vector3(800, 200, 1);
        
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        MakeText(go.transform, "Text", text, 40, FontStyles.Bold, Vector2.zero, Vector2.one);
        return go;
    }
}
