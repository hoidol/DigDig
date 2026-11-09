using UnityEngine;
using UnityEngine.UI;

// 유물 썸네일 (등급 배경 + 등급 아이콘). 목록 칸·정보 창·뽑기 결과 카드 공용
public class RelicThumPanel : MonoBehaviour
{
    public Image thumImage;
    public Image bgImage;
    public Image gradeImage;
    public bool isNullToInactive;

    public void SetRelicData(RelicData relicData)
    {
        thumImage.enabled = relicData != null;
        gradeImage.enabled = relicData != null;
        if (relicData != null)
        {
            gameObject.SetActive(true);
            thumImage.sprite = relicData.thum;
            bgImage.color = Grade.GetGradeColor(relicData.grade);
            gradeImage.sprite = Grade.GetGradeSprite(relicData.grade);
        }
        else
        {
            if (isNullToInactive)
                gameObject.SetActive(false);
            bgImage.color = Color.gray;
        }
    }
}
