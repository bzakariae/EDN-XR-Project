using UnityEngine;
using System;
using System.Collections.Generic;

public class HiddenObjectManager : MonoBehaviour
{
    public static HiddenObjectManager Instance;
    const string HiddenSpawnsRootName = "[HiddenSpawns]";
    const string LeaderboardKeyPrefix = "HiddenObjectLeaderboard_";

    [Header("UI")]
    public HiddenObjectUI uiManager;

    [Header("Spawns & Items")]
    public GameObject[] spawnPointObjects;
    public Transform[] spawnPoints;

    [Serializable]
    public class HiddenItemDef
    {
        public string itemName;
        public GameObject prefab;
        public Sprite itemImage;
    }

    public List<HiddenItemDef> possibleItems;

    [Header("Effects")]
    public GameObject poofParticlePrefab;

    [Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public float bestTimeSeconds;
    }

    [Serializable]
    public class LeaderboardData
    {
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
    }

    private int gameModeObjectCount;
    private int totalObjectsToFind;
    private int objectsFound;
    private float timeLimitSeconds;
    private float remainingTimeSeconds;
    private bool gameRunning;
    private HiddenItemDef currentTarget;
    private GameObject currentSpawnedObject;
    private List<HiddenItemDef> availableItems;
    private List<Transform> availableSpawns;
    private Dictionary<GameObject, Sprite> generatedItemImages = new Dictionary<GameObject, Sprite>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (uiManager != null)
            uiManager.ShowMainMenu();
    }

    void Update()
    {
        if (!gameRunning)
            return;

        remainingTimeSeconds -= Time.deltaTime;
        if (uiManager != null)
            uiManager.UpdateTimer(Mathf.Max(0f, remainingTimeSeconds));

        if (remainingTimeSeconds <= 0f)
            LoseGame();
    }

    public void StartGame(int objectCount)
    {
        availableItems = possibleItems != null
            ? new List<HiddenItemDef>(possibleItems)
            : new List<HiddenItemDef>();
        availableSpawns = BuildSpawnList();

        gameModeObjectCount = objectCount;
        totalObjectsToFind = Mathf.Min(objectCount, availableItems.Count, availableSpawns.Count);
        if (totalObjectsToFind <= 0)
        {
            Debug.LogError("[HiddenObjectManager] Aucun objet/cachette a utiliser. Avez-vous configure les objets et les positions ?");
            return;
        }

        objectsFound = 0;
        timeLimitSeconds = GetTimeLimitForMode(objectCount);
        remainingTimeSeconds = timeLimitSeconds;
        gameRunning = true;

        if (uiManager != null)
        {
            uiManager.ShowGameHUD();
            uiManager.UpdateScore(objectsFound, totalObjectsToFind);
            uiManager.UpdateTimer(remainingTimeSeconds);
        }

        SpawnNextObject();
    }

    void SpawnNextObject()
    {
        if (!gameRunning)
            return;

        if (objectsFound >= totalObjectsToFind || availableSpawns.Count == 0)
        {
            WinGame();
            return;
        }

        int itemIndex = UnityEngine.Random.Range(0, availableItems.Count);
        currentTarget = availableItems[itemIndex];
        availableItems.RemoveAt(itemIndex);

        int spawnIndex = UnityEngine.Random.Range(0, availableSpawns.Count);
        Transform spawnPoint = availableSpawns[spawnIndex];
        availableSpawns.RemoveAt(spawnIndex);

        try
        {
            currentSpawnedObject = Instantiate(currentTarget.prefab, spawnPoint.position, spawnPoint.rotation);
            currentSpawnedObject.transform.localScale *= 2.5f;

            HiddenObjectInteractable interactable = currentSpawnedObject.GetComponent<HiddenObjectInteractable>();
            if (interactable == null)
                interactable = currentSpawnedObject.AddComponent<HiddenObjectInteractable>();

            interactable.itemName = currentTarget.itemName;

            if (uiManager != null)
                uiManager.UpdateTargetItem(currentTarget.itemName, GetDisplayImage(currentTarget));
        }
        catch (Exception e)
        {
            Debug.LogError("Erreur au spawn de l'objet : " + e.Message);
            objectsFound++;
            SpawnNextObject();
        }
    }

    public void OnObjectFound(string foundItemName, Vector3 location)
    {
        if (!gameRunning)
            return;

        if (currentTarget != null && currentTarget.itemName == foundItemName)
        {
            objectsFound++;

            if (uiManager != null)
                uiManager.UpdateScore(objectsFound, totalObjectsToFind);

            if (poofParticlePrefab != null)
                Instantiate(poofParticlePrefab, location, Quaternion.identity);

            if (currentSpawnedObject != null)
                Destroy(currentSpawnedObject);

            if (objectsFound >= totalObjectsToFind)
                WinGame();
            else
                SpawnNextObject();
        }
    }

    public void SubmitPlayerName(string playerName)
    {
        if (gameModeObjectCount != 5 && gameModeObjectCount != 10)
            gameModeObjectCount = totalObjectsToFind <= 5 ? 5 : 10;

        float elapsedSeconds = timeLimitSeconds - remainingTimeSeconds;
        string cleanName = string.IsNullOrWhiteSpace(playerName) ? "Joueur" : playerName.Trim();

        LeaderboardData data = LoadLeaderboard(gameModeObjectCount);
        LeaderboardEntry existing = data.entries.Find(entry =>
            string.Equals(entry.playerName, cleanName, StringComparison.OrdinalIgnoreCase));

        if (existing == null)
        {
            data.entries.Add(new LeaderboardEntry
            {
                playerName = cleanName,
                bestTimeSeconds = elapsedSeconds
            });
        }
        else if (elapsedSeconds < existing.bestTimeSeconds)
        {
            existing.playerName = cleanName;
            existing.bestTimeSeconds = elapsedSeconds;
        }

        data.entries.Sort((a, b) => a.bestTimeSeconds.CompareTo(b.bestTimeSeconds));
        if (data.entries.Count > 10)
            data.entries.RemoveRange(10, data.entries.Count - 10);

        SaveLeaderboard(gameModeObjectCount, data);

        if (uiManager != null)
            uiManager.ShowLeaderboard(gameModeObjectCount, FormatLeaderboard(data));
    }

    void WinGame()
    {
        if (!gameRunning)
            return;

        gameRunning = false;

        if (currentSpawnedObject != null)
            Destroy(currentSpawnedObject);

        float elapsedSeconds = timeLimitSeconds - remainingTimeSeconds;
        if (uiManager != null)
            uiManager.ShowVictoryNameEntry(totalObjectsToFind, elapsedSeconds);
    }

    void LoseGame()
    {
        if (!gameRunning)
            return;

        gameRunning = false;
        remainingTimeSeconds = 0f;

        if (currentSpawnedObject != null)
            Destroy(currentSpawnedObject);

        if (uiManager != null)
            uiManager.ShowTimeUp(objectsFound, totalObjectsToFind);
    }

    List<Transform> BuildSpawnList()
    {
        List<Transform> result = new List<Transform>();

        GameObject hiddenSpawnsRoot = GameObject.Find(HiddenSpawnsRootName);
        if (hiddenSpawnsRoot != null)
        {
            foreach (Transform child in hiddenSpawnsRoot.transform)
            {
                if (child.name.StartsWith("Spawn_"))
                    result.Add(child);
            }
        }

        if (spawnPointObjects != null)
        {
            foreach (GameObject spawnObject in spawnPointObjects)
            {
                if (spawnObject != null && !result.Contains(spawnObject.transform))
                    result.Add(spawnObject.transform);
            }
        }

        if (result.Count == 0 && spawnPoints != null)
        {
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                    result.Add(spawnPoint);
            }
        }

        return result;
    }

    float GetTimeLimitForMode(int objectCount)
    {
        return objectCount >= 10 ? 5f * 60f : 3f * 60f;
    }

    Sprite GetDisplayImage(HiddenItemDef item)
    {
        if (item == null)
            return null;

        if (item.itemImage != null)
            return item.itemImage;

        if (item.prefab == null)
            return null;

        if (generatedItemImages.TryGetValue(item.prefab, out Sprite cachedSprite))
            return cachedSprite;

        SpriteRenderer spriteRenderer = item.prefab.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            generatedItemImages[item.prefab] = spriteRenderer.sprite;
            return spriteRenderer.sprite;
        }

        Renderer renderer = item.prefab.GetComponentInChildren<Renderer>();
        Texture2D texture = renderer != null && renderer.sharedMaterial != null
            ? renderer.sharedMaterial.mainTexture as Texture2D
            : null;

        if (texture == null)
            return null;

        Sprite generatedSprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));

        generatedItemImages[item.prefab] = generatedSprite;
        return generatedSprite;
    }

    LeaderboardData LoadLeaderboard(int objectCount)
    {
        string json = PlayerPrefs.GetString(GetLeaderboardKey(objectCount), "");
        if (string.IsNullOrEmpty(json))
            return new LeaderboardData();

        LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(json);
        if (data == null)
            data = new LeaderboardData();
        if (data.entries == null)
            data.entries = new List<LeaderboardEntry>();

        return data;
    }

    void SaveLeaderboard(int objectCount, LeaderboardData data)
    {
        PlayerPrefs.SetString(GetLeaderboardKey(objectCount), JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    string GetLeaderboardKey(int objectCount)
    {
        return LeaderboardKeyPrefix + objectCount;
    }

    string FormatLeaderboard(LeaderboardData data)
    {
        if (data.entries.Count == 0)
            return "Aucun score pour le moment.";

        List<string> lines = new List<string>();
        for (int i = 0; i < data.entries.Count; i++)
        {
            LeaderboardEntry entry = data.entries[i];
            lines.Add((i + 1) + ". " + entry.playerName + " - " + FormatTime(entry.bestTimeSeconds));
        }

        return string.Join("\n", lines);
    }

    public static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(seconds));
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;
        return minutes.ToString("00") + ":" + remainingSeconds.ToString("00");
    }

    public void ResetToMenu()
    {
        if (currentSpawnedObject != null)
            Destroy(currentSpawnedObject);

        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
