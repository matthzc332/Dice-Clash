using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using System.IO;

public class BuildScript
{
    static string[] scenes = { "Assets/Scenes/MainMenuScene.unity", "Assets/Scenes/SampleScene.unity" };
    static string buildNumberPath = "Builds/.buildnumber";
    static string buildLogPath = "Builds/BUILD_LOG.txt";

    static int ReadBuildNumber()
    {
        if (!File.Exists(buildNumberPath)) return 1;
        string s = File.ReadAllText(buildNumberPath).Trim();
        if (int.TryParse(s, out int n)) return n;
        return 1;
    }

    static void WriteBuildNumber(int n)
    {
        File.WriteAllText(buildNumberPath, n.ToString());
    }

    static void LogBuild(int number, string target)
    {
        string line = $"{number:D4} | {System.DateTime.Now:yyyy-MM-dd HH:mm} | {target}";
        File.AppendAllText(buildLogPath, line + "\n");
    }

    [MenuItem("Build/Build Standalone Windows")]
    public static void BuildWindows()
    {
        int buildNum = ReadBuildNumber();
        string dir = $"Builds/v{buildNum:D4}";
        Directory.CreateDirectory(dir);
        string exePath = $"{dir}/DiceClashTactics_v{buildNum:D4}.exe";

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = exePath,
            target = BuildTarget.StandaloneWindows,
            options = BuildOptions.None
        };

        BuildPipeline.BuildPlayer(options);
        WriteBuildNumber(buildNum + 1);
        LogBuild(buildNum, "StandaloneWindows");
        Debug.Log($"Windows build v{buildNum:D4} complete → {exePath}");
    }

    [MenuItem("Build/Build WebGL")]
    public static void BuildWebGL()
    {
        int buildNum = ReadBuildNumber();
        string dir = $"Builds/v{buildNum:D4}_WebGL";
        Directory.CreateDirectory(dir);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = dir,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildPipeline.BuildPlayer(options);
        WriteBuildNumber(buildNum + 1);
        LogBuild(buildNum, "WebGL");
        Debug.Log($"WebGL build v{buildNum:D4} complete → {dir}");
    }
}
