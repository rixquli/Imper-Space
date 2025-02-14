using UnityEngine;
using UnityEngine.UI;
public class PlayerController : MonoBehaviour
{
  public GameObject controllerUIPrefab;

  void Start()
  {
    if (!controllerUIPrefab) return;
    GameObject camera = GameObject.Find("Main Camera");
    if (camera.TryGetComponent<FollowPlayer>(out FollowPlayer component))
      component.PlayerTransform = transform;

    PlayerEntity playerEntity = gameObject.GetComponent<PlayerEntity>();

    // Check if UIController and Entity are valid
    if (UIController.Instance != null && playerEntity != null)
    {
      UIController.Instance.player = playerEntity;
    }
    else
    {
      Debug.LogWarning("UIController or Entity is not set correctly.");
    }

    // Find the GameUI object in the scene
    GameObject gameUIGameObject = UIController.Instance.gameUIGameObject;

    if (gameUIGameObject == null)
    {
      Debug.LogError("GameUI GameObject not found!");
      return;
    }

    // Instantiate the controller UI prefab as a child of GameUI
    GameObject controllers = Instantiate(controllerUIPrefab, gameUIGameObject.transform);
    controllers.transform.SetSiblingIndex(0);
    UIController.Instance.playerController = controllers;

    // Find the PlayerMouvement component on the current game object
    PlayerMouvement playerMouvement = gameObject.GetComponent<PlayerMouvement>();

    if (playerMouvement == null)
    {
      Debug.LogError("PlayerMouvement component not found on this GameObject!");
      return;
    }

    // Find the "Mouvement Joystick" within the instantiated controllers
    Transform joystickMouvementTransform = controllers.transform.Find("Mouvement Joystick");

    if (joystickMouvementTransform == null)
    {
      Debug.LogError("Mouvement Joystick not found in the instantiated controllers!");
      return;
    }

    // Assign the found joystick GameObject to the mouvementJoystick field
    playerMouvement.mouvementJoystick = joystickMouvementTransform.gameObject.GetComponent<VariableJoystick>();

    // Find the ShieldSystem component on the current game object
    ShieldSystem shieldSystem = gameObject.GetComponent<ShieldSystem>();

    if (shieldSystem == null)
    {
      Debug.LogError("ShieldSystem component not found on this GameObject!");
      return;
    }

    // Find the "Shield Joystick" within the instantiated controllers
    Transform joystickShieldTransform = controllers.transform.Find("Shield Joystick");

    if (joystickShieldTransform == null)
    {
      Debug.LogError("Shield Joystick not found in the instantiated controllers!");
      return;
    }

    // Assign the found joystick GameObject to the mouvementJoystick field
    shieldSystem.joystick = joystickShieldTransform.gameObject.GetComponent<VariableJoystick>();

    // Find the ShieldSystem component on the current game object
    ShootSystem shootSystem = gameObject.GetComponent<ShootSystem>();

    if (shootSystem == null)
    {
      Debug.LogError("ShootSystem component not found on this GameObject!");
      return;
    }

    // Find the "Mouvement Joystick" within the instantiated controllers
    Transform joystickShootTransform = controllers.transform.Find("Shoot Joystick");

    if (joystickShootTransform == null)
    {
      Debug.LogError("Shoot Joystick not found in the instantiated controllers!");
      return;
    }

    // Assign the found joystick GameObject to the mouvementJoystick field
    shootSystem.joystick = joystickShootTransform.gameObject.GetComponent<VariableJoystick>();

    // RadarUi
    Transform radarTransform = controllers.transform.Find("Radar UI");

    if (radarTransform == null)
    {
      Debug.LogError("Radar not found in the instantiated controllers!");
      return;
    }

    radarTransform.gameObject.GetComponent<Ilumisoft.RadarSystem.Radar>().Player = gameObject;

    // GameOver Screen
    Transform gameOverTransform = controllers.transform.Find("GameOver");

    if (gameOverTransform == null)
    {
      Debug.LogError("Radar not found in the instantiated controllers!");
      return;
    }

    UIController.Instance.gameOverScreen = gameOverTransform.gameObject;

    Transform resumGameTransform = controllers.transform.Find("ResumGame");

    if (resumGameTransform == null)
    {
      Debug.LogError("Radar not found in the instantiated controllers!");
      return;
    }

    UIController.Instance.resumGameScreen = resumGameTransform.gameObject;

    Transform victoryTransform = controllers.transform.Find("Victory");

    if (victoryTransform == null)
    {
      Debug.LogError("Radar not found in the instantiated controllers!");
      return;
    }

    UIController.Instance.victoryScreen = victoryTransform.gameObject;

    DashEnergyController dashEnergyController = controllers.GetComponentInChildren<DashEnergyController>();

    if (dashEnergyController == null)
    {
      Debug.LogError("dashEnergyController not found in the instantiated controllers!");
      return;
    }

    playerEntity.dashEnergyController = dashEnergyController;
    dashEnergyController.Setup(playerEntity.availableDash);

    MissileButtonManager missileButtonManager = controllers.GetComponentInChildren<MissileButtonManager>();

    if (missileButtonManager == null)
    {
      Debug.LogError("missileButtonManager not found in the instantiated controllers!");
      return;
    }

    missileButtonManager.playerEntity = playerEntity;
    missileButtonManager.shootSystem = shootSystem;

    BonusUIManager bonusUIManager = controllers.GetComponentInChildren<BonusUIManager>();

    if (bonusUIManager == null)
    {
      Debug.LogError("bonusUIManager not found in the instantiated controllers!");
      return;
    }

    bonusUIManager.SetPlayerEntity(playerEntity);
  }
}
