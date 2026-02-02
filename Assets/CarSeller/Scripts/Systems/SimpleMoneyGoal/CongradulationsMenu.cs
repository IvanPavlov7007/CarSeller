using UnityEngine;
using Pixelplacement;
using UnityEngine.UI;

public class CongradulationsMenu : Singleton<CongradulationsMenu>
{
    public Canvas Canvas { get; private set; }
    public CanvasGroup CanvasGroup { get; private set; }
    //[SerializeField] TMPro.TextMeshProUGUI goalAmountText;
    Button closeButton;

    private void Awake()
    {
        Canvas = GetComponent<Canvas>();
        CanvasGroup = GetComponent<CanvasGroup>();
        closeButton = GetComponentInChildren<Button>();
        closeButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        Hide();
    }
    public void Show()
    {
        BlockUIManager.Instance.Block(Canvas, Hide);
        CanvasGroup.alpha = 1;
        CanvasGroup.blocksRaycasts = true;
    }
    public void Hide()
    {
        BlockUIManager.Instance.Unblock(Canvas);
        CanvasGroup.alpha = 0;
        CanvasGroup.blocksRaycasts = false;
    }
}