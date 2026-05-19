using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HiddenObjectUI : MonoBehaviour
{
    [Header("Placement")]
    public Transform menuAnchor;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameHudPanel;
    public GameObject gameOverPanel;

    [Header("HUD Elements")]
    public TextMeshProUGUI targetItemText;
    public Image targetItemImage;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    [Header("Menu Buttons")]
    public Button btnFind5;
    public Button btnFind10;

    [Header("Game Over Elements")]
    public TextMeshProUGUI finalScoreText;
    public GameObject nameEntryPanel;
    public TMP_InputField playerNameInput;
    public Button btnSubmitName;
    public TextMeshProUGUI leaderboardText;
    public Button btnRestart;

    void Start()
    {
        EnsureRuntimeUI();

        if (btnFind5 != null)
            btnFind5.onClick.AddListener(() => StartGame(5));
        if (btnFind10 != null)
            btnFind10.onClick.AddListener(() => StartGame(10));
        if (btnRestart != null)
            btnRestart.onClick.AddListener(ReturnToMenu);
        if (btnSubmitName != null)
            btnSubmitName.onClick.AddListener(SubmitPlayerName);

        PlaceCanvas();
    }

    void EnsureRuntimeUI()
    {
        if (gameHudPanel != null)
        {
            if (timerText == null)
                timerText = MakeRuntimeText(gameHudPanel.transform, "TimerText", "03:00", 42, FontStyles.Bold, new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.96f));

            if (targetItemImage == null)
            {
                targetItemImage = MakeRuntimeImage(gameHudPanel.transform, "TargetItemImage", new Vector2(0.08f, 0.48f), new Vector2(0.3f, 0.78f));
                targetItemImage.preserveAspect = true;
                targetItemImage.gameObject.SetActive(false);
            }

            if (targetItemText != null)
            {
                RectTransform rt = targetItemText.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.32f, 0.48f);
                rt.anchorMax = new Vector2(0.95f, 0.78f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                targetItemText.alignment = TextAlignmentOptions.MidlineLeft;
            }
        }

        if (gameOverPanel != null)
        {
            if (finalScoreText != null)
            {
                RectTransform rt = finalScoreText.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.08f, 0.72f);
                rt.anchorMax = new Vector2(0.92f, 0.95f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                finalScoreText.fontSize = 42;
            }

            if (nameEntryPanel == null)
                nameEntryPanel = BuildNameEntryPanel(gameOverPanel.transform);

            if (leaderboardText == null)
                leaderboardText = MakeRuntimeText(gameOverPanel.transform, "LeaderboardText", "", 30, FontStyles.Normal, new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.72f));

            if (btnRestart != null)
            {
                RectTransform rt = btnRestart.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.25f, 0.06f);
                rt.anchorMax = new Vector2(0.75f, 0.22f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }
    }

    GameObject BuildNameEntryPanel(Transform parent)
    {
        GameObject panel = new GameObject("NameEntryPanel");
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.12f, 0.32f);
        rt.anchorMax = new Vector2(0.88f, 0.68f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        MakeRuntimeText(panel.transform, "NamePrompt", "Votre nom", 30, FontStyles.Bold, new Vector2(0f, 0.68f), new Vector2(1f, 1f));
        playerNameInput = MakeRuntimeInput(panel.transform, "PlayerNameInput", new Vector2(0f, 0.32f), new Vector2(0.65f, 0.62f));
        btnSubmitName = MakeRuntimeButton(panel.transform, "SubmitNameButton", "Valider", new Vector2(0.69f, 0.32f), new Vector2(1f, 0.62f));

        panel.SetActive(false);
        return panel;
    }

    void PlaceCanvas()
    {
        if (menuAnchor != null)
        {
            transform.SetPositionAndRotation(menuAnchor.position, menuAnchor.rotation);
            return;
        }

        PlaceCanvasInFrontOfPlayer();
    }

    void PlaceCanvasInFrontOfPlayer()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            transform.position = cam.transform.position + cam.transform.forward * 1.2f;
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
        }
    }

    void StartGame(int count)
    {
        if (HiddenObjectManager.Instance != null)
            HiddenObjectManager.Instance.StartGame(count);
    }

    void ReturnToMenu()
    {
        if (HiddenObjectManager.Instance != null)
            HiddenObjectManager.Instance.ResetToMenu();
    }

    void SubmitPlayerName()
    {
        string playerName = playerNameInput != null ? playerNameInput.text : "";
        if (HiddenObjectManager.Instance != null)
            HiddenObjectManager.Instance.SubmitPlayerName(playerName);
    }

    public void ShowMainMenu()
    {
        SetPanel(mainMenuPanel, true);
        SetPanel(gameHudPanel, false);
        SetPanel(gameOverPanel, false);
    }

    public void ShowGameHUD()
    {
        SetPanel(mainMenuPanel, false);
        SetPanel(gameHudPanel, true);
        SetPanel(gameOverPanel, false);
    }

    public void ShowVictoryNameEntry(int total, float elapsedSeconds)
    {
        SetPanel(mainMenuPanel, false);
        SetPanel(gameHudPanel, false);
        SetPanel(gameOverPanel, true);

        if (finalScoreText != null)
            finalScoreText.text = "Victoire !\n" + total + " objets trouves en " + HiddenObjectManager.FormatTime(elapsedSeconds);

        SetPanel(nameEntryPanel, true);
        SetTextActive(leaderboardText, false);

        if (playerNameInput != null)
            playerNameInput.text = "";
    }

    public void ShowTimeUp(int found, int total)
    {
        SetPanel(mainMenuPanel, false);
        SetPanel(gameHudPanel, false);
        SetPanel(gameOverPanel, true);

        if (finalScoreText != null)
            finalScoreText.text = "Temps ecoule !\n" + found + " / " + total + " objets trouves";

        SetPanel(nameEntryPanel, false);
        SetTextActive(leaderboardText, false);
    }

    public void ShowLeaderboard(int objectCount, string leaderboard)
    {
        SetPanel(mainMenuPanel, false);
        SetPanel(gameHudPanel, false);
        SetPanel(gameOverPanel, true);
        SetPanel(nameEntryPanel, false);

        if (finalScoreText != null)
            finalScoreText.text = "Classement - " + objectCount + " objets";

        if (leaderboardText != null)
        {
            leaderboardText.gameObject.SetActive(true);
            leaderboardText.text = leaderboard;
        }
    }

    public void UpdateTargetItem(string itemName, Sprite itemImage)
    {
        if (targetItemText != null)
            targetItemText.text = "Cherchez :\n<color=#FFD700><size=150%>" + itemName + "</size></color>";

        if (targetItemImage != null)
        {
            targetItemImage.sprite = itemImage;
            targetItemImage.gameObject.SetActive(itemImage != null);
        }
    }

    public void UpdateScore(int current, int total)
    {
        if (scoreText != null)
            scoreText.text = "Score : " + current + " / " + total;
    }

    public void UpdateTimer(float seconds)
    {
        if (timerText != null)
            timerText.text = "Temps : " + HiddenObjectManager.FormatTime(seconds);
    }

    void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    void SetTextActive(TextMeshProUGUI text, bool active)
    {
        if (text != null)
            text.gameObject.SetActive(active);
    }

    TextMeshProUGUI MakeRuntimeText(Transform parent, string name, string text, int size, FontStyles style, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return tmp;
    }

    Image MakeRuntimeImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image image = go.AddComponent<Image>();
        image.color = Color.white;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return image;
    }

    Button MakeRuntimeButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.4f, 0.8f);
        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        BoxCollider box = go.AddComponent<BoxCollider>();
        box.size = new Vector3(400, 100, 1);

        MakeRuntimeText(go.transform, "Text", label, 26, FontStyles.Bold, Vector2.zero, Vector2.one);
        return button;
    }

    TMP_InputField MakeRuntimeInput(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image background = go.AddComponent<Image>();
        background.color = Color.white;

        TMP_InputField input = go.AddComponent<TMP_InputField>();
        input.targetGraphic = background;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        TextMeshProUGUI text = MakeRuntimeText(go.transform, "Text", "", 24, FontStyles.Normal, new Vector2(0.04f, 0f), new Vector2(0.96f, 1f));
        text.color = Color.black;
        text.alignment = TextAlignmentOptions.MidlineLeft;

        TextMeshProUGUI placeholder = MakeRuntimeText(go.transform, "Placeholder", "Nom", 24, FontStyles.Italic, new Vector2(0.04f, 0f), new Vector2(0.96f, 1f));
        placeholder.color = new Color(0.45f, 0.45f, 0.45f);
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;

        input.textComponent = text;
        input.placeholder = placeholder;
        input.textViewport = rt;
        return input;
    }
}
