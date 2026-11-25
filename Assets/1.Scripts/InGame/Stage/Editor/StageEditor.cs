using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Stage 인스펙터: Tile Index 자동 설정(위치 기준, 임시)·중복 검사·타일별 index 직접 수정 + 씬 뷰에 index 표시
[CustomEditor(typeof(Stage))]
public class StageEditor : Editor
{
    bool showTiles = true;
    List<string> duplicates;

    public override void OnInspectorGUI()
    {
        var stage = (Stage)target;
        DrawDefaultInspector();
        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("타일 모으기"))
            {
                Undo.RecordObject(stage, "Collect Tiles");
                stage.CollectTiles();
                EditorUtility.SetDirty(stage);
                duplicates = stage.FindDuplicateIndices();
            }
            if (GUILayout.Button("Tile Index 자동 설정 (위치 기준)"))
            {
                Undo.RecordObject(stage, "Collect Tiles");
                if (stage.tiles == null || stage.tiles.Length == 0)
                    stage.CollectTiles();
                Undo.RecordObjects(stage.tiles, "Assign Tile Index");
                stage.AssignTileIndicesByPosition();
                foreach (Tile tile in stage.tiles)
                    if (tile != null)
                        EditorUtility.SetDirty(tile);
                EditorUtility.SetDirty(stage);
                duplicates = stage.FindDuplicateIndices();
            }
            if (GUILayout.Button("중복 검사"))
                duplicates = stage.FindDuplicateIndices();
        }

        if (duplicates != null)
        {
            if (duplicates.Count == 0)
                EditorGUILayout.HelpBox("겹치는 index 없음", MessageType.Info);
            else
                EditorGUILayout.HelpBox("겹치는 index:\n" + string.Join("\n", duplicates), MessageType.Warning);
        }

        if (stage.tiles == null || stage.tiles.Length == 0)
            return;
        showTiles = EditorGUILayout.Foldout(showTiles, $"Tile Index ({stage.tiles.Length})", true);
        if (!showTiles)
            return;
        foreach (Tile tile in stage.tiles)
        {
            if (tile == null)
                continue;
            using (new EditorGUILayout.HorizontalScope())
            {
                string label = tile.transform.parent != null ? $"{tile.transform.parent.name}/{tile.transform.GetSiblingIndex()}" : tile.name;
                if (GUILayout.Button(label, EditorStyles.miniButton, GUILayout.Width(110)))
                    EditorGUIUtility.PingObject(tile);
                EditorGUI.BeginChangeCheck();
                Vector2Int index = EditorGUILayout.Vector2IntField(GUIContent.none, tile.index);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(tile, "Edit Tile Index");
                    tile.index = index;
                    EditorUtility.SetDirty(tile);
                    duplicates = stage.FindDuplicateIndices();
                }
            }
        }
    }

    // 씬 뷰: 이 Stage를 선택하면 타일마다 index 글씨
    void OnSceneGUI()
    {
        var stage = (Stage)target;
        if (stage.tiles == null)
            return;
        var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.yellow } };
        foreach (Tile tile in stage.tiles)
            if (tile != null)
                Handles.Label(tile.transform.position, $"{tile.index.x},{tile.index.y}", style);
    }
}
