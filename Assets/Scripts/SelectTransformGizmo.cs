using UnityEngine;
using UnityEngine.EventSystems;
using RuntimeHandle;

public class SelectTransformGizmo : MonoBehaviour
{
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

    void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        // Prevent clicking through UI (safe check)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 🔥 IMPORTANT: First check if we hit the gizmo
        // if (Physics.Raycast(ray, out RaycastHit handleHit, Mathf.Infinity, runtimeTransformLayerMask))
        // {
        //     // Clicked on gizmo → do nothing
        //     return;
        // }

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

    // 🔥 Cleaner recursive version (no depth limit)
    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}