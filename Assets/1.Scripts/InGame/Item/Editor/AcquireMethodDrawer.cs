using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AcquireMethod))]
public class AcquireMethodDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.intValue = (int)(AcquireMethod)EditorGUI.EnumFlagsField(position, label, (AcquireMethod)property.intValue);
    }
}
