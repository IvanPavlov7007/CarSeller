using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonStateController : MonoBehaviour, IPointerUpHandler
{
    Button button;
    UIElement uIElement;
    public bool Initialized{ get; private set; }
    public bool Interactable => Initialized && uIElement.IsInteractable;

    public void OnPointerUp(PointerEventData eventData)
    {
        if(!Interactable)
            GlobalHintManager.Instance.ShowHint(uIElement.UnavailabilityReason);
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(onClick);
    }

    public void Initialize(UIElement uIElement)
    {
        this.uIElement = uIElement;
        Initialized = true;
    }

    //private void Update()
    //{
    //    if(!Initialized) return;
    //    checkInteractabe();
    //}

    void onClick()
    {
        if(Interactable)
        {
            uIElement.OnClick();
            if (uIElement.closePopupOnClick)
            {
                var popup =
                GetComponentInParent<IClosable>();
                if (popup != null)
                    popup.Close();
                else
                    Debug.LogError("No popup context menu found in parents for this ui element " + uIElement);
            }
        }
    }
}

public interface IClosable
{
    public void Close();
}