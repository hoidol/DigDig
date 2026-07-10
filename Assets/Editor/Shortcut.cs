using UnityEditor;
using System;

public static class AddressablesShortcut
{
    [MenuItem("Window/Asset Management/Addressables/Groups %g")]
    static void OpenAddressableGroups()
    {
        var windowType = Type.GetType("UnityEditor.AddressableAssets.GUI.AddressableAssetsWindow,Unity.Addressables.Editor");
        if (windowType != null)
            EditorWindow.GetWindow(windowType).Show();
    }
}
