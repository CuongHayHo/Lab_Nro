using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class SceneSetupHelper
{
    static SceneSetupHelper()
    {
        EditorApplication.delayCall += SetupScene;
    }

    [MenuItem("Tools/Cài Đặt Toàn Bộ Scene & Background (1-Click)")]
    public static void SetupScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var activeScene = EditorSceneManager.GetActiveScene();
        if (!activeScene.isLoaded) return;

        // 1. Kiểm tra hoặc tạo BackgroundManager
        BackgroundManager bgMgr = Object.FindFirstObjectByType<BackgroundManager>();
        if (bgMgr == null)
        {
            GameObject bgGo = new GameObject("BackgroundManager");
            bgGo.transform.position = Vector3.zero;
            bgMgr = bgGo.AddComponent<BackgroundManager>();
            Debug.Log("[SceneSetupHelper] Đã tạo mới GameObject BackgroundManager trong Scene.");
        }

        bgMgr.AutoLoadSprites();
        bgMgr.UpdateBoundaries();
        bgMgr.SetupPieces();

        // 2. Đồng bộ PlayerA
        PlayerA playerA = Object.FindFirstObjectByType<PlayerA>();
        if (playerA != null)
        {
            float targetGround = BackgroundManager.GroundY;
            Vector3 pos = playerA.transform.position;
            pos.y = targetGround;
            playerA.transform.position = pos;
            playerA.headHeight = 1.5375f;
            CharacterParts cp = playerA.GetComponent<CharacterParts>();
            if (cp != null)
            {
                cp.headHeight = 1.5375f;
                cp.SyncAndApplyAll();
            }
            EditorUtility.SetDirty(playerA);
        }

        // 3. Đồng bộ EnemyB
        EnemyB[] enemies = Object.FindObjectsByType<EnemyB>(FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            if (e != null)
            {
                e.headHeight = 1.1375f;
                CharacterParts cp = e.GetComponent<CharacterParts>();
                if (cp != null)
                {
                    cp.headHeight = 1.1375f;
                    cp.SyncAndApplyAll();
                }
                EditorUtility.SetDirty(e);
            }
        }

        // 4. Kiểm tra hoặc tạo ObstacleManager
        ObstacleManager obsMgr = Object.FindFirstObjectByType<ObstacleManager>();
        if (obsMgr == null)
        {
            GameObject obsGo = new GameObject("ObstacleManager");
            obsGo.transform.position = Vector3.zero;
            obsMgr = obsGo.AddComponent<ObstacleManager>();
            Debug.Log("[SceneSetupHelper] Đã tạo mới GameObject ObstacleManager trong Scene.");
        }
        obsMgr.AutoLoadSprite();
        EditorUtility.SetDirty(obsMgr);

        EditorUtility.SetDirty(bgMgr);
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);

        Debug.Log("===> [SceneSetupHelper] CÀI ĐẶT BACKGROUND, VẬT CẢN VÀ BOUNDARY SCENE THÀNH CÔNG! <===");
    }

    [MenuItem("Tools/Build Game ra File Chạy .exe (Windows 64-bit)")]
    public static void BuildStandaloneWindows()
    {
        string buildDir = "E:/solo/Build_Game";
        if (!System.IO.Directory.Exists(buildDir))
        {
            System.IO.Directory.CreateDirectory(buildDir);
        }

        string exePath = System.IO.Path.Combine(buildDir, "Lab_NRO_2D.exe");
        string[] scenes = new string[] { "Assets/Scenes/SampleScene.unity" };

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = exePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        Debug.Log("[Build] Đang tiến hành build game ra: " + exePath);
        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("===> BUILD GAME THÀNH CÔNG! File chạy: " + exePath);
            EditorUtility.RevealInFinder(exePath);
        }
        else
        {
            Debug.LogError("[Build] Build game thất bại với kết quả: " + report.summary.result);
        }
    }

    [MenuItem("Tools/Xuất Toàn Bộ Dự Án Thành 1 File .unitypackage")]
    public static void ExportUnityPackage()
    {
        string exportPath = "E:/solo/Lab_NRO_2D_FullProject.unitypackage";
        string[] assetPaths = new string[] { "Assets" };
        AssetDatabase.ExportPackage(assetPaths, exportPath, ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
        Debug.Log("===> ĐÃ XUẤT THÀNH CÔNG FILE UNITY PACKAGE: " + exportPath);
        EditorUtility.RevealInFinder(exportPath);
    }

    [MenuItem("Tools/Build Game ra File Cài Đặt .apk (Android)")]
    public static void BuildAndroidAPK()
    {
        // 1. Cấu hình Package Name hợp lệ cho Android
        string currentId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
        if (string.IsNullOrEmpty(currentId) || currentId.Contains("DefaultCompany"))
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.solo.nro2d");
        }

        // 2. Cố định màn hình ngang (Landscape)
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

        string buildDir = "E:/solo/Build_Android";
        if (!System.IO.Directory.Exists(buildDir))
        {
            System.IO.Directory.CreateDirectory(buildDir);
        }

        string apkPath = System.IO.Path.Combine(buildDir, "Lab_NRO_2D.apk");
        string[] scenes = new string[] { "Assets/Scenes/SampleScene.unity" };

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = apkPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        Debug.Log("[Build Android] Đang tiến hành build file APK ra: " + apkPath);
        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("===> BUILD FILE APK THÀNH CÔNG! File cài đặt: " + apkPath);
            EditorUtility.RevealInFinder(apkPath);
        }
        else
        {
            Debug.LogError("[Build Android] Build APK chưa thành công: " + report.summary.result);
        }
    }
}
