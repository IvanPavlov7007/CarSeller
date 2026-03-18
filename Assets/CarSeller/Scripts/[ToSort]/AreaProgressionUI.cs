using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class AreaProgressionUI : MonoBehaviour
{
    public TMP_Text nextLevelIndexDisplayer;
    public Slider slider;
    public ParticleSystem particlesSystem;

    public float progressSpeed = 1f;

    public float targetProgressFloat;
    public int targetProgressLevel;

    float _currentProgressValue;
    int _currentProgressLevel;
    

    const float TERMINAL_PROGRESS_VALUE = 0.999f;


    [Button]
    public void ResetBar()
    {
        _currentProgressLevel = 1;
        _currentProgressValue = 0f;
    }
    
    
    void Update()
    {
        if (_currentProgressLevel < targetProgressLevel)
        {
            fillUp(float.MaxValue);
            if (_currentProgressValue > TERMINAL_PROGRESS_VALUE)
            {
                iterateLevel();
            }
        }
        else
        {
            fillUp(targetProgressFloat);
        }
        updateIndexDisplayer();
        updateProgressBar();
    }

    void iterateLevel()
    {
        _currentProgressLevel++;
        _currentProgressValue = 0f;
        particlesSystem.Play();
        updateIndexDisplayer();
    }

    void updateIndexDisplayer()
    {
        nextLevelIndexDisplayer.text = _currentProgressLevel.ToString();
    }

    void updateProgressBar()
    {
        slider.value = _currentProgressValue;
    }

    void fillUp(float maxProgress)
    {
        _currentProgressValue += progressSpeed * Time.deltaTime;
        _currentProgressValue = Mathf.Min(_currentProgressValue, maxProgress);
    }

}