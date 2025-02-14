using System;
using CodeMonkey.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TroopsPlacementsManager : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private InputAction press,
        screenPos;

    [Serializable]
    class Corners
    {
        public Vector2 topLeft,
            bottomRight;
    }

    [SerializeField]
    Corners[] spawnableAreas;

    private Vector3 curScreenPos;
    private Vector3 WorldPos
    {
        get
        {
            float z = _camera.WorldToScreenPoint(transform.position).z;
            return _camera.ScreenToWorldPoint(curScreenPos + new Vector3(0, 0, z));
        }
    }

    public Plane plane = new Plane(Vector3.forward, Vector3.zero);

    private void OnEnable()
    {
        screenPos.Enable();
        press.Enable();

        screenPos.performed += context =>
        {
            curScreenPos = context.ReadValue<Vector2>();
        };
        press.performed += _ => OnClick();
    }

    private void OnDisable()
    {
        screenPos.performed -= context =>
        {
            curScreenPos = context.ReadValue<Vector2>();
        };
        press.performed -= _ => OnClick();
        screenPos.Disable();
        press.Disable();
    }

    private void OnClick()
    {
        if (GlobalAttackVillageManager.Instance.hadStartGame)
            return;

        if (UtilsClass.IsPointerOverUI())
            return;
        if (
            GlobalAttackVillageManager.Instance.selectedTroop.amount > 0
            && IsInSpawnableArea(WorldPos)
        )
        {
            GlobalAttackVillageManager.Instance.selectedTroop.amount--;
            GlobalAttackVillageManager.Instance.selectedTroop.troopsCard?.SetAmount(
                GlobalAttackVillageManager.Instance.selectedTroop.amount.ToString()
            );
            PlaceTroop(GlobalAttackVillageManager.Instance.selectedTroop.troopsData, WorldPos);
        }
    }

    private float Vector2ToAngle(Vector2 direction)
    {
        // Mathf.Atan2 returns the angle in radians
        float angleInRadians = Mathf.Atan2(direction.y, direction.x);

        // Convert radians to degrees
        float angleInDegrees = angleInRadians * Mathf.Rad2Deg;

        return angleInDegrees;
    }

    private void PlaceTroop(TroopsData troopsData, Vector2 position)
    {
        // Calculate the direction from the troop's position to the origin (0, 0)
        Vector2 directionToOrigin = Vector2.zero - position;

        // Get the angle to the origin
        float angleToOrigin = Vector2ToAngle(directionToOrigin);

        // Instantiate the troop object and rotate it to face the origin
        GameObject troopObject = Instantiate(
            troopsData.previewPrefab,
            position,
            Quaternion.Euler(0, 0, angleToOrigin)
        );

        if (troopsData.id == 0)
        { // this is the player troop
            GlobalAttackVillageManager.Instance.playerTransform = troopObject.transform;
        }

        PreviewForAttackVillageMode component =
            troopObject.AddComponent<PreviewForAttackVillageMode>();
        // if this troop is the player pass true as argument
        component.SetPrefab(troopsData.prefab, troopsData.id == 0);

        GlobalAttackVillageManager.Instance.aliveAllies++;
    }

    private bool IsInSpawnableArea(Vector2 position)
    {
        Debug.Log($"tested position:{position}");

        foreach (Corners area in spawnableAreas)
        {
            // Check if position is within the rectangular bounds
            if (
                position.x >= area.topLeft.x
                && position.x <= area.bottomRight.x
                && position.y <= area.topLeft.y
                && position.y >= area.bottomRight.y
            )
            {
                return true; // Position is inside this spawnable area
            }
        }

        return false; // Position is not inside any spawnable area
    }
}
