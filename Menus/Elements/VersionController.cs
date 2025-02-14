using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VersionController : MonoBehaviour
{
    private TextMeshProUGUI textMeshProUGUI;

    private void Awake()
    {
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        textMeshProUGUI.text = $"V {Application.version} Beta";
    }
}
