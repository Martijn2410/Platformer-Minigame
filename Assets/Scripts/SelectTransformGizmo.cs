using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // Added for the new Input System
using RuntimeHandle;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SelectTransformGizmo : MonoBehaviour
{
    [Header("Input Actions")]
    [Tooltip("Reference to the click/tap action (Button)")]
    public InputActionReference clickAction;
    [Tooltip("Reference to the pointer's screen position (Vector2)")]
    public InputActionReference pointerPositionAction;

    [Header("Selection Visual")]
    [Tooltip("Material added on selection to render the object outline.")]
    public Material selectionOutlineMaterial;

    [Header("Selectable Objects Visual")]
    [Tooltip("Material added on top of existing materials for objects tagged Selectable.")]
    public Material selectableObjectsMaterial;

    private Transform selection;
    private readonly Dictionary<Renderer, Material[]> originalSharedMaterials = new Dictionary<Renderer, Material[]>();
    private readonly Dictionary<Renderer, Material[]> originalSelectableSharedMaterials = new Dictionary<Renderer, Material[]>();

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

        Deselect();
        RestoreSelectableObjectsMaterial();
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

        // Resolve hits from nearest to farthest so front-most targets win.
        RaycastHit[] hits = Physics.RaycastAll(ray);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit currentHit in hits)
        {
            Transform selectableTarget = GetSelectableTarget(currentHit.transform);
            if (selectableTarget != null)
            {
                Select(selectableTarget);
                return;
            }

            if (currentHit.transform.gameObject.layer == runtimeTransformLayer ||
                currentHit.transform.GetComponentInParent<HandleBase>() != null)
            {
                return;
            }
        }

        // No selectable object or gizmo was hit.
        Deselect();
    }

    Transform GetSelectableTarget(Transform hitTransform)
    {
        Transform current = hitTransform;
        while (current != null)
        {
            if (current.CompareTag("Selectable"))
                return current;

            current = current.parent;
        }

        return null;
    }

    void Select(Transform target)
    {
        if (selection == target)
            return;

        ClearSelectionOutline();

        selection = target;
        ApplySelectionOutline(selection);

        runtimeTransformHandle.target = selection;

        // Ensure gizmo and all children are on correct layer
        SetLayerRecursively(runtimeTransformGameObj, runtimeTransformLayer);

        runtimeTransformGameObj.SetActive(true);
    }

    void Deselect()
    {
        if (selection == null)
            return;

        ClearSelectionOutline();
        selection = null;
        runtimeTransformHandle.target = null;
        runtimeTransformGameObj.SetActive(false);
    }

    void OnDestroy()
    {
        ClearSelectionOutline();
        RestoreSelectableObjectsMaterial();
    }

    public void DeselectCurrent()
    {
        Deselect();
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

    void ApplySelectionOutline(Transform target)
    {
        if (selectionOutlineMaterial == null || target == null)
            return;

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            Material[] original = renderer.sharedMaterials;
            if (original == null || original.Length == 0)
                continue;

            // Skip if already outlined.
            if (original[original.Length - 1] == selectionOutlineMaterial)
                continue;

            originalSharedMaterials[renderer] = original;

            Material[] outlined = new Material[original.Length + 1];
            Array.Copy(original, outlined, original.Length);
            outlined[outlined.Length - 1] = selectionOutlineMaterial;
            renderer.sharedMaterials = outlined;
        }
    }

    void ClearSelectionOutline()
    {
        foreach (KeyValuePair<Renderer, Material[]> entry in originalSharedMaterials)
        {
            if (entry.Key != null)
                entry.Key.sharedMaterials = entry.Value;
        }

        originalSharedMaterials.Clear();
    }

    public void ApplySelectableObjectsMaterial()
    {
        RestoreSelectableObjectsMaterial();

        if (selectableObjectsMaterial == null)
            return;

        HashSet<Renderer> selectableRenderers = new HashSet<Renderer>();
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            CollectSelectableRenderers(rootObject.transform, selectableRenderers);
        }

        foreach (Renderer renderer in selectableRenderers)
        {
            if (renderer == null)
                continue;

            Material[] original = renderer.sharedMaterials;
            if (original == null || original.Length == 0)
                continue;

            originalSelectableSharedMaterials[renderer] = original;

            Material[] overlaid = new Material[original.Length + 1];
            Array.Copy(original, overlaid, original.Length);
            overlaid[overlaid.Length - 1] = selectableObjectsMaterial;

            renderer.sharedMaterials = overlaid;
        }
    }

    public void RestoreSelectableObjectsMaterial()
    {
        foreach (KeyValuePair<Renderer, Material[]> entry in originalSelectableSharedMaterials)
        {
            if (entry.Key != null)
                entry.Key.sharedMaterials = entry.Value;
        }

        originalSelectableSharedMaterials.Clear();
    }

    void CollectSelectableRenderers(Transform current, HashSet<Renderer> selectableRenderers)
    {
        if (current.CompareTag("Selectable"))
        {
            Renderer[] renderers = current.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                selectableRenderers.Add(renderer);
            }
        }

        foreach (Transform child in current)
        {
            CollectSelectableRenderers(child, selectableRenderers);
        }
    }
}