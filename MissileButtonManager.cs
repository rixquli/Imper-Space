using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MissileButtonManager : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI cooldawnText;
    public Button button;
    public PlayerEntity playerEntity;
    public ShootSystem shootSystem;
    private bool isImageAndTextAbles = true;
    private float Cooldown
    {
        get
        {
            if (playerEntity.bonusActiveTimeRemain[BonusBehavior.BonusType.MissileCooldown] > 0)
            {
                return playerEntity.missilleCooldown / 2;
            }
            else
            {
                return playerEntity.missilleCooldown;
            }
        }
    }


    private void Awake()
    {
        SetAbleImageAndText();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    private void Update()
    {
        if (playerEntity.missilleCurrentCooldawn > 0 && isImageAndTextAbles)
        {
            isImageAndTextAbles = false;
            SetUnableImageAndText();
        }
        else if (playerEntity.missilleCurrentCooldawn <= 0 && !isImageAndTextAbles)
        {
            isImageAndTextAbles = true;
            SetAbleImageAndText();
        }
    }

    private void OnButtonClick()
    {
        if (Shoot())
        {
            shootSystem.ShootMissile();
        }
    }

    public bool Shoot()
    {
        if (playerEntity.missilleCurrentCooldawn <= 0)
        {
            float initialCooldown = Cooldown;
            playerEntity.missilleCurrentCooldawn = initialCooldown;
            playerEntity.missilleCurrentCooldawn = Cooldown;
            CoroutineRunner.Instance.StartCoroutine(StartCooldawn(initialCooldown));
            return true;
        }
        else
        {
            return false;
        }
    }

    IEnumerator StartCooldawn(float initialCooldown)
    {
        float elapsedTime = 0f;
        float remainingTime = initialCooldown;

        while (elapsedTime < initialCooldown)
        {
            if (playerEntity.missilleCurrentCooldawn <= 0)
            {
                break;
            }
            float deltaTime = Time.deltaTime;
            elapsedTime += deltaTime;
            remainingTime -= deltaTime;

            playerEntity.missilleCurrentCooldawn = Mathf.Max(remainingTime, 0);

            float newCooldawnForText = remainingTime > 1 ?
            Mathf.Round(remainingTime) :
            Mathf.Round(remainingTime * 10) / 10;
            cooldawnText.text = newCooldawnForText.ToString();

            yield return null; // Wait for the next frame
        }
    }

    private void SetUnableImageAndText()
    {
        if (image == null || cooldawnText == null)
        {
            return;
        }
        image.color = new Color(0, 0, 0, 0.5f);
        cooldawnText.color = new Color(cooldawnText.color.r, cooldawnText.color.g, cooldawnText.color.b, 1f);
    }

    private void SetAbleImageAndText()
    {
        if (image == null || cooldawnText == null)
        {
            return;
        }
        image.color = new Color(1, 1, 1, 1);
        cooldawnText.color = new Color(cooldawnText.color.r, cooldawnText.color.g, cooldawnText.color.b, 0f);
    }
}