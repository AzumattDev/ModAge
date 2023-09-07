using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModAge
{
    public class ModAgeUI : MonoBehaviour
    {
        public Canvas canvas = null!;
        public RectTransform root = null!;
        public Image BackgroundBack = null!;
        public Button BackgroundBackButton = null!;
        public Button BackgroundBackButtonImage = null!;
        public Image Background = null!;
        public RectTransform Header = null!;
        public HorizontalLayoutGroup HeaderHLG = null!;
        public Image HeaderImageLeft = null!;
        public Image HeaderImageRight = null!;
        public TextMeshProUGUI HeaderTMP = null!;
        public RectTransform content = null!;
        public ScrollRect contentScrollRect = null!;
        public Image contentScrollRectImage = null!;
        public RectTransform contentList = null!;
        public VerticalLayoutGroup contentListVLG = null!;
        public ContentSizeFitter contentListContentSizeFitter = null!;

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

        [Header("Scrollarea")]
        public Scrollbar contentScrollbar = null!;

        public RectTransform contentScrollbarSlidingArea = null!;
        public Image contentScrollbarImage = null!;
        public RectTransform contentScrollbarHandle = null!;
        public Image contentScrollbarHandleImage = null!;

        public void Awake()
        {
        }

        public void Start()
        {
        }

        public void Update()
        {
        }

        public void OnMoreInfoClicked()
        {
            // Not implemented in initial release version. Not sure how I want this to work yet, so I took it out.
        }
    }
}