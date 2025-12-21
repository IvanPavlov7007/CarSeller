using Pixelplacement;
using TMPro;
using UnityEngine;

public class WorldEffectsDisplayer : Singleton<WorldEffectsDisplayer>
{
    public float MoneyEffectHeightOffset = 2f;
    public GameObject MoneyEffectPrefab;
    public void PlayMoneyEffect(float price, Vector3 position)
    {
        // Implementation for playing money effect at the given position
        if (MoneyEffectPrefab != null)
        {
            var go = GameObject.Instantiate(MoneyEffectPrefab, position, Quaternion.identity);
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

}