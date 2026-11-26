# Firebase Realtime Database 보안 규칙 (원본)

콘솔(Realtime Database → 규칙)에 붙여 넣어 게시. 규칙을 바꾸면 이 파일도 같이 고칠 것.

- `ranking.rules.json` → 랭킹 DB (`dreamofslime-ranking`): rankings · shardMap · clearTips(.indexOn updatedAt·pinned) · clearTipConfig
- `userdata.rules.json` → 유저 데이터 DB (`dreamofslime-userdatas-1`, `-2`): users/{uid} 본인만

콘솔에서 직접 넣는 데이터(운영자 고정 팁 `pinned: true`, 금칙어 `clearTipConfig/bannedWords`)는 규칙과 상관없이 들어감.
운영자 팁 예: `clearTips/Normal_0/admin_1 = { nickname, text, updatedAt: 0, pinned: true }` (key 이름은 `updatedAt`)
