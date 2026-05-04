using Unity.VisualScripting;
using UnityEngine;

public class PlayerObjectFacingCamera : MonoBehaviour
{
    Transform[] childs;

    void Start()
    {
        childs = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            childs[i] = transform.GetChild(i);
        }
        
        for (int i = 0; i < childs.Length; i++)
        {
            childs[i].rotation = Camera.main.transform.rotation;
        }
    }

    void Update()
    {
    }
}
