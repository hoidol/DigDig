using System;
using UnityEditor;
using UnityEngine;

// DefenceTable을 행(DefenceType) × 열(AttackType) 표로 편집
[CustomEditor(typeof(DefenceTable))]
public class DefenceTableEditor : Editor
{
    const float LABEL_WIDTH = 90f;
    const float CELL_WIDTH = 80f;

    static readonly Color STRONG_COLOR = new Color(0.55f, 0.9f, 0.55f); // 배율 > 1 (약점)
    static readonly Color WEAK_COLOR = new Color(0.95f, 0.55f, 0.55f);  // 배율 < 1 (저항)

    public override void OnInspectorGUI()
    {
        DefenceTable table = (DefenceTable)target;
        string[] attackNames = Enum.GetNames(typeof(AttackType));

        EditorGUILayout.HelpBox("피해 배율 = 공격력 × 표 값\n1보다 크면 초록(약점), 작으면 빨강(저항)", MessageType.None);

        // 헤더 (AttackType)
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("방어 \\ 공격", EditorStyles.boldLabel, GUILayout.Width(LABEL_WIDTH));
        foreach (string attackName in attackNames)
            GUILayout.Label(attackName, EditorStyles.boldLabel, GUILayout.Width(CELL_WIDTH));
        EditorGUILayout.EndHorizontal();

        if (table.rows == null)
            return;

        EditorGUI.BeginChangeCheck();
        float[][] edited = new float[table.rows.Length][];
        for (int d = 0; d < table.rows.Length; d++)
        {
            DefenceTable.Row row = table.rows[d];
            edited[d] = (float[])row.multipliers.Clone();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(row.defenceType.ToString(), GUILayout.Width(LABEL_WIDTH));
            for (int a = 0; a < edited[d].Length; a++)
            {
                Color prev = GUI.backgroundColor;
                if (edited[d][a] > 1f) GUI.backgroundColor = STRONG_COLOR;
                else if (edited[d][a] < 1f) GUI.backgroundColor = WEAK_COLOR;
                edited[d][a] = EditorGUILayout.FloatField(edited[d][a], GUILayout.Width(CELL_WIDTH));
                GUI.backgroundColor = prev;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(table, "Edit DefenceTable");
            for (int d = 0; d < table.rows.Length; d++)
                table.rows[d].multipliers = edited[d];
            EditorUtility.SetDirty(table);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("기획 기본값으로 초기화"))
        {
            Undo.RecordObject(table, "Reset DefenceTable");
            for (int d = 0; d < table.rows.Length; d++)
            {
                DefenceTable.Row row = table.rows[d];
                for (int a = 0; a < row.multipliers.Length; a++)
                    row.multipliers[a] = DefenceTable.GetDefault((AttackType)a, row.defenceType);
            }
            EditorUtility.SetDirty(table);
        }
    }
}
