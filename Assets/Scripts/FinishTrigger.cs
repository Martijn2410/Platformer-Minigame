using UnityEngine;

public class HideCanvasOnContact : MonoBehaviour
{
    [Tooltip("Drag the Canvas or UI GameObject you want to hide here in the Inspector.")]
    public GameObject canvasElement;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger has the "Player" tag
        if (other.CompareTag("Player"))
        {
            // Hide the canvas element
            if (canvasElement != null)
            {
                canvasElement.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the object exiting the trigger has the "Player" tag
        if (other.CompareTag("Player"))
        {
            // Show the canvas element again when the player leaves
            if (canvasElement != null)
            {
                canvasElement.SetActive(false);
            }
        }
    }
}