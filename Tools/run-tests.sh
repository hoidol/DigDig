#!/bin/zsh
# 테스트 전용 복사본(~/KingSlime-Test)을 동기화하고 EditMode 테스트를 돌린다.
# 메인 에디터를 띄워 둔 채로 작업하면서 테스트할 수 있다.
# - 테스트 복사본 에디터가 열려 있으면 그 에디터에서 실행(unity CLI), 아니면 배치모드로 실행
#   Tools/run-tests.sh                       전체
#   Tools/run-tests.sh "KSUpgradePlayTest"   테스트 이름 필터 (에디터: 부분 일치 / 배치모드: ; 구분, 정규식 가능)
#   NO_SYNC=1 Tools/run-tests.sh             동기화 생략
set -e
MAIN=${MAIN:-${0:A:h:h}} # 이 스크립트가 있는 프로젝트
TEST=${TEST:-$HOME/KingSlime-Test}
UNITY=${UNITY:-/Applications/Unity/Hub/Editor/$(sed -n 's/^m_EditorVersion: //p' "$MAIN/ProjectSettings/ProjectVersion.txt")/Unity.app/Contents/MacOS/Unity}
OUT=$TEST/TestResults
FILTER=$1

[[ -z $NO_SYNC ]] && "$MAIN/Tools/sync-test-project.sh"

# 열린 에디터가 있으면 그쪽에서 실행 (같은 프로젝트를 배치모드로 또 열 수 없음)
editor_ready() { unity status 2>/dev/null | awk -v p="$TEST" '$3 == p && $2 == "ready"' | grep -q . }
if unity status 2>/dev/null | awk -v p="$TEST" '$3 == p' | grep -q .; then
  cli=(unity cmd --project-path "$TEST")
  echo "에디터에서 실행 (동기화 후 재컴파일 대기)"
  sleep 5
  until editor_ready; do sleep 5; done
  unity recompile --project-path "$TEST" >/dev/null 2>&1 || true
  until editor_ready; do sleep 5; done
  errors=$(unity recompile --project-path "$TEST" 2>&1 | tail -n +2)
  if [[ -n $errors ]]; then
    echo "컴파일 에러:"; echo "$errors"; exit 1
  fi

  "${cli[@]}" test run --mode editor ${FILTER:+--filter "$FILTER"} --async-tests true >/dev/null
  sleep 20
  # 플레이 모드 진입·도메인 리로드 중엔 응답이 없으므로 completed가 나올 때까지 대기
  while true; do
    st=$("${cli[@]}" test status --format json 2>/dev/null || true)
    [[ $st == *'"status":"completed"'* || $st == *'"status": "completed"'* ]] && break
    sleep 10
  done
  python3 - "$st" <<'PY'
import sys, json
d = json.loads(sys.argv[1]); r = d.get('result', d); s = r['summary']
print(f"\n전체 {s['total']} · 통과 {s['passed']} · 실패 {s['failed']} · 스킵 {s['skipped']} · {float(r.get('duration', 0)):.1f}s")
for x in r['results']:
    if x['Status'] == 'Failed':
        print(f"\n✗ {x['FullName']}\n  {(x.get('Message') or '').strip()[:1500]}")
sys.exit(1 if s['failed'] else 0)
PY
  exit $?
fi

mkdir -p "$OUT"
rm -f "$OUT/results.xml"
args=(-batchmode -projectPath "$TEST" -runTests -testPlatform EditMode
      -testResults "$OUT/results.xml" -logFile "$OUT/editor.log")
[[ -n $FILTER ]] && args+=(-testFilter "$FILTER")

echo "running tests${FILTER:+ ($FILTER)} … log: $OUT/editor.log"
set +e
"$UNITY" "${args[@]}"
code=$?
set -e

if [[ ! -f $OUT/results.xml ]]; then
  echo "결과 파일 없음 (exit $code). 컴파일 에러 등 확인:"
  grep -E "error CS|Exception|Aborting" "$OUT/editor.log" | head -30
  exit 1
fi

python3 - "$OUT/results.xml" <<'PY'
import sys, xml.etree.ElementTree as ET
root = ET.parse(sys.argv[1]).getroot()
a = root.attrib
print(f"\n전체 {a.get('total')} · 통과 {a.get('passed')} · 실패 {a.get('failed')} · 스킵 {a.get('skipped')} · {float(a.get('duration', 0)):.1f}s")
for tc in root.iter('test-case'):
    if tc.get('result') == 'Failed':
        msg = (tc.findtext('failure/message') or '').strip()
        print(f"\n✗ {tc.get('fullname')}\n  {msg[:1500]}")
PY
exit $code
