using System.Collections.Generic;
using UnityEngine;

public sealed class TrafficLightRuntimeController : MonoBehaviour
{
    private readonly List<string> _keys = new List<string>();
    private readonly List<CityGraphAsset.TrafficLightProgramStepData> _program = new List<CityGraphAsset.TrafficLightProgramStepData>();
    private readonly Dictionary<string, TrafficLightState> _statesByKey = new Dictionary<string, TrafficLightState>();

    private TrafficLightViewController _view;

    private int _currentStepIndex;
    private float _timeLeft;
    private float _preparationTimeSeconds;
    private bool _inPreparation;
    private bool _initialized;

    public void Initialize(
        TrafficLightViewController view,
        IEnumerable<string> allKeys,
        IList<CityGraphAsset.TrafficLightProgramStepData> program,
        float preparationTimeSeconds)
    {
        _view = view;
        _preparationTimeSeconds = Mathf.Max(0f, preparationTimeSeconds);

        _keys.Clear();
        if (allKeys != null)
        {
            foreach (var k in allKeys)
                _keys.Add(k);
        }

        _program.Clear();
        if (program != null)
        {
            for (int i = 0; i < program.Count; i++)
                _program.Add(program[i]);
        }

        _currentStepIndex = 0;
        _timeLeft = 0f;
        _inPreparation = false;
        _initialized = true;

        ApplyGoStep(_currentStepIndex);
    }

    private void Update()
    {
        if (!_initialized || _view == null)
            return;

        if (_program.Count == 0)
            return;

        _timeLeft -= Time.deltaTime;

        var currentStep = _program[_currentStepIndex];
        var effectivePrep = Mathf.Min(_preparationTimeSeconds, Mathf.Max(0f, currentStep.DurationSeconds));

        // Yellow window occurs at the end of the current step, before switching to the next step.
        if (!_inPreparation && effectivePrep > 0f && _timeLeft <= effectivePrep)
        {
            ApplyPreparationForNext();
            _inPreparation = true;
        }

        if (_timeLeft > 0f)
            return;

        _currentStepIndex = (_currentStepIndex + 1) % _program.Count;
        _inPreparation = false;
        ApplyGoStep(_currentStepIndex);
    }

    private void ApplyGoStep(int stepIndex)
    {
        if (_program.Count == 0)
            return;

        var step = _program[stepIndex];
        _timeLeft = Mathf.Max(0.01f, step.DurationSeconds);

        var goSet = new HashSet<string>(step.GoEdgeKeys ?? System.Array.Empty<string>());

        _statesByKey.Clear();
        for (int i = 0; i < _keys.Count; i++)
        {
            var k = _keys[i];
            _statesByKey[k] = goSet.Contains(k) ? TrafficLightState.Go : TrafficLightState.Stop;
        }

        _view.SetLightState(_statesByKey);
    }

    private void ApplyPreparationForNext()
    {
        if (_program.Count == 0)
            return;

        var currentStep = _program[_currentStepIndex];
        var currentGoSet = new HashSet<string>(currentStep.GoEdgeKeys ?? System.Array.Empty<string>());

        var nextStepIndex = (_currentStepIndex + 1) % _program.Count;
        var nextStep = _program[nextStepIndex];
        var nextGoSet = new HashSet<string>(nextStep.GoEdgeKeys ?? System.Array.Empty<string>());

        _statesByKey.Clear();
        for (int i = 0; i < _keys.Count; i++)
        {
            var k = _keys[i];

            if (currentGoSet.Contains(k))
            {
                _statesByKey[k] = TrafficLightState.Go;
            }
            else if (nextGoSet.Contains(k))
            {
                _statesByKey[k] = TrafficLightState.Yellow;
            }
            else
            {
                _statesByKey[k] = TrafficLightState.Stop;
            }
        }

        _view.SetLightState(_statesByKey);
    }
}