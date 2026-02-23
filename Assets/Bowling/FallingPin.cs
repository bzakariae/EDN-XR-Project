using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPin : MonoBehaviour
{

    public float fallAngleThreshold = 45f;


    public bool isFallen = false;

    void Start()
    {
        // Pas d'initialisation spécifique requise ici pour ce script
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.SetActive(!isFallen);

        if (!isFallen)
        {
            float angle = Vector3.Angle(transform.up, Vector3.up);
            if (angle < fallAngleThreshold)
            {
                isFallen = true;

            }
        }
    }
}