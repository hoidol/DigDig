#!/bin/zsh
# 메인 프로젝트(커밋 + 커밋 안 한 변경)를 테스트 전용 복사본(git worktree)에 맞춘다.
# 1) 테스트 폴더를 메인의 현재 커밋으로 맞추고  2) 작업 중인 파일을 rsync로 덮어씀(삭제 포함).
set -e
MAIN=${MAIN:-${0:A:h:h}} # 이 스크립트가 있는 프로젝트
TEST=${TEST:-$HOME/KingSlime-Test}

SHA=$(git -C "$MAIN" rev-parse HEAD)
git -C "$TEST" reset -q --hard "$SHA"

for dir in Assets Packages ProjectSettings; do
  rsync -a --delete --exclude '.DS_Store' "$MAIN/$dir/" "$TEST/$dir/"
done

echo "synced: $(git -C "$MAIN" rev-parse --short HEAD) + working tree → $TEST"
