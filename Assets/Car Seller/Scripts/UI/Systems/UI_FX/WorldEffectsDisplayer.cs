using Pixelplacement;
using TMPro;
using UnityEngine;

public class WorldEffectsDisplayer : Singleton<WorldEffectsDisplayer>
{
    public float MoneyEffectHeightOffset = 2f;
    public GameObject MoneyWorldEffectPrefab;
    public GameObject MoneyUIEffectPrefab;

    Canvas Canvas;
    Camera Camera => Camera.main;

    private void Awake()
    {
        Canvas = GetComponent<Canvas>();
    }

    public void PlayMoneyEffectWorld(float price, Vector3 position)
    {
        // Implementation for playing money effect at the given position
        if (MoneyWorldEffectPrefab != null)
        {
            var go = GameObject.Instantiate(MoneyWorldEffectPrefab, position, Quaternion.identity);
            var textMesh = go.GetComponentInChildren<TextMeshProUGUI>();
            if (textMesh != null)
            {
                textMesh.text = $"+${price:N0}";
                textMesh.color = Color.yellow;
            }
            Tween.Position(go.transform, position + Vector3.up * MoneyEffectHeightOffset, 3f, 0f, Tween.EaseInOut);
            Tween.Color(textMesh, Color.clear, 3f, 0f, Tween.EaseInOut);
            Object.Destroy(go, 3f); // Destroy after 3 seconds
        }
    }

    public void PlayMonetEffectScreen(float price, Vector3 position)
    {
        // Implementation for playing money effect on the UI canvas
        if (MoneyUIEffectPrefab != null)
        {
            var screenPosition = Camera.WorldToScreenPoint(position);
            var uiPosition = Canvas.transform.InverseTransformPoint(screenPosition);
            var go = GameObject.Instantiate(MoneyUIEffectPrefab, Canvas.transform);
            go.GetComponent<RectTransform>().anchoredPosition = uiPosition;
            var textMesh = go.GetComponentInChildren<TextMeshProUGUI>();
            if (textMesh != null)
            {
                textMesh.text = $"+${price:N0}";
                textMesh.color = Color.yellow;
            }
            Tween.Position(go.transform, uiPosition + Vector3.up * 100f, 2f, 0f, Tween.EaseInOut);
            Tween.Color(textMesh, Color.clear, 2f, 0f, Tween.EaseInOut);
            Object.Destroy(go, 3f); // Destroy after 2 seconds
        }
    }

}