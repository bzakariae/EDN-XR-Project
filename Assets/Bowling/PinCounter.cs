using UnityEngine;
using TMPro;

/// <summary>
/// Compte le nombre de quilles tombées et met à jour l'affichage du score.
/// À attacher sur l'objet BowlingAlley (parent des quilles).
/// </summary>
public class PinCounter : MonoBehaviour
{
    [Tooltip("Tableau des quilles (rempli automatiquement au Start)")]
    private FallingPin[] pins;

    [Tooltip("Nombre de quilles tombées")]
    private int fallenCount;

    [Tooltip("Référence vers le texte UI du score")]
    public TextMeshProUGUI scoreText;

    void Start()
    {
        // Récupérer toutes les quilles enfants de cet objet
        pins = GetComponentsInChildren<FallingPin>();
    }

    void Update()
    {
        // Compter les quilles tombées
        fallenCount = 0;
        foreach (var pin in pins)
        {
            if (pin.isFallen)
                fallenCount++;
        }

        // Mettre à jour l'affichage du score
        if (scoreText != null)
        {
            scoreText.text = "Score : " + fallenCount + " / " + pins.Length;
        }
    }
}