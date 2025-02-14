using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FilterController : MonoBehaviour
{
    [SerializeField] private Button sortButton;
    [SerializeField] private TextMeshProUGUI sortText;

    [SerializeField] private Button sortModeButton;
    [SerializeField] private UIEffectsManager sortModeEffect;

    [SerializeField] private GameObject sortMenu;
    [SerializeField] private UIEffectsManager sortMenuEffect;
    public enum SortType
    {
        Rarity,
        Type,
        Level
    }
    [SerializeField] private UDictionary<SortType, Button> sortButtons = new();

    private bool isSortMenuOpen = false;
    public enum SortMode
    {
        Ascending,
        Descending,
    }

    private SortType sortType = SortType.Rarity;
    private SortMode sortMode = SortMode.Ascending;

    public event Action<SortType, SortMode> OnSortChanged;



    private void Awake()
    {
        sortButton.onClick.AddListener(() =>
        {
            if (isSortMenuOpen)
            {
                sortMenuEffect.Run("Hide");
                isSortMenuOpen = false;
            }
            else
            {
                sortMenuEffect.Run("Show");
                isSortMenuOpen = true;
            }
        });

        foreach (var button in sortButtons)
        {
            button.Value.onClick.AddListener(() =>
            {
                switch (button.Key)
                {
                    case SortType.Rarity:
                        sortText.text = "Rarity";
                        break;
                    case SortType.Type:
                        sortText.text = "Type";
                        break;
                    case SortType.Level:
                        sortText.text = "Level";
                        break;
                }
                sortType = button.Key;
                OnSortChanged?.Invoke(sortType, sortMode);
            });
        }

        sortModeButton.onClick.AddListener(() =>
        {
            sortMode = sortMode == SortMode.Ascending ? SortMode.Descending : SortMode.Ascending;
            if (sortMode == SortMode.Ascending)
            {
                sortModeEffect.Run("ASC");
            }
            else
            {
                sortModeEffect.Run("DESC");
            }
            OnSortChanged?.Invoke(sortType, sortMode);
        });
    }
}
