using System.Globalization;
using UnityEngine;
using TMPro;

public class MoneyDisplayer : MonoBehaviour
{
    TextMeshProUGUI TextMeshProUGUI;

    private void Awake()
    {
        TextMeshProUGUI = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        GameEvents.Instance.OnPlayerMoneyChanged += onPlayerMoneyChanged;
        updateAccountValue(G.Player.Money);
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnPlayerMoneyChanged -= onPlayerMoneyChanged;
    }

    void onPlayerMoneyChanged(PlayerMoneyChangeEventData data)
    {
        updateAccountValue(data.NewMoney);
    }

    void updateAccountValue(float value)
    {
        if (TextMeshProUGUI == null) return;
        TextMeshProUGUI.text = value.ToString("C2", CultureInfo.CurrentCulture);
    }
}