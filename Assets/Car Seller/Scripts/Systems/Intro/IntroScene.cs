using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Linq;
using UnityEngine;

public class IntroScene : SerializedMonoBehaviour
{
    public RectTransform introPanel;
    [OdinSerialize]
    IUIElementBuilder<RectTransform> uIElementBuilder;
    
    private void Awake()
    {
        var content = new UIElement()
        {
            Type = UIElementType.Button,
            OnClick = () =>
            {
                G.Instance.GameFlowController.EnterWarehouse((Warehouse)
                    World.Instance.City.Positions.Keys.First(x => x.GetType() == typeof(Warehouse)));
            },
            Text = "Start game"
        };
        uIElementBuilder.Build(content,introPanel);
    }
}