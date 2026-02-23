using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // [cite: 58] Obligatoire pour utiliser TextMeshProUGUI

public class PinCounter : MonoBehaviour
{
     private FallingPin[] pins; 
     public int fallenCount = 0; 

    public TextMeshProUGUI scoreText;

    void Start()
    {
        
        pins = GetComponentsInChildren<FallingPin>();
    }

    void Update()
    {
        int currentCount = 0; 

        foreach (FallingPin pin in pins) 
        {
             if (pin.isFallen) 
            {
                 currentCount++; 
            }
        }

        fallenCount = currentCount; 

        if (scoreText != null)
        {
            scoreText.text = "Score: " + fallenCount.ToString();
        }
    }
}