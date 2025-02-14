using UnityEngine;

public class TutoAutoModeButtonInDefense : MonoBehaviour
{
    void Update()
    {
        if (StaticDataManager.IsAutoModeActive)
        {
            gameObject.SetActive(false);
        }
    }
}
