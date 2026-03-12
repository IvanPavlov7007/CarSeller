using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrimarySelectionCarButtonState : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] TMP_Text CarName;
    [SerializeField] TMP_Text CarRarity;

    Button button;

    private void Awake()
    {
        
    }

    public void SetUp(PrimarySelectionCarButtonWidget widget)
    {
        button = GetComponent<Button>();
        image.sprite = widget.sprite;
        CarName.text = widget.Car.Name;
        CarRarity.text = widget.rarity;

        if(widget.OnClick != null)
            button.onClick.AddListener(widget.OnClick.Invoke);
    }
}