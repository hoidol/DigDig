using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// (2026-11-26) Lobby 씬 BattleCanvas 아래에 클리어 팁 띠지(ClearTipBand + LineA/LineB)를 실제로 만들어 ClearTipTicker에 연결
// 실행 중엔 생성하지 않음 → 이 메뉴로 씬에 만들고 위치·모양은 씬에서 직접 수정. 이미 있으면 지우고 새로 만듦. 씬 저장은 직접
public static class ClearTipBandBuilder
{
    [MenuItem("Tools/Lobby/Add ClearTip Band To Lobby Scene")]
    public static void Build()
    {
        var battle = Object.FindFirstObjectByType<BattleCanvas>(FindObjectsInactive.Include);
        if (battle == null)
        {
            Debug.LogWarning("[ClearTipBandBuilder] BattleCanvas가 열린 씬에 없음 (Lobby 씬을 열고 실행)");
            return;
        }
        Undo.RecordObject(battle, "Add ClearTip Band");
        ClearTipTicker ticker = battle.GetComponent<ClearTipTicker>();
        if (ticker == null)
            ticker = Undo.AddComponent<ClearTipTicker>(battle.gameObject);
        Undo.RecordObject(ticker, "Add ClearTip Band");
        if (ticker.band != null)
            Undo.DestroyObjectImmediate(ticker.band.gameObject);
        ticker.band = null;
        ticker.lineA = ticker.lineB = null;
        ticker.BuildUI();
        Undo.RegisterCreatedObjectUndo(ticker.band.gameObject, "Add ClearTip Band");
        ticker.lineA.text = "닉네임 : 클리어 팁 미리보기";
        battle.clearTipTicker = ticker;
        EditorUtility.SetDirty(ticker);
        EditorUtility.SetDirty(battle);
        EditorSceneManager.MarkSceneDirty(battle.gameObject.scene);
        Selection.activeGameObject = ticker.band.gameObject;
        Debug.Log("[ClearTipBandBuilder] 생성: BattleCanvas/ClearTipBand (씬 저장은 직접)");
    }
}
