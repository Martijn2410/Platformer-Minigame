using UnityEngine;

public class CameraHolderFPS : MonoBehaviour
{

    public Transform cameraPosition;

    void Update()
    {
        transform.position = cameraPosition.position;
    }
}
