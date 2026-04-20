using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HiddenObjectInteractable : MonoBehaviour
{
    public string itemName;
    private XRSimpleInteractable interactable;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
            col = gameObject.AddComponent<BoxCollider>();
        
        if (col is BoxCollider bc) bc.size *= 1.5f;

        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            // Ajouter de maniere safe
            interactable = gameObject.AddComponent<XRSimpleInteractable>();
        }
        
        // Ajouter un tag pour le raycast
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    void OnEnable()
    {
        if (interactable != null)
            interactable.selectEntered.AddListener(OnSelected);
    }

    void OnDisable()
    {
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnSelected);
    }

    void OnSelected(SelectEnterEventArgs args)
    {
        if (HiddenObjectManager.Instance != null)
        {
            HiddenObjectManager.Instance.OnObjectFound(itemName, transform.position);
        }
    }
}
