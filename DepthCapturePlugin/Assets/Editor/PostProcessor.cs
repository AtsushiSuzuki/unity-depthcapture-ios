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
            var infoPath = Path.Combine(path, "Info.plist");
            var info = new PlistDocument();
            info.ReadFromFile(infoPath);
            info.root.SetString("NSCameraUsageDescription", "深度カメラの動作テストのためにカメラAPIを使用します");
            info.WriteToFile(infoPath);
        }
    }
}
