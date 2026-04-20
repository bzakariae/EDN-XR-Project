using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HiddenObjectUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameHudPanel;
    public GameObject gameOverPanel;

    [Header("HUD Elements")]
    public TextMeshProUGUI targetItemText;
    public TextMeshProUGUI scoreText;

    [Header("Menu Buttons")]
    public Button btnFind5;
    public Button btnFind10;
    
    [Header("Game Over Elements")]
    public TextMeshProUGUI finalScoreText;
    public Button btnRestart;

    void Start()
    {
        // On branche les boutons pour qu'ils lancent le jeu
        btnFind5.onClick.AddListener(() => StartGame(5));
        btnFind10.onClick.AddListener(() => StartGame(10));
        btnRestart.onClick.AddListener(ReturnToMenu);

        // Au demarrage, on deplace le menu pile devant les yeux du joueur (Camera)
        PlaceCanvasInFrontOfPlayer();
    }

    void PlaceCanvasInFrontOfPlayer()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            // On place le canvas 1.2 metres devant le casque du joueur, et a hauteur de ses yeux
            transform.position = cam.transform.position + cam.transform.forward * 1.2f;
            // On le tourne pour qu'il regarde le joueur
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
        }
    }

    void StartGame(int count)
    {
        HiddenObjectManager.Instance.StartGame(count);
    }

    void ReturnToMenu()
    {
        HiddenObjectManager.Instance.ResetToMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        gameHudPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void ShowGameHUD()
    {
        mainMenuPanel.SetActive(false);
        gameHudPanel.SetActive(true);
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        mainMenuPanel.SetActive(false);
        gameHudPanel.SetActive(false);
        gameOverPanel.SetActive(true);
    }

    public void UpdateTargetItem(string itemName)
    {
        // Traduire le nom ou l'afficher en grand
        if (targetItemText != null)
            targetItemText.text = "Cherchez l'objet :\n<color=#FFD700><size=150%>" + itemName + "</size></color>";
    }

    public void UpdateScore(int current, int total)
    {
        if (scoreText != null)
            scoreText.text = "Score : " + current + " / " + total;
            
        if (finalScoreText != null)
            finalScoreText.text = "Victoire !\nVous avez trouve les " + total + " objets !";
    }
}
