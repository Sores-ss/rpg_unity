using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Headless build entry points, invoked from the command line:
///   Unity -batchmode -nographics -quit -projectPath RPG -executeMethod BuildScript.BuildLinux
/// Optional: -buildOutput <dir> overrides the output root (defaults to ../Builds next to the project).
/// </summary>
public static class BuildScript
{
    private const string ProductName = "EldenPixel";

    [MenuItem("Build/Linux x64")]
    public static void BuildLinux()  => Build(BuildTarget.StandaloneLinux64,   "Linux",   ProductName + ".x86_64");

    [MenuItem("Build/Windows x64")]
    public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "Windows", ProductName + ".exe");

    [MenuItem("Build/All platforms")]
    public static void BuildAll()
    {
        BuildLinux();
        BuildWindows();
    }

    private static void Build(BuildTarget target, string folderName, string executableName)
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
            Fail("No scene enabled in Build Settings.");

        string outputRoot = GetArg("-buildOutput") ?? Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "Builds"));
        string outputDir  = Path.Combine(outputRoot, folderName);
        Directory.CreateDirectory(outputDir);

        var options = new BuildPlayerOptions
        {
            scenes           = scenes,
            locationPathName = Path.Combine(outputDir, executableName),
            target           = target,
            options          = BuildOptions.None,
        };

        Debug.Log($"[BuildScript] Building {target} -> {options.locationPathName}");
        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result != BuildResult.Succeeded)
            Fail($"{target} build failed: {summary.result} ({summary.totalErrors} error(s)).");

        Debug.Log($"[BuildScript] {target} build succeeded: {summary.totalSize / (1024UL * 1024UL)} MB in {summary.totalTime.TotalSeconds:F0}s");
    }

    private static string GetArg(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == name)
                return args[i + 1];
        return null;
    }

    private static void Fail(string message)
    {
        Debug.LogError("[BuildScript] " + message);
        if (Application.isBatchMode)
            EditorApplication.Exit(1);
        throw new BuildFailedException(message);
    }
}
