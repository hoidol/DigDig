using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// (2026-11-26) InGame 씬 ResultCanvas에 클리어 팁 입력(ClearTipInput: 입력칸 + 등록 버튼)을 실제로 만들어 연결 (실행 후 씬 저장)
// 이미 있으면 지우고 새로. 처음엔 꺼 둠 (클리어 결과 화면에서 켜짐). 이후 모양·위치는 씬에서 직접 수정
public static class ClearTipInputBuilder
{
    [MenuItem("Tools/InGame/Add ClearTip Input To ResultCanvas")]
    public static void Build()
    {
        var result = Object.FindFirstObjectByType<ResultCanvas>(FindObjectsInactive.Include);
        if (result == null)
        {
            Debug.LogWarning("[ClearTipInputBuilder] ResultCanvas가 열린 씬에 없음 (InGame 씬을 열고 실행)");
            return;
        }
        Undo.RecordObject(result, "Add ClearTip Input");
        ClearTipInputPanel panel = result.GetComponent<ClearTipInputPanel>();
        if (panel == null)
            panel = Undo.AddComponent<ClearTipInputPanel>(result.gameObject);
        Undo.RecordObject(panel, "Add ClearTip Input");
        if (panel.rootObject != null)
            Undo.DestroyObjectImmediate(panel.rootObject);
        panel.rootObject = null;
        panel.inputField = null;
        panel.submitButton = null;
        panel.submitText = null;
        panel.BuildUI();
        Undo.RegisterCreatedObjectUndo(panel.rootObject, "Add ClearTip Input");
        if (panel.submitText != null)
            panel.submitText.text = "등록";
        panel.rootObject.SetActive(false);
        result.clearTipInput = panel;
        EditorUtility.SetDirty(panel);
        EditorUtility.SetDirty(result);
        EditorSceneManager.MarkSceneDirty(result.gameObject.scene);
        Selection.activeGameObject = panel.rootObject;
        Debug.Log("[ClearTipInputBuilder] 생성: ResultCanvas/ClearTipInput (씬 저장은 직접)");
    }
}
