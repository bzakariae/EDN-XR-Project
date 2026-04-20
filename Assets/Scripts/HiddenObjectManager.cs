using UnityEngine;
using System.Collections.Generic;

public class HiddenObjectManager : MonoBehaviour
{
    public static HiddenObjectManager Instance;

    [Header("UI")]
    public HiddenObjectUI uiManager;

    [Header("Spawns & Items")]
    public Transform[] spawnPoints;
    
    [System.Serializable]
    public class HiddenItemDef
    {
        public string itemName;
        public GameObject prefab;
    }
    public List<HiddenItemDef> possibleItems;

    [Header("Effects")]
    public GameObject poofParticlePrefab;

    private int totalObjectsToFind = 0;
    private int objectsFound = 0;
    private HiddenItemDef currentTarget;
    private GameObject currentSpawnedObject;
    private List<Transform> availableSpawns;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // On s'assure d'afficher le menu de depart immediatement
        uiManager.ShowMainMenu();
    }

    public void StartGame(int objectCount)
    {
        // Calcule le nombre max d'objets possibles
        totalObjectsToFind = Mathf.Min(objectCount, possibleItems.Count);
        if (totalObjectsToFind <= 0)
        {
            Debug.LogError("[HiddenObjectManager] Aucun objet a trouver. Avez-vous genere les objets ?");
            return;
        }

        objectsFound = 0;
        availableSpawns = new List<Transform>(spawnPoints);

        uiManager.ShowGameHUD();
        uiManager.UpdateScore(objectsFound, totalObjectsToFind);
        
        SpawnNextObject();
    }

    void SpawnNextObject()
    {
        if (objectsFound >= totalObjectsToFind || availableSpawns.Count == 0)
        {
            uiManager.ShowGameOver();
            return;
        }

        // Selectionner un objet aleatoirement et le retirer de la liste
        int itemIndex = Random.Range(0, possibleItems.Count);
        currentTarget = possibleItems[itemIndex];
        possibleItems.RemoveAt(itemIndex);

        // Selectionner un point de cachette et le retirer
        int spawnIndex = Random.Range(0, availableSpawns.Count);
        Transform spawnPoint = availableSpawns[spawnIndex];
        availableSpawns.RemoveAt(spawnIndex);

        try
        {
            // Faire apparaitre l'objet
            currentSpawnedObject = Instantiate(currentTarget.prefab, spawnPoint.position, spawnPoint.rotation);
            currentSpawnedObject.transform.localScale *= 2.5f; // Rendre tres visible
            
            // Ajouter le composant d'interaction XR
            HiddenObjectInteractable interactable = currentSpawnedObject.GetComponent<HiddenObjectInteractable>();
            if (interactable == null)
                interactable = currentSpawnedObject.AddComponent<HiddenObjectInteractable>();
            
            interactable.itemName = currentTarget.itemName;

            // Afficher le nom de l'objet sur l'UI
            uiManager.UpdateTargetItem(currentTarget.itemName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur au spawn de l'objet : " + e.Message);
            objectsFound++;
            SpawnNextObject(); // Si erreur, on passe au suivant
        }
    }

    public void OnObjectFound(string foundItemName, Vector3 location)
    {
        // Verification si c'est bien l'objet recherche
        if (currentTarget != null && currentTarget.itemName == foundItemName)
        {
            objectsFound++;
            uiManager.UpdateScore(objectsFound, totalObjectsToFind);

            // Petit effet optionnel (desactive si null)
            if (poofParticlePrefab != null)
                Instantiate(poofParticlePrefab, location, Quaternion.identity);

            // Disparition de l'objet
            if (currentSpawnedObject != null)
                Destroy(currentSpawnedObject);

            // Victoire ou prochain objet ?
            if (objectsFound >= totalObjectsToFind)
                uiManager.ShowGameOver();
            else
                SpawnNextObject();
        }
    }

    public void ResetToMenu()
    {
        if (currentSpawnedObject != null) Destroy(currentSpawnedObject);
        // Si on veut recharger la scene complete (le plus propre)
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
