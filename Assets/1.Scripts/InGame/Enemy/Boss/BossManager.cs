using UnityEngine;

public class BossManager : MonoBehaviour
{
    public void Awake()
    {
        GameEventBus.Subscribe<BossEvent>(OnBossEvent);
    }

    void OnBossEvent(BossEvent e)
    {
        //보스 연출 시 

        // 흐름
        // 플레이어로부터 Y로 20만큼 떨어진 위치에 보스 소환
        // 카메라를 위로 빠르게 올려서 보스 보여주고 + 이름 노출 (2초 멈춤)

        // 대사 : 잘도 찾아왔구나! 그만 죽거라!

        // 즉시 플레이어쪽으로 화면 전환
        // 보스 공격 시 붉은 두꺼운 아웃라인! - Rendering Feature 써보자 

        //Camera UI를 써서 
        //보스 썸네일 + 이름 보여주고 등장 연출
        //
        // 화면 전체가 붉게 물드는 페이드(FadeCanvas)
        // 카메라 진동(진폭 큰 shake)
        // 보스 위치에서 균열 파티클 → 보스 등장
        // 보스 이름 +대사 UI가 화면 중앙에 타이핑 효과로 출력
        // 보스 스프라이트 색이 붉게 물들며 아우라 이펙트 활성화




    }
}