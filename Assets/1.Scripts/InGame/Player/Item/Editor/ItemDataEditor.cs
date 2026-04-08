using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ItemData itemData = (ItemData)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Edit"))
        {
            itemData.Edit();
        }
    }
}
