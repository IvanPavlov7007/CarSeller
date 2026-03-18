using Pixelplacement;
using UnityEngine;

public class ProgressionUIManager : MonoBehaviour
{
    public ProgressionUI progressionUI;

    CanvasGroup canvasGroup;


    private void Awake()
    {
        canvasGroup = progressionUI.GetComponent<CanvasGroup>();
        Debug.Assert(canvasGroup != null, "ProgressionUI should have a CanvasGroup component.");
    }

    void Start()
    {
        GameEvents.Instance.onAreaProgressed += onAreaProgressed;
        resetProgressbar();
    }

    private void resetProgressbar()
    {
        progressionUI.targetProgressLevel = G.Area.CurrentLevelNode.Value.levelNumber;
        progressionUI.targetProgressValue = G.Area.currentXP / G.Area.CurrentLevelNode.Value.xpToNextLevel;
    }

    bool _showing = false;
    float _showingTime = 0f;
    public float fadeSpeed = 2f;
    public float shownDuration = 3f;

    private void Update()
    {
        // if showing, rise group alpha to 1, then after shownDuration, turn showing to false
        // if not showing, lower group alpha to 0
        if (_showing)
        {
            canvasGroup.alpha = Mathf.Min(1f, canvasGroup.alpha + fadeSpeed * Time.deltaTime);
            _showingTime += Time.deltaTime;
            if (_showingTime >= shownDuration)
            {
                _showing = false;
            }
        }
        else
        {
            canvasGroup.alpha = Mathf.Max(0f, canvasGroup.alpha - fadeSpeed * Time.deltaTime);
        }
    }

    void onAreaProgressed(AreaProgressEventData data)
    {
        _showing = true;
        _showingTime = 0f;
        progressionUI.targetProgressLevel = data.Area.CurrentLevelNode.Value.levelNumber;
        progressionUI.targetProgressValue = data.Area.currentXP / data.Area.CurrentLevelNode.Value.xpToNextLevel;
        progressionUI.targetIsMax = data.Area.areaLevels.Count == data.NewLevel;
    }
}