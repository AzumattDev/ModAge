using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModAge;

public class ModAgeUIPlaceholder : MonoBehaviour
{
    [Header("Placeholder Object")]
    public ModAgeUIPlaceholder PlaceholderInstance = null!;
    public RectTransform Placeholder = null!;
    public RectTransform PlaceholderBack = null!;
    public RectTransform PlaceholderBackBg = null!;
    public RectTransform PlaceholderBackBgImg = null!;
    public RectTransform PlaceholderBackBorder = null!;
    public RectTransform ModRowPlaceholderBackBorderImg = null!;
    public RectTransform PlaceholderContent = null!;
    public RectTransform PlaceholderContentTopRow = null!;
    public Image PlaceholderModImage = null!;
    public TextMeshProUGUI PlaceholderModName = null!;
    public TextMeshProUGUI PlaceholderLastUpdated = null!;
    public TextMeshProUGUI PlaceholderGameUpdatedBool = null!;
    public TextMeshProUGUI PlaceholderVersionInstalled = null!;
    public TextMeshProUGUI PlaceholderVersionAvailable = null!;
    public Button PlaceholderMoreInfoButton = null!;

    private void Awake()
    {
        PlaceholderInstance = this;
    }
}