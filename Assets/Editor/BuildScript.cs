using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

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

        BuildReport report = BuildPipeline.BuildPlayer(options);
        WriteBuildNumber(buildNum + 1);
        LogBuild(buildNum, "WebGL");
        LogBuildReport(report);
        Debug.Log($"WebGL build v{buildNum:D4} complete → {dir}");
    }

    static void LogBuildReport(BuildReport report)
    {
        if (report == null) return;
        string outPath = "Builds/websize_report.txt";
        List<string> lines = new List<string>();
        lines.Add($"== WebGL Build Report | total {report.summary.totalSize:N0} bytes ({report.summary.totalSize / 1048576.0:N2} MB) ==");

        List<KeyValuePair<string, ulong>> assets = new List<KeyValuePair<string, ulong>>();
        foreach (UnityEditor.Build.Reporting.PackedAssetInfo info in report.packedAssets.SelectMany(p => p.contents))
        {
            string name = info.sourceAssetPath;
            if (string.IsNullOrEmpty(name)) name = info.type.ToString();
            assets.Add(new KeyValuePair<string, ulong>(name, info.packedSize));
        }

        Dictionary<string, ulong> grouped = new Dictionary<string, ulong>();
        foreach (KeyValuePair<string, ulong> a in assets)
        {
            string key = a.Key;
            if (!grouped.ContainsKey(key)) grouped[key] = 0;
            grouped[key] += a.Value;
        }

        IEnumerable<KeyValuePair<string, ulong>> sorted = grouped.OrderByDescending(a => a.Value).Take(40);
        ulong totalListed = 0;
        foreach (KeyValuePair<string, ulong> a in sorted)
        {
            lines.Add($"{(a.Value / 1048576.0):N2} MB\t{a.Key}");
            totalListed += a.Value;
        }
        lines.Add($"-- listed {sorted.Count()} assets, {totalListed / 1048576.0:N2} MB --");
        File.WriteAllLines(outPath, lines.ToArray());
        Debug.Log("Build report written to " + outPath);
    }

    [MenuItem("Build/Build Android APK")]
    public static void BuildAndroid()
    {
        int buildNum = ReadBuildNumber();
        string dir = "Builds/v" + buildNum.ToString("D4") + "_Android";
        Directory.CreateDirectory(dir);
        string apkPath = dir + "/DiceClashTactics_v" + buildNum.ToString("D4") + ".apk";

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = apkPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildPipeline.BuildPlayer(options);
        WriteBuildNumber(buildNum + 1);
        LogBuild(buildNum, "Android");
        Debug.Log($"Android apk v{buildNum:D4} complete → {apkPath}");
    }

    public static void BuildWindowsCLI()
    {
        BuildWindows();
    }

    public static void BuildAndroidCLI()
    {
        BuildAndroid();
    }

    public static void BuildWebGLCLI()
    {
        BuildWebGL();
    }
}
