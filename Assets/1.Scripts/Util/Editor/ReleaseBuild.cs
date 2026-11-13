using System;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// 테스트 배포용 배치 빌드 (Tools/build-release.sh가 -executeMethod로 호출).
//   -buildNumber N   Android versionCode / iOS buildNumber
//   -outputPath P    결과 경로 (Android = .apk 파일, iOS = Xcode 프로젝트 폴더)
// Android 서명: 환경 변수 KS_KEYSTORE_PATH / KS_KEYSTORE_PASS / KS_KEY_ALIAS / KS_KEY_PASS가 있으면 그 키, 없으면 디버그 키.
// iOS 서명: 환경 변수 KS_APPLE_TEAM_ID (자동 서명).
public static class ReleaseBuild
{
    public const string APP_ID = "com.BEI.KingSlime";

    public static void BuildAndroid()
    {
        Run(BuildTarget.Android, NamedBuildTarget.Android, () =>
        {
            PlayerSettings.Android.bundleVersionCode = GetBuildNumber();
            EditorUserBuildSettings.buildAppBundle = false; // App Distribution은 APK가 바로 설치됨 (AAB는 Play 연결 필요)

            string keystore = Environment.GetEnvironmentVariable("KS_KEYSTORE_PATH");
            PlayerSettings.Android.useCustomKeystore = !string.IsNullOrEmpty(keystore);
            if (PlayerSettings.Android.useCustomKeystore)
            {
                PlayerSettings.Android.keystoreName = keystore;
                PlayerSettings.Android.keystorePass = Environment.GetEnvironmentVariable("KS_KEYSTORE_PASS");
                PlayerSettings.Android.keyaliasName = Environment.GetEnvironmentVariable("KS_KEY_ALIAS");
                PlayerSettings.Android.keyaliasPass = Environment.GetEnvironmentVariable("KS_KEY_PASS");
            }
        });
    }

    public static void BuildIOS()
    {
        Run(BuildTarget.iOS, NamedBuildTarget.iOS, () =>
        {
            PlayerSettings.iOS.buildNumber = GetBuildNumber().ToString();
            string teamId = Environment.GetEnvironmentVariable("KS_APPLE_TEAM_ID");
            if (!string.IsNullOrEmpty(teamId))
            {
                PlayerSettings.iOS.appleDeveloperTeamID = teamId;
                PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            }
        });
    }

    static void Run(BuildTarget target, NamedBuildTarget named, Action setup)
    {
        string output = GetArg("-outputPath") ?? throw new Exception("-outputPath 없음");

        if (EditorUserBuildSettings.activeBuildTarget != target)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(target), target);

        PlayerSettings.SetApplicationIdentifier(named, APP_ID);
        setup();

        // Addressables는 플레이어 빌드에 자동 포함되지 않으므로 먼저 빌드
        AddressableAssetSettings.CleanPlayerContent();
        AddressableAssetSettings.BuildPlayerContent(out var addrResult);
        if (!string.IsNullOrEmpty(addrResult.Error))
            Fail("Addressables 빌드 실패: " + addrResult.Error);

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
        var options = new BuildPlayerOptions
        {
            scenes = Array.ConvertAll(Array.FindAll(EditorBuildSettings.scenes, s => s.enabled), s => s.path),
            locationPathName = output,
            target = target,
            options = BuildOptions.None,
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            Fail($"빌드 실패: {report.summary.result} (에러 {report.summary.totalErrors})");

        Debug.Log($"[ReleaseBuild] 성공 {target} v{PlayerSettings.bundleVersion} ({GetBuildNumber()}) → {output}");
        EditorApplication.Exit(0);
    }

    static int GetBuildNumber()
    {
        string arg = GetArg("-buildNumber");
        return arg != null ? int.Parse(arg) : 1;
    }

    static string GetArg(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == name) return args[i + 1];
        return null;
    }

    static void Fail(string msg)
    {
        Debug.LogError("[ReleaseBuild] " + msg);
        EditorApplication.Exit(1);
    }
}
