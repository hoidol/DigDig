#!/bin/zsh
# 테스트 배포: 빌드 전용 복사본(~/KingSlime-Build, git worktree)에서 빌드 → Firebase App Distribution 업로드.
# 메인 에디터·테스트 복사본(~/KingSlime-Test)의 플랫폼을 건드리지 않는다.
#   Tools/build-release.sh android              Android APK
#   Tools/build-release.sh ios                  iOS IPA (Unity iOS 모듈 + Apple Team ID 필요)
#   Tools/build-release.sh all -m "릴리스 노트"  둘 다 (노트 생략 시 마지막 커밋 메시지)
#   NO_UPLOAD=1 ...                             빌드만
# 설정·비밀 값: ~/.kingslime-release/config.sh (git에 넣지 않음, 아래 기본값을 덮어씀)
set -e
MAIN=${MAIN:-${0:A:h:h}}
BUILD=${BUILD:-$HOME/KingSlime-Build}
UNITY=${UNITY:-/Applications/Unity/Hub/Editor/$(sed -n 's/^m_EditorVersion: //p' "$MAIN/ProjectSettings/ProjectVersion.txt")/Unity.app/Contents/MacOS/Unity}

# Firebase 앱 ID (Assets/google-services.json, GoogleService-Info.plist)
FIREBASE_ANDROID_APP=1:327107647483:android:7ef74f22296cc08ba21d5e
FIREBASE_IOS_APP=1:327107647483:ios:43458eec530c7455a21d5e
FIREBASE_GROUPS=${FIREBASE_GROUPS:-testers}   # App Distribution 테스터 그룹 alias (쉼표 구분)
CONFIG=$HOME/.kingslime-release/config.sh
[[ -f $CONFIG ]] && source "$CONFIG"

PLATFORM=$1; shift || true
NOTES=""
while [[ $# -gt 0 ]]; do
  case $1 in -m) NOTES=$2; shift 2 ;; *) echo "알 수 없는 인자: $1"; exit 1 ;; esac
done
[[ $PLATFORM == (android|ios|all) ]] || { echo "사용법: $0 android|ios|all [-m 노트]"; exit 1 }
[[ -z $NOTES ]] && NOTES="$(git -C "$MAIN" log -1 --pretty=%s) ($(git -C "$MAIN" rev-parse --short HEAD))"
BUILD_NUMBER=${BUILD_NUMBER:-$(date +%y%m%d%H%M | cut -c1-9)} # 년월일시 + 분 앞자리 (int 범위)

# 빌드 복사본 준비 + 메인의 커밋·작업 중 변경 동기화
if [[ ! -d $BUILD ]]; then
  echo "빌드 복사본 생성: $BUILD (첫 빌드는 에셋 임포트로 오래 걸림)"
  git -C "$MAIN" worktree add --detach "$BUILD" HEAD
fi
TEST=$BUILD "$MAIN/Tools/sync-test-project.sh"
if unity status 2>/dev/null | awk -v p="$BUILD" '$3 == p' | grep -q .; then
  echo "빌드 복사본 에디터가 열려 있습니다. 닫고 다시 실행하세요."; exit 1
fi

OUT=$BUILD/Builds
mkdir -p "$OUT"

unity_build() { # $1 = method, $2 = buildTarget, $3 = outputPath, $4 = log
  echo "Unity 빌드 ($2, #$BUILD_NUMBER) … log: $4"
  if ! "$UNITY" -batchmode -nographics -quit -projectPath "$BUILD" -buildTarget "$2" \
      -executeMethod "ReleaseBuild.$1" -buildNumber "$BUILD_NUMBER" -outputPath "$3" -logFile "$4"; then
    echo "빌드 실패. 로그 발췌:"; grep -E "error CS|\[ReleaseBuild\]|Exception|BuildFailed|FAILURE" "$4" | head -30; exit 1
  fi
}

distribute() { # $1 = file, $2 = app id
  [[ -n $NO_UPLOAD ]] && { echo "업로드 생략: $1"; return }
  firebase appdistribution:distribute "$1" --app "$2" --release-notes "$NOTES" --groups "$FIREBASE_GROUPS"
}

if [[ $PLATFORM == (android|all) ]]; then
  APK=$OUT/Android/KingSlime.apk
  unity_build BuildAndroid Android "$APK" "$OUT/android.log"
  distribute "$APK" $FIREBASE_ANDROID_APP
fi

if [[ $PLATFORM == (ios|all) ]]; then
  [[ -n $KS_APPLE_TEAM_ID ]] || { echo "KS_APPLE_TEAM_ID 없음 ($CONFIG)"; exit 1 }
  XC=$OUT/iOS/Xcode
  unity_build BuildIOS iOS "$XC" "$OUT/ios.log"

  # Firebase SDK(CocoaPods)가 있으면 workspace가 생김
  if [[ -d $XC/Unity-iPhone.xcworkspace ]]; then proj=(-workspace "$XC/Unity-iPhone.xcworkspace")
  else proj=(-project "$XC/Unity-iPhone.xcodeproj"); fi
  ARCHIVE=$OUT/iOS/KingSlime.xcarchive
  echo "Xcode archive … log: $OUT/ios-xcode.log"
  xcodebuild "${proj[@]}" -scheme Unity-iPhone -configuration Release -destination generic/platform=iOS \
    -archivePath "$ARCHIVE" -allowProvisioningUpdates DEVELOPMENT_TEAM=$KS_APPLE_TEAM_ID CODE_SIGN_STYLE=Automatic \
    archive > "$OUT/ios-xcode.log" 2>&1 || { tail -30 "$OUT/ios-xcode.log"; exit 1 }

  # Ad Hoc(release-testing) 서명: 테스터 기기 UDID가 Apple Developer에 등록돼 있어야 설치됨
  cat > "$OUT/iOS/ExportOptions.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0"><dict>
  <key>method</key><string>release-testing</string>
  <key>teamID</key><string>$KS_APPLE_TEAM_ID</string>
  <key>signingStyle</key><string>automatic</string>
</dict></plist>
EOF
  rm -rf "$OUT/iOS/ipa"
  xcodebuild -exportArchive -archivePath "$ARCHIVE" -exportPath "$OUT/iOS/ipa" \
    -exportOptionsPlist "$OUT/iOS/ExportOptions.plist" -allowProvisioningUpdates >> "$OUT/ios-xcode.log" 2>&1 \
    || { tail -30 "$OUT/ios-xcode.log"; exit 1 }
  distribute "$(ls "$OUT"/iOS/ipa/*.ipa | head -1)" $FIREBASE_IOS_APP
fi

echo "완료 #$BUILD_NUMBER: $NOTES"
