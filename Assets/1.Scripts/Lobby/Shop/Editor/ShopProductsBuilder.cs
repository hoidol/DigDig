using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;

// (2026-11-27) 상점 패키지 탭(Package_Scroll) 판매 상품 = 4개: 기본 게임 팩 · 용사 패스 · 광고 제거 · 게임 속도 (ProductData.csv)
// - 기존 "Ad Remove Package" 칸을 복사해 BaseGamePack·HeroPass 칸을 만들고, 기존 두 칸은 광고 제거(RemoveAd)·게임 속도(GameSpeedUp) 단품으로 key를 바꿈
// - 버튼 key·제목 번역 key(Product.{key})·onPurchased → 칸 Purchased 연결. 예전 상점 내용(꺼 둔 Bg/Contents: 옛 단품·미니언 뽑기)은 삭제
// - 다시 실행하면 BaseGamePack·HeroPass 칸을 지우고 새로. 이후 모양은 씬에서 직접 수정
public static class ShopProductsBuilder
{
    [MenuItem("Tools/Lobby/Setup Shop Products (4)")]
    public static void Build()
    {
        ShopCanvas shop = Object.FindFirstObjectByType<ShopCanvas>(FindObjectsInactive.Include);
        if (shop == null)
        {
            Debug.LogWarning("[ShopProductsBuilder] ShopCanvas가 열린 씬에 없음 (Lobby 씬을 열고 실행)");
            return;
        }
        Transform contents = Find(shop.transform, "Package_Scroll")?.Find("Contents");
        if (contents == null)
        {
            Debug.LogWarning("[ShopProductsBuilder] Package_Scroll/Contents 없음");
            return;
        }
        Transform removeAd = contents.Cast<Transform>().FirstOrDefault(t => t.GetComponent<RemoveAdPackagePanel>() != null);
        Transform speed = contents.Cast<Transform>().FirstOrDefault(t => t.GetComponent<GameSpeedUpPackagePanel>() != null);
        if (removeAd == null || speed == null)
        {
            Debug.LogWarning("[ShopProductsBuilder] 광고 제거·게임 속도 칸(RemoveAdPackagePanel·GameSpeedUpPackagePanel)이 없음");
            return;
        }
        foreach (Transform old in contents.Cast<Transform>().Where(t => t.GetComponent<BaseGamePackPanel>() != null || t.GetComponent<HeroPassPanel>() != null).ToArray())
            Undo.DestroyObjectImmediate(old.gameObject);

        int first = Mathf.Min(removeAd.GetSiblingIndex(), speed.GetSiblingIndex());
        GameObject pack = Duplicate<BaseGamePackPanel>(removeAd.gameObject, "BaseGamePack");
        GameObject pass = Duplicate<HeroPassPanel>(removeAd.gameObject, "HeroPass");
        pack.transform.SetSiblingIndex(first);
        pass.transform.SetSiblingIndex(first + 1);

        Setup(pack, InAppProductInfo.BASE_GAME_PACK_KEY);
        Setup(pass, InAppProductInfo.HERO_PASS_KEY);
        Undo.RecordObject(removeAd.gameObject, "Shop products");
        removeAd.name = "RemoveAd";
        Setup(removeAd.gameObject, InAppProductInfo.REMOVE_AD_KEY);
        Undo.RecordObject(speed.gameObject, "Shop products");
        speed.name = "GameSpeedUp";
        Setup(speed.gameObject, InAppProductInfo.GAME_SPEED_UP_KEY);

        // 예전 상점 내용 (꺼 둔 Bg/Contents: 옛 단품·미니언 뽑기, 스크립트 빠진 칸 포함)
        Transform oldContents = shop.transform.Find("Panel/Bg/Contents");
        if (oldContents != null)
            Undo.DestroyObjectImmediate(oldContents.gameObject);

        EditorSceneManager.MarkSceneDirty(shop.gameObject.scene);
        Debug.Log("[ShopProductsBuilder] 상점 판매 상품 4개 구성 (씬 저장은 직접)");
    }

    static GameObject Duplicate<T>(GameObject src, string name) where T : PackageProductPanel
    {
        GameObject go = Object.Instantiate(src, src.transform.parent);
        Undo.RegisterCreatedObjectUndo(go, "Shop products");
        go.name = name;
        go.SetActive(true);
        foreach (PackageProductPanel p in go.GetComponents<PackageProductPanel>())
            Object.DestroyImmediate(p);
        go.AddComponent<T>();
        return go;
    }

    // 버튼 key · 제목 번역 key · 구매 완료 → 칸 Purchased
    static void Setup(GameObject go, string key)
    {
        PackageProductPanel panel = go.GetComponent<PackageProductPanel>();
        Undo.RecordObject(panel, "Shop products");
        panel.rewards = new PackageProductPanel.Reward[0];
        foreach (ProductPurchaseButton b in go.GetComponentsInChildren<ProductPurchaseButton>(true))
        {
            Undo.RecordObject(b, "Shop products");
            b.key = key;
            for (int i = b.onPurchased.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(b.onPurchased, i);
            UnityEventTools.AddPersistentListener(b.onPurchased, panel.Purchased);
            EditorUtility.SetDirty(b);
        }
        Transform titleTr = go.transform.Find("Text_Title");
        if (titleTr != null)
        {
            TranslateText tt = titleTr.GetComponent<TranslateText>();
            if (tt != null)
            {
                Undo.RecordObject(tt, "Shop products");
                tt.key = $"Product.{key}";
                tt.formatArg = null;
            }
            TMP_Text t = titleTr.GetComponent<TMP_Text>();
            if (t != null)
                t.text = TranslateManager.GetText($"Product.{key}");
        }
        EditorUtility.SetDirty(panel);
    }

    static Transform Find(Transform root, string name) =>
        root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
}
