using TMPro;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;

static class LocalizeMenuItem
{
    public const string TABLE_NAME = "Text table colleciton";

    [MenuItem("CONTEXT/TextMeshPro/Localize - Add Entry")]
    [MenuItem("CONTEXT/TextMeshProUGUI/Localize - Add Entry")]
    static void LocalizeTMProText(MenuCommand command)
    {
        var target = command.context as TextMeshProUGUI;
        SetupForLocalization(target);
    }

    public static void SetupForLocalization(TextMeshProUGUI target)
    {
        var comp = Undo.AddComponent(target.gameObject, typeof(LocalizeStringEvent)) as LocalizeStringEvent;
        var setStringMethod = target.GetType().GetProperty("text").GetSetMethod();
        var methodDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<string>), target, setStringMethod) as UnityAction<string>;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(comp.OnUpdateString, methodDelegate);

        var table = LocalizationEditorSettings.GetStringTableCollection(TABLE_NAME);
        var stringTable = table.GetTable("en") as StringTable;

        var newKey = stringTable.SharedData.AddKey();
        var entry = stringTable.AddEntry(newKey.Id, target.text);

        var strEvent = target.gameObject.GetComponent<LocalizeStringEvent>();
        strEvent.StringReference.SetReference(table.name, entry.Key);

        EditorUtility.SetDirty(stringTable);
        EditorUtility.SetDirty(stringTable.SharedData);
    }
}