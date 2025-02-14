using DG.Tweening;
using UnityEngine;

public class RandomEventManager : MonoBehaviour
{
    [SerializeField]
    private UDictionary<float, GameObject> percentagePerBonuToSpawn = new();
    [SerializeField]
    private Transform topLeftPosition;
    [SerializeField]
    private Transform bottomRightPosition;

    private void Start()
    {
        Debug.LogError("percentagePerBonuToSpawn.Count: " + percentagePerBonuToSpawn.Count); // 4
        foreach (var percentage in percentagePerBonuToSpawn)
        {
            Debug.LogError("percentage.Key: " + percentage.Key); // n'est appele que trois fois
            float random = Random.Range(0, 100);
            Debug.LogError(random);
            if (random <= percentage.Key)
            {
                Instantiate(percentage.Value, new Vector2(Random.Range(topLeftPosition.position.x, bottomRightPosition.position.x), Random.Range(topLeftPosition.position.y, bottomRightPosition.position.y)), Quaternion.identity);
            }
        }
    }
}
