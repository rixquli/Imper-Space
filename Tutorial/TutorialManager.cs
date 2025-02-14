using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialLobbyState
    {
        // Lobby
        Welcome,
        Equipements,
        ShowEquipButton,
        EquipShield,
        Ships,
        SeeShips,
        Upgrade,
        SeeUpgrade,
        TryUpgrade,
        BackToMainMenu,
        ClickOnPlayButton,
        SeeGameMode,
        ClickOnCampaignMode,
        SeeLevelStats,
        ClickOnStartButton,

        // In Game
        FirstApparitionInGame,
        DescriptionOfPoint,
        TryToPutYourPoints,
        WelcomeIn,
        TravelingToEnemyBase,
        TravelingToPlayerBase,
        MoveWithJoystick,
        ShootWithJoystick,
        ShieldWithJoystick,
        MissileWithButton,
        TryKillEnemy,
        SeeTurret,
        TryCaptureTheTurret,
        YourGoal,
        PrepareYourDefense,
        AfterTheWaveShieldBreak,

        // Lobby For Space Base
        YouCanSeeYourSpaceBase,
        ClickOnSpaceBase,

        // In Game For Space Base
        FirstApparitionInSpaceBase,
        YouCanUpgradeIt,
        TryPlaceOneTurret,
        TryPlaceOneOfEachBuilding,
        AttackNeedTroops,
        TryRecruitTroops,
        AttackExplanation,
        DefenseExplanation,
        PlacingTroopExplanation,
        TryAttack,

        // In Game For Extermination
        FirstApparitionInExtermination,
        DescriptionOfPointInExtermination,
        TryToPutYourPointsInExtermination,
        GoalInExtermination,
        MoveWithJoystickInExtermination,
        ShootWithJoystickInExtermination,
        ShieldWithJoystickInExtermination,
        MissileWithButtonInExtermination,
        TryKillEnemyInExtermination,
        GoodLuckInExtermination,
    }

    public static TutorialManager Instance;

    public Camera mainCamera; // La caméra utilisée pour le raycasting
    public FollowPlayer cameraLogic;
    public GraphicRaycaster raycasterMain; // Raycaster pour la scène principale
    public GraphicRaycaster raycasterTutorial; // Raycaster pour la scène tutoriel

    public InputAction clickAction;

    public CanvasGroup tutorialBackground;

    public UDictionary<TutorialLobbyState, TutoPanelCard> tutorialPanels = new();

    private int tutoIndex = 0;

    public bool blockMaineScene = true;

    private bool hadFirstKill = false;
    private bool hadFirstTurretCaptured = false;

    private bool hadPlaceAllFreeBuilding = false;
    private bool hadRecruitTroop = false;
    [SerializeField] private bool startTutoOnStart = true;

    [Header("Elements For In Game Tutorial")]
    [SerializeField]
    private Vector2 enemyBasePosition;

    [SerializeField]
    private Vector2 topExterminationPosition;

    [SerializeField]
    private Vector2 playerBasePosition = new(0, 0);

    [SerializeField]
    private Vector2 firstEnemyPosition = new(0, 0);

    [SerializeField]
    private Vector2 firstTurretPosition = new(0, 0);

    private List<TutorialLobbyState> requireMinButton =
        new()
        {
            TutorialLobbyState.Equipements,
            TutorialLobbyState.ShowEquipButton,
            TutorialLobbyState.EquipShield,
            TutorialLobbyState.Ships,
            TutorialLobbyState.Upgrade,
            TutorialLobbyState.TryUpgrade,
            TutorialLobbyState.BackToMainMenu,
            TutorialLobbyState.ClickOnCampaignMode,
            TutorialLobbyState.ClickOnPlayButton,
            TutorialLobbyState.ClickOnStartButton,
            TutorialLobbyState.TryPlaceOneTurret,
            TutorialLobbyState.TryPlaceOneOfEachBuilding,
            TutorialLobbyState.TryAttack,
        };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (startTutoOnStart)
            TutoSwitch(0);
        else
            tutoIndex = -1;
    }

    private void OnEnable()
    {
        // Enable the input action
        clickAction.Enable();
        clickAction.performed += OnClick;
    }

    private void OnDisable()
    {
        // Disable the input action
        clickAction.performed -= OnClick;
        clickAction.Disable();
    }

    private void TutoSwitch(int index)
    {
        if (index > 0)
            tutorialPanels.Values[index - 1].Deactivate();
        Debug.LogError(
            $"index: {index},tutorialPanels.Values.Count: {tutorialPanels.Values.Count - 1} "
        );
        if (index > tutorialPanels.Values.Count - 1 || index < 0)
            return;
        tutorialPanels.Values[index].Activate();
    }

    public void ShowTutorialBackground()
    {
        blockMaineScene = true;
        tutorialBackground.alpha = 1f;
    }

    public void HideTutorialBackground()
    {
        blockMaineScene = false;
        tutorialBackground.alpha = 0f;
        if (SpecialAction(tutorialPanels.Keys[tutoIndex]))
        {
            return;
        }
        tutoIndex++;
        TutoSwitch(tutoIndex);
    }

    private void ForceSwitchToNextTutorialWithDelay(float delay)
    {
        StopAllCoroutines();
        StartCoroutine(ForceSwitchToNextTutorialCoroutine(delay));
    }

    private IEnumerator ForceSwitchToNextTutorialCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        tutoIndex++;
        TutoSwitch(tutoIndex);
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        if (mainCamera == null || raycasterMain == null || raycasterTutorial == null)
            return;

        Vector2 touchPosition;

        // Check if a touchscreen is available
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null) // Fallback to mouse if no touch input
        {
            touchPosition = Mouse.current.position.ReadValue();
        }
        else
        {
            return; // No valid input detected
        }

        Button mainButton = RaycastUI(touchPosition, raycasterMain);
        Button tutoButton = RaycastUI(touchPosition, raycasterTutorial);

        Debug.Log(
            $"tutoIndex : {tutoIndex}, requireMinButton: {requireMinButton.Exists(l => (int)l == tutoIndex)}"
        );
        Debug.Log($"mainButton : {mainButton != null}, tutoButton: {tutoButton != null}");

        if (requireMinButton.Exists(l => (int)l == tutoIndex))
        {
            if (mainButton != null && tutoButton != null)
            {
                Debug.Log($"mainButton.name: {mainButton.name}");
                Debug.Log($"tutoButton.name: {tutoButton.name}");
                mainButton.onClick?.Invoke();
                tutoButton.onClick?.Invoke();
            }
        }
        else
        {
            if (tutoButton != null)
            {
                Debug.Log($"tutoButton.name: {tutoButton.name}");
                tutoButton.onClick?.Invoke();
            }
        }
    }

    private Button RaycastUI(Vector2 screenPos, GraphicRaycaster raycaster)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = screenPos,
        };

        // Crée une liste pour les résultats du raycast
        var results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        // Vérifie si un bouton a été touché
        foreach (var result in results)
        {
            Button button = result.gameObject.GetComponent<Button>();
            if (button != null && button.IsActive())
            {
                // Simule le clic sur le bouton trouvé
                // button.onClick.Invoke();
                return button;
            }
        }

        return null;
    }

    // Custom function for tutorial in game

    private bool SpecialAction(TutorialLobbyState tutorialLobbyState)
    {
        Debug.Log($"SpecialAction: {tutorialLobbyState}");
        // return false if the tuto must continue normally and true if the next tuto card should not appear normaly but manualy
        switch (tutorialLobbyState)
        {
            // case TutorialLobbyState.TryToPutYourPoints:
            //     return true;
            case TutorialLobbyState.WelcomeIn:
                ForceSwitchToNextTutorialWithDelay(0.5f);
                Invoke(nameof(Wait05SBeforeTravelingToTurret), 0.5f);
                return true;
            // case TutorialLobbyState.TravelingToEnemyBase - 1:
            //     cameraLogic.Traveling(enemyBasePosition, 1.5f);
            //     return false;
            // case TutorialLobbyState.TravelingToPlayerBase - 1:
            //     cameraLogic.Traveling(playerBasePosition, 0.5f);
            //     return false;
            // case TutorialLobbyState.TravelingToPlayerBase:
            //     cameraLogic.StopTraveling();
            //     return false;
            // case TutorialLobbyState.MoveWithJoystick:
            //     ForceSwitchToNextTutorialWithDelay(2f);
            //     return true;
            // case TutorialLobbyState.ShootWithJoystick:
            //     ForceSwitchToNextTutorialWithDelay(2f);
            //     return true;
            // case TutorialLobbyState.ShieldWithJoystick:
            //     ForceSwitchToNextTutorialWithDelay(2f);
            //     return true;
            // case TutorialLobbyState.MissileWithButton: // after missile wait for the next
            //     return true;
            case TutorialLobbyState.TryCaptureTheTurret - 1: // after missile wait for the next
                cameraLogic.StopTraveling();
                return false;
            case TutorialLobbyState.PrepareYourDefense:
                cameraLogic.Traveling(enemyBasePosition, 1.5f);
                return false;
            case TutorialLobbyState.AfterTheWaveShieldBreak:
                cameraLogic.StopTraveling();
                return false;
            // Space Base
            case TutorialLobbyState.TryPlaceOneTurret:
                return true;
            case TutorialLobbyState.TryPlaceOneOfEachBuilding:
                return true;
            case TutorialLobbyState.TryRecruitTroops:
                return true;
            // Extermination
            case TutorialLobbyState.TryToPutYourPointsInExtermination:
                return true;
            case TutorialLobbyState.MoveWithJoystickInExtermination - 1:
                return false;
            case TutorialLobbyState.MoveWithJoystickInExtermination:
                ForceSwitchToNextTutorialWithDelay(2f);
                return true;
            case TutorialLobbyState.ShootWithJoystickInExtermination:
                ForceSwitchToNextTutorialWithDelay(2f);
                return true;
            case TutorialLobbyState.ShieldWithJoystickInExtermination:
                ForceSwitchToNextTutorialWithDelay(2f);
                return true;
            case TutorialLobbyState.MissileWithButtonInExtermination: // after missile wait for the next
                ForceSwitchToNextTutorialWithDelay(0.5f);
                // Invoke(nameof(Wait05SBeforeTravelingToEnemy), 0.5f);
                return true;
            case TutorialLobbyState.GoodLuckInExtermination:
                return true;
            default:
                return false;
        }
    }

    public void HadCloseStatPanelOnce()
    {
        tutoIndex++;
        TutoSwitch(tutoIndex);
    }

    public void FirstKill()
    {
        if (!hadFirstKill)
        {
            hadFirstKill = true;
            // cameraLogic.Traveling(firstTurretPosition, 0.5f);
            ForceNetxCard();
        }
    }

    public void TurretCaptured()
    {
        if (!hadFirstTurretCaptured)
        {
            hadFirstTurretCaptured = true;
            StaticDataManager.StartGameplay();
            Invoke(nameof(ForceNetxCard), 10f);
        }
    }

    private bool CheckIfAllFreeBuildingArePlaced()
    {
        foreach (var buildingElement in VillageRessourcesManager.Instance.buildingList)
        {
            foreach (var building in buildingElement.buildings)
            {
                if (building.price == 0 && !building.isPurchase)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void BuildingPlaced(int id)
    {
        if (!hadPlaceAllFreeBuilding && CheckIfAllFreeBuildingArePlaced())
        {
            hadPlaceAllFreeBuilding = true;
            ForceNetxCard();
        }
    }

    public void TroopPanelClosed()
    {
        if (hadRecruitTroop)
            return;
        foreach (TroopsToPlace item in VillageRessourcesManager.Instance.troopsToPlaces)
        {
            if (item.amount > 0)
            {
                ForceNetxCard();
                hadRecruitTroop = true;
                return;
            }
        }
    }

    public void ForceNetxCard(int addAmount = 1)
    {
        tutoIndex += addAmount;
        TutoSwitch(tutoIndex);
    }

    private void Wait05SBeforeTravelingToTurret()
    {
        cameraLogic.Traveling(firstTurretPosition, 0.5f);
    }

    private void Wait05SBeforeTravelingToEnemy()
    {
        cameraLogic.Traveling(firstEnemyPosition, 0.5f);
    }
}
