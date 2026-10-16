# DigDig 기획 노트

Claude가 기획 의도를 유지하기 위한 문서. 기획이 바뀌거나 사용자가 새 의도를 설명하면 이 파일을 갱신한다.
코드에서 바로 알 수 있는 구현 세부는 적지 않고, **왜 그렇게 하는지(의도)** 위주로 적는다.

## 핵심 루프
- 적 처치 → 코인 획득 → 슬라임 소환(뽑기) → 칸이 차면 합성 → 빈 칸에 다시 소환
- 돈이 계속 쌓이는 만큼 계속 강해질 수 있어야 한다.

## 슬라임 소환 / 합성
- 소환 버튼: **10코인**, 장착한 **5종 중 1종이 랜덤**으로 mergeLv 0으로 **16칸** 중 빈 칸에 등장.
- 합성 조건: **같은 key + 같은 mergeLv** 끼리만 (ice 0 + ice 0 O / ice + flame X / ice 0 + ice 1 X).
- mergeLv는 0부터 시작. 최대치는 `SlimeData.MAX_MERGE_LEVEL` (3단계 0~2 → 5~6까지 확장 중).
- 합성 결과 key: 현재 코드는 장착 5종 중 **랜덤** (`Slime.Merge`). 기획 의도 확인 필요.
- 슬라임 판매/버리기 기능 없음.

### 의도
- **소환 가격은 올리지 않는다.** 싸게 계속 뽑는 뽑기 재미가 핵심.
- **합성은 스탯 합계로 보면 약간 손해**여야 한다. 대신 칸이 비어서 다시 소환할 수 있다는 게 합성의 이득.
  - 칸이 남아 있으면 소환이 이득, 칸이 차면 손해를 감수하고 합성 → 이 순환이 의도.
- 강화 레벨(`userSlime.enhanceLevel`, 로비에서 영구 성장)과 머지 레벨(`mergeLevel`, 인게임 합성)은 별개.
  - 슬라임 에셋의 `level: N` 블록 = 강화 레벨, 그 안의 `values` 배열 = mergeLv별 값.
  - `values` 2개 = `기본/증가량`(선형), 그 외 = mergeLv별 값(모자라면 마지막 값).
- Ice / Flame / Wacky / Bounce: mergeLv가 오를수록 한 번에 발사하는 탄 수 증가(`shootCounts`), 탄마다 **서로 다른 타겟**을 노림.
- 랜덤 데미지(Wacky 등): 데이터에는 **중간값만** 입력, SlimeData의 `attackPowerRandomRatio`로 `중간값 × (1 ± 비율)` 범위 결정. `3~15` 같은 범위 직접 입력은 쓰지 않는다.

## 웨이브 / 페이즈
- phase 0~14, 14 = 보스. phase 0은 Normal 고정, 이후 매 phase 시작 시 `SelectWaveCanvas`에서 웨이브 선택.
  - 선택지 1개: Top / 2개: Left, Right / 3개: Left, Top, Right (`WaveWayDirection`).
- WaveType: Normal, Hard, Break, Break_Snake, Shop, Elite, MidBoss, Boss. 각 타입별 `Wave` 컴포넌트가 진행/종료 담당.
- 전투 웨이브(BattleWave): 타이머 종료 → 스폰 중지 → **필드 적 전부 제거 시** 다음 phase.
- Normal vs Hard: 스포너 설정(인스펙터 값)과 배율만 다르게, 코드는 공유하는 방향.

### 난이도 의도
- 적 체력/공격력: phase에 따라 상승 (`StageData` 수식). 배율은 Enemy 내부 판단이 아니라 **외부(웨이브/스포너)에서 전달**.
- 스폰 수: 웨이브 진행률에 따라 증가. **초반 phase는 거의 일정, 후반 phase(13 등)는 웨이브 막바지에 쏟아지게.**
  - `배율 = 1 + (PeakCountMul(phase) - 1) × waveCurve(진행률)` (`PhaseEnemySpawnPattern`).
  - 특수 스폰(BundleWave 등)은 WaveData의 `SpecialPattern`이 TRUE인 phase에서만.

## 데이터 파이프라인
- **SlimeData 배열 필드(bounces, shootCounts 등)는 에셋에 직렬화된 값이 코드 초기값보다 우선.** 코드 기본값만 바꾸면 기존 에셋엔 반영 안 됨 → 에셋도 같이 수정.
- 기획 데이터는 `Assets/Json/**/*.csv`(엑셀 → TSV). 에디터에서 `LoadData`(DataTableImporter)를 실행해야 에셋(`AddressableResources`)에 반영됨.
  - WaveData/{난이도}.csv: Phase, WaveType(`/` 구분), PeakCountMul, SpecialPattern
  - EnemyPatternData/{난이도}.csv: Phase, EnemyType, WaveSpawnCounts, WaveIntervalTimes
  - SlimeEnhance/{key}.csv: 강화 레벨별 AttackPower/AttackSpeed/AttackRange(`/`로 mergeLv 값) + 고유 스탯 컬럼
