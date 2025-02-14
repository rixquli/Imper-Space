using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Burst;

public class HealthAndLevelBarManager : MonoBehaviour
{
    private static HealthAndLevelBarManager _instance;
    public static HealthAndLevelBarManager instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject managerObject = new GameObject("HealthAndLevelBarManager");
                _instance = managerObject.AddComponent<HealthAndLevelBarManager>();
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    public GameObject healthBarPrefab;
    public GameObject levelBarPrefab;
    private Dictionary<Transform, Transform> bars = new Dictionary<Transform, Transform>();

    private TransformAccessArray transformAccessArray;
    private NativeArray<float3> parentPositions;

    private void Awake()
    {
        // Ensure only one instance exists
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
        transformAccessArray = new TransformAccessArray(0);
    }

    public void Add(Transform parentTransform, Transform healthBar, float offsetY = 2f)
    {
        GameObject go = new GameObject("Bars");
        WorldCanvasManager.Instance.AddUIElementToCanvas(go);

        healthBar.transform.SetParent(go.transform);
        if (healthBar.TryGetComponent(out RectTransform healthBarRect))
        {
            healthBarRect.position = new Vector2(0, offsetY);
        }

        bars.Add(parentTransform, go.transform);
        transformAccessArray.Add(go.transform);
    }
    public void Add(Transform parentTransform, Transform healthBar, Transform levelBar)
    {
        GameObject go = new GameObject("Bars");
        WorldCanvasManager.Instance.AddUIElementToCanvas(go);

        healthBar.transform.SetParent(go.transform);
        levelBar.transform.SetParent(go.transform);
        if (healthBar.TryGetComponent(out RectTransform healthBarRect))
        {
            healthBarRect.position = new Vector2(0, 2);
        }
        if (levelBar.TryGetComponent(out RectTransform levelBarRect))
        {
            levelBarRect.position = new Vector2(0, 2.5f);
        }

        bars.Add(parentTransform, go.transform);
        transformAccessArray.Add(go.transform);
    }

    private void OnDestroy()
    {
        if (transformAccessArray.isCreated)
        {
            transformAccessArray.Dispose();
        }
        if (parentPositions.IsCreated)
        {
            parentPositions.Dispose();
        }
    }

    private void Update()
    {
        if (transformAccessArray.length == 0) return;

        if (parentPositions.IsCreated)
        {
            parentPositions.Dispose();
        }

        parentPositions = new NativeArray<float3>(bars.Count, Allocator.TempJob);

        int index = 0;
        List<Transform> keysToRemove = new List<Transform>();

        foreach (var kvp in bars)
        {
            if (kvp.Key == null)
            {
                keysToRemove.Add(kvp.Key);
            }
            else
            {
                parentPositions[index] = kvp.Key.position;
                index++;
            }
        }

        foreach (var key in keysToRemove)
        {
            if (bars.TryGetValue(key, out Transform barTransform) && barTransform != null)
            {
                Destroy(barTransform.gameObject);
                bars.Remove(key);
            }
        }

        // Rebuild the TransformAccessArray
        Transform[] remainingTransforms = new Transform[bars.Count];
        int i = 0;
        foreach (var kvp in bars)
        {
            remainingTransforms[i] = kvp.Value;
            i++;
        }

        transformAccessArray.Dispose();
        transformAccessArray = new TransformAccessArray(remainingTransforms);

        if (parentPositions.Length == 0) return;

        var updateJob = new UpdateBarJob
        {
            parentPositions = parentPositions,
            offset = new float2(0, 0f)
        };

        JobHandle updateHandle = updateJob.Schedule(transformAccessArray);
        updateHandle.Complete();

        parentPositions.Dispose();
    }

    private struct UpdateBarJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<float3> parentPositions;
        public float2 offset;

        public void Execute(int index, TransformAccess transform)
        {
            float3 parentPosition = parentPositions[index];
            float3 newPosition = parentPosition + new float3(0, offset.y, 0);
            transform.position = newPosition;
        }
    }
}