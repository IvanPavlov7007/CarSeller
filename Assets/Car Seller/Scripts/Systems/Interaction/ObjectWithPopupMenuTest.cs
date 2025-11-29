using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ObjectWithPopupMenuTest : MonoBehaviour
{
    [System.Serializable]
    public enum TestButton { Crouch, Jump}
    public TestButton buttonToTest;

    public SimpleContentProvider contentProvider { get; private set; }

    private void Awake()
    {
     contentProvider = new SimpleContentProvider(this.gameObject);
    }


    void OnTestButton()
    {
        GameEvents.Instance.OnObjectWithPopupClicked?.Invoke(this);
    }

    private void OnEnable()
    {
        switch (buttonToTest)
        {
            case TestButton.Crouch:
                PlayerInputController.Instance.crouched += OnTestButton;
                break;
            case TestButton.Jump:
                PlayerInputController.Instance.jumped += OnTestButton;
                break;
            default:
                break;
        }
    }


    private void OnDisable()
    {
        switch (buttonToTest)
        {
            case TestButton.Crouch:
                PlayerInputController.Instance.crouched -= OnTestButton;
                break;
            case TestButton.Jump:
                PlayerInputController.Instance.jumped -= OnTestButton;
                break;
            default:
                break;
        }
    }

    public class SimpleContentProvider : UIContentProvider
    {
        GameObject owner;

        public SimpleContentProvider(GameObject owner)
        {
            this.owner = owner;
        }

        public IEnumerable<UISingleContent> GetContents(UIContext context)
        {
            List<UISingleContent> contents = new List<UISingleContent>()
            {
                new UISingleContent{ ContentType = UIContentType.Header, Header = owner.name}
            };
            return contents;
        }
    }
}

