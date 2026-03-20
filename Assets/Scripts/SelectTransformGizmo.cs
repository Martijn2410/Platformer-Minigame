using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // Added for the new Input System
using RuntimeHandle;

public class SelectTransformGizmo : MonoBehaviour
{
    [Header("Input Actions")]
    [Tooltip("Reference to the click/tap action (Button)")]
    public InputActionReference clickAction;
    [Tooltip("Reference to the pointer's screen position (Vector2)")]
    public InputActionReference pointerPositionAction;

    private Transform selection;

    private GameObject runtimeTransformGameObj;
    public RuntimeTransformHandle runtimeTransformHandle;

    private int runtimeTransformLayer = 6;
    private int runtimeTransformLayerMask;

    void Start()
    {
        runtimeTransformGameObj = new GameObject("RuntimeTransformHandle");
        runtimeTransformHandle = runtimeTransformGameObj.AddComponent<RuntimeTransformHandle>();

        runtimeTransformGameObj.layer = runtimeTransformLayer;
        runtimeTransformLayerMask = 1 << runtimeTransformLayer;

        runtimeTransformHandle.type = HandleType.POSITION;
        runtimeTransformHandle.autoScale = true;
        runtimeTransformHandle.autoScaleFactor = 1.0f;
        runtimeTransformHandle.space = HandleSpace.WORLD;

        runtimeTransformGameObj.SetActive(false);
    }

    // Enable our input actions
    private void OnEnable()
    {
        if (clickAction != null) clickAction.action.Enable();
        if (pointerPositionAction != null) pointerPositionAction.action.Enable();
    }

    // Disable our input actions
    private void OnDisable()
    {
        if (clickAction != null) clickAction.action.Disable();
        if (pointerPositionAction != null) pointerPositionAction.action.Disable();
    }

    void Update()
    {
        // Safety check to ensure actions are assigned
        if (clickAction == null || pointerPositionAction == null) return;

        // --- NEW INPUT SYSTEM: Click Check ---
        if (!clickAction.action.WasPressedThisFrame())
            return;

        // Prevent clicking through UI (safe check)
        // Note: This still works perfectly as long as your EventSystem is updated!
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // --- NEW INPUT SYSTEM: Pointer Position ---
        Vector2 screenPosition = pointerPositionAction.action.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        // Then check normal objects
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Transform target = hit.transform;

            if (target.CompareTag("Selectable"))
            {
                Select(target);
            }
            else
            {
                Deselect();
            }
        }
        else
        {
            // Clicked empty space
            Deselect();
        }
    }

    void Select(Transform target)
    {
        if (selection == target)
            return;

        selection = target;

        runtimeTransformHandle.target = selection;

        // Ensure gizmo and all children are on correct layer
        SetLayerRecursively(runtimeTransformGameObj, runtimeTransformLayer);

        runtimeTransformGameObj.SetActive(true);
    }

    void Deselect()
    {
        if (selection == null)
            return;

        selection = null;
        runtimeTransformGameObj.SetActive(false);
    }

    // Cleaner recursive version (no depth limit)
    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}