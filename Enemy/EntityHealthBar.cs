using UnityEngine;
using UnityEngine.UI;

public class EntityHealthBar : MonoBehaviour
{
    public static Color enemyBarColor = new(1, 0, 0);
    public static Color allyBarColor = new(3 / 255f, 207 / 255f, 16 / 255f);

    [Tooltip("Image to show the Health Bar, should be set as Fill, the script modifies fillAmount")]
    public Image image;

    /// <summary>
    /// Update Health Bar using the Image fillAmount based on the current Health Amount
    /// </summary>
    public void UpdateHealthBar(float health)
    {
        image.fillAmount = health;
    }
}