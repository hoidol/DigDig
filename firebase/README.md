# Firebase Realtime Database 보안 규칙 (원본)

콘솔(Realtime Database → 규칙)에 붙여 넣어 게시. 규칙을 바꾸면 이 파일도 같이 고칠 것.

- `ranking.rules.json` → 랭킹 DB (`dreamofslime-ranking`): rankings · shardMap · clearTips(.indexOn updatedAt·pinned) · clearTipConfig
- `userdata.rules.json` → 유저 데이터 DB (`dreamofslime-userdatas-1`, `-2`): users/{uid} 본인만
- `shared.rules.json` → 서버 공용 DB (`dreamofslime-37cb2-default-rtdb`): nicknames/{소문자 닉네임} = uid (닉네임 중복 확인, 본인만 선점·해제)
- `common.rules.json` → 플레이 기록 DB (`dreamofslime-multigame-common-data`): meta/nextIndex(올린 판 수) · index/{0~99}(요약) · records/{0~99}(기록 JSON) — 100칸 돌려쓰기, 멀티 AI 봇이 읽음
- `multi.rules.json` → 멀티 게임 DB (`dreamofslime-mutlgame-data1`): lobby(대기열) · rooms/{방 번호}(.indexOn createdAt). 방 삭제는 시작(fullAt, 없으면 createdAt) 5시간이 지난 방만 허용 — 앱이 대기열에 들어갈 때 오래된 방을 지움

콘솔에서 직접 넣는 데이터(운영자 고정 팁 `pinned: true`, 금칙어 `clearTipConfig/bannedWords`)는 규칙과 상관없이 들어감.
운영자 팁 예: `clearTips/Normal_0/admin_1 = { nickname, text, updatedAt: 0, pinned: true }` (key 이름은 `updatedAt`)
