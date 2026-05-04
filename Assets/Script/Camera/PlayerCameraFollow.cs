using Unity.Netcode;
using UnityEngine;

public class PlayerCameraFollow : NetworkBehaviour
{
    private Vector3 cameraOffset;

    void Start()
    {
        if (IsOwner)
        {
            cameraOffset = Camera.main.transform.position - transform.position;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            Camera.main.transform.position = transform.position + cameraOffset;
        }
        
    }
}
