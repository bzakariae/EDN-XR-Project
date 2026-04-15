using UnityEngine;

public class CookEggs : MonoBehaviour
{
    public GameObject eggs;
    public GameObject omelette;

    void Update()
    {
        // Appuie sur E pour cuisiner
        if (Input.GetKeyDown(KeyCode.E))
        {
            Cook();
        }
    }

    void Cook()
    {
        if (eggs != null && omelette != null)
        {
            eggs.SetActive(false);
            omelette.SetActive(true);
        }
    }
}