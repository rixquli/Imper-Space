using UnityEngine;

public class VillageMaker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // get all object with a script with the interface IDraggable

        foreach (Transform item in transform)
        {
            if (IsDraggable(item))
            {
                Debug.Log($"item.name: {item.name}, x:{item.position.x}, y:{item.position.y}");
            }
            else
            {
                foreach (Transform child in item)
                {
                    Debug.Log($"item.name: {child.name}, x:{child.position.x}, y:{child.position.y} , rotation: {child.rotation.z}");
                }
            }
        }
    }

    private bool IsDraggable(Transform item)
    {
        return item.GetComponent<IDraggable>() != null;
    }
}
