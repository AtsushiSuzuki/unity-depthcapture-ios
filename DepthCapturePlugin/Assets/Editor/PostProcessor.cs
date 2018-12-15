using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class Postprocessor
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            var projectPath = Path.Combine(path, "Unity-iPhone.xcodeproj/project.pbxproj");
            var project = new PBXProject();
            project.ReadFromFile(projectPath);
            var target = project.TargetGuidByName("Unity-iPhone");
            project.SetBuildProperty(target, "SWIFT_VERSION", "4.2");
            project.WriteToFile(projectPath);

            var infoPath = Path.Combine(path, "Info.plist");
            var info = new PlistDocument();
            info.ReadFromFile(infoPath);
            info.root.SetString("NSCameraUsageDescription", "深度カメラの動作テストのためにカメラAPIを使用します");
            info.WriteToFile(infoPath);
        }
    }
}
