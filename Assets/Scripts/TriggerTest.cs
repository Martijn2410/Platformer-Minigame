using UnityEngine;

public class TriggerTest : MonoBehaviour
{

//  void OnTriggerEnter (Collider other)
//     {
//         Renderer render = GetComponent<Renderer>();
//         render.material.color = Color.green;
//         Debug.Log("Triggered");
//     }

    public CameraManager cameraManager;
    public SelectTransformGizmo selectTransformGizmo;
    private Renderer render;
    private Color originalColor;

    void Start()
    {
        render = GetComponent<Renderer>();
        originalColor = render.material.color;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // optional but recommended
        {
            render.material.color = Color.green;
            if (selectTransformGizmo != null)
            {
                selectTransformGizmo.ApplySelectableObjectsMaterial();
            }
            Cursor.lockState = CursorLockMode.None;
            cameraManager.SwitchCamera(cameraManager.builderCam);
            Cursor.visible = true;
            Debug.Log("Entered trigger");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            render.material.color = originalColor;
            Cursor.lockState = CursorLockMode.Locked;
            cameraManager.SwitchCamera(cameraManager.thirdPersonCam);
            Cursor.visible = false;
            if (selectTransformGizmo != null)
            {
                selectTransformGizmo.DeselectCurrent();
                selectTransformGizmo.RestoreSelectableObjectsMaterial();
            }
            Debug.Log("Exited trigger");
        }
    }
}
