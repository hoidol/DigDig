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
        if(idx < 0)
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


        string json = Resources.Load<TextAsset>("JSON/TranslateData").text;
        JSONArray arr = JSONObject.Parse(json).GetArray("JSON");
        for(int i =0;i< arr.Length; i++)
        {
            TranslateData translateData = new TranslateData();

            translateData.key = arr[i].Obj.GetString("key");

            translateData.en = arr[i].Obj.GetString("en");
            translateData.en = translateData.en.Replace("\\n", "\n");


            translateData.kr = arr[i].Obj.GetString("kr");
            translateData.kr = translateData.kr.Replace("\\n", "\n");

            //3. 시트 to Data
            translateData.vi = arr[i].Obj.GetString("vi");
            translateData.vi = translateData.vi.Replace("\\n", "\n");

            translateData.es = arr[i].Obj.GetString("es");
            translateData.es = translateData.es.Replace("\\n", "\n");

            translateData.pt = arr[i].Obj.GetString("pt");
            translateData.pt = translateData.pt.Replace("\\n", "\n");

            translateData.de = arr[i].Obj.GetString("de");
            translateData.de = translateData.de.Replace("\\n", "\n");


            translateDic.Add(translateData.key, translateData);
            
        }
    }
    static LanguageType curLanguageType;
    public static string GetText(string key)
    {
        Init();
        if (!translateDic.ContainsKey(key))
        {
            Debug.LogWarning($"{key} 해당하는 번역 데이터 없음");
            return null;
        }

        //4. 현재 언어 설정에 맞는 데이터 가져오기
        if (curLanguageType == LanguageType.kr)
        {
            return translateDic[key].kr;
        }
        else if (curLanguageType == LanguageType.vi)
        {
            return translateDic[key].vi;
        }
        else if (curLanguageType == LanguageType.de)
        {
            return translateDic[key].en;
        }
        else if (curLanguageType == LanguageType.es)
        {
            return translateDic[key].es;
        }
        else if (curLanguageType == LanguageType.pt)
        {
            return translateDic[key].pt;
        }

        return translateDic[key].en;
    }

    public static void ChangeLanguage(LanguageType languageType)
    {
        curLanguageType = languageType;
        TranslateText[] translateTexts = GameObject.FindObjectsByType<TranslateText>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < translateTexts.Length; i++)
        {
            translateTexts[i].UpdatePanel();
        }

        PopupCanvas.Instance.OpenCanvas(TranslateManager.GetText("changedLanguage"));
        PlayerPrefs.SetInt("LanguageType", (int)curLanguageType);

        changedLanguageObs.ForEach(e => e.Invoke());
    }

     static List<Action> changedLanguageObs = new List<Action>();
    public static void AddChangedLanguage(Action action)
    {
        if(!changedLanguageObs.Contains(action))
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
