using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModAge
{
    public class ModAgeUI : MonoBehaviour
    {
        // This class has a lot of bullshit about the UI I might not need. I'll keep it for now, in case I want to expand on the UI later or add configs
        public Canvas canvas;
        public RectTransform root;
        public Image BackgroundBack;
        public Button BackgroundBackButton;
        public Button BackgroundBackButtonImage;
        public Image Background;
        public RectTransform Header;
        public HorizontalLayoutGroup HeaderHLG;
        public Image HeaderImageLeft;
        public Image HeaderImageRight;
        public TextMeshProUGUI HeaderTMP;
        public RectTransform content;
        public ScrollRect contentScrollRect;
        public Image contentScrollRectImage;
        public RectTransform contentList;
        public VerticalLayoutGroup contentListVLG;
        public ContentSizeFitter contentListContentSizeFitter;

        public RectTransform ModRowPlaceholder;
        public RectTransform ModRowPlaceholderBack;
        public RectTransform ModRowPlaceholderBackBg;
        public RectTransform ModRowPlaceholderBackBgImg;
        public RectTransform ModRowPlaceholderBackBorder;
        public RectTransform ModRowPlaceholderBackBorderImg;
        public RectTransform ModRowPlaceholderContent;
        public RectTransform ModRowPlaceholderContentLeftCol;
        public HorizontalLayoutGroup ModRowPlaceholderContentLeftColHLG;
        public RectTransform ModRowPlaceholderContentLeftColIcon;
        public Image ModRowPlaceholderContentLeftColIconImage;
        public Image ModRowPlaceholderContentLeftColIconImageSelected;
        public Image ModRowPlaceholderContentLeftColIconImageBorder;
        public RectTransform ModRowPlaceholderContentLeftColNaming;
        public VerticalLayoutGroup ModRowPlaceholderContentLeftColNamingVLG;
        public RectTransform ModRowPlaceholderContentLeftColNamingMStatus;
        public TextMeshProUGUI ModRowPlaceholderContentLeftColNamingMStatusTMP;
        public RectTransform ModRowPlaceholderContentLeftColNamingName;
        public TextMeshProUGUI ModRowPlaceholderContentLeftColNamingNameTMP;
        public RectTransform ModRowPlaceholderContentLeftColNamingMVersion;
        public TextMeshProUGUI ModRowPlaceholderContentLeftColNamingMVersionTMP;
        public RectTransform ModRowPlaceholderContentRightCol;
        public Image ModRowPlaceholderContentRightColBg;
        public TMP_InputField ModRowPlaceholderContentRightColInputField;

        public Scrollbar contentScrollbar;

        public RectTransform contentScrollbarSlidingArea;
        public Image contentScrollbarImage;
        public RectTransform contentScrollbarHandle;
        public Image contentScrollbarHandleImage;
    }
}