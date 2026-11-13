using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Boomlagoon.JSON;
using System;

public class TranslateManager
{
    static bool init = false;

    public static Dictionary<string, TranslateData> translateDic = new Dictionary<string, TranslateData>();


    public static void Init()
    {
        if (init)
            return;
        init = true;
        int idx = PlayerPrefs.GetInt("LanguageType", -1);
        if (idx < 0)
        {
            //2. 시스템 언어로 번역 언어 설정
            curLanguageType = LanguageType.en;

            switch (Application.systemLanguage)
            {
                case SystemLanguage.Korean:
                    curLanguageType = LanguageType.kr;
                    break;

                case SystemLanguage.Vietnamese:
                    curLanguageType = LanguageType.vi;
                    break;

                case SystemLanguage.Dutch:
                    curLanguageType = LanguageType.de;
                    break;

                case SystemLanguage.Spanish:
                    curLanguageType = LanguageType.es;
                    break;

                case SystemLanguage.Portuguese:
                    curLanguageType = LanguageType.pt;
                    break;

                default:
                    // 기본적으로 영어로 설정
                    curLanguageType = LanguageType.en;
                    break;
            }
        }
        else
        {
            curLanguageType = (LanguageType)idx;
        }
        PlayerPrefs.SetInt("LanguageType", (int)curLanguageType);

        // Json/Translate.csv → Resources/TranslateData.asset (DataTableImporter)
        translateDic.Clear();
        TranslateDataTable table = Resources.Load<TranslateDataTable>(TranslateDataTable.RESOURCE_PATH);
        if (table == null)
        {
            Debug.LogWarning("[TranslateManager] Resources/TranslateData 없음 (Tools/Data/Import All 실행 필요)");
            return;
        }
        foreach (TranslateDataTable.Row row in table.rows)
        {
            if (string.IsNullOrEmpty(row.key) || translateDic.ContainsKey(row.key))
                continue;
            translateDic.Add(row.key, new TranslateData
            {
                key = row.key,
                kr = (row.kr ?? "").Replace("\\n", "\n"),
                en = (row.en ?? "").Replace("\\n", "\n"),
            });
        }
    }
    static LanguageType curLanguageType;
    // 번역 없음 → "*key" (빠진 번역이 화면에서 보이게). 현재 언어 칸이 비어 있으면 Kr → 그래도 없으면 En
    // 시트에는 Kr / En만 있음 → 그 외 언어는 En
    public static string GetText(string key)
    {
        Init();
        if (string.IsNullOrEmpty(key) || !translateDic.TryGetValue(key, out TranslateData data))
            return $"*{key}";

        string text = curLanguageType == LanguageType.kr ? data.kr : data.en;
        if (string.IsNullOrEmpty(text))
            text = !string.IsNullOrEmpty(data.kr) ? data.kr : data.en;
        return string.IsNullOrEmpty(text) ? $"*{key}" : text;
    }

    public static bool HasKey(string key)
    {
        Init();
        return !string.IsNullOrEmpty(key) && translateDic.ContainsKey(key);
    }

    public static void ChangeLanguage(LanguageType languageType)
    {
        curLanguageType = languageType;
        TranslateText[] translateTexts = GameObject.FindObjectsByType<TranslateText>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < translateTexts.Length; i++)
        {
            translateTexts[i].UpdatePanel();
        }

        PlayerPrefs.SetInt("LanguageType", (int)curLanguageType);
        if (PopupCanvas.Instance != null)
            PopupCanvas.Instance.OpenCanvas(TranslateManager.GetText("changedLanguage"));
        else
            ToastCanvas.Toast(TranslateManager.GetText("changedLanguage"));

        changedLanguageObs.ForEach(e => e.Invoke());
    }

    static List<Action> changedLanguageObs = new List<Action>();
    public static void AddChangedLanguage(Action action)
    {
        if (!changedLanguageObs.Contains(action))
            changedLanguageObs.Add(action);
    }
    public static void RemoveChangedLanguage(Action action)
    {
        if (changedLanguageObs.Contains(action))
            changedLanguageObs.Remove(action);
    }
    public static string GetTitle(LanguageType languageType)
    {
        switch (languageType)
        {
            case LanguageType.kr:
                return "한국어";
            case LanguageType.vi:
                return "Tiếng Việt";
            case LanguageType.de:
                return "Deutsch";
            case LanguageType.es:
                return "Español";
            case LanguageType.pt:
                return "Português";

            default:
                return "English";

        }
    }

    public static LanguageType GetLanguageType()
    {
        return curLanguageType;
    }
}

public class TranslateData
{
    public string key;
    public string en;
    public string kr;
    public string vi;//1. 변수 추가
    public string es;
    public string pt;
    public string de;

}
//0. 열거형에 번역 언어 추가 
public enum LanguageType
{
    kr,
    en,
    vi,
    de,//독일어
    es,// 스페인어
    pt,//포르투칼어
    th,//태국어 
    ru,// 러시아어 (Russian)
    ja,//일본어 (Japanese)	
    zh,//중국어 (Chinese)	

}
