//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using UnityEditor;
using Sparrow.Verification;
using OTBT.Framework.Utils;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using System.IO;
using OTBT.Framework.Networking;


public class BuildServerFunctions
{

    static void PerformAutomatedBuilds()
    {
        string peytonMessage = ":construction_worker: **Automatic build report for project " + Application.productName + "**";
        string verifyMessage = !HasArg("-otbtSkipVerify") ? PerformVerify() : "";
        
        EditorPrefs.SetString("AndroidSdkRoot", "/opt/Unity/Editor/Data/PlaybackEngines/AndroidPlayer/SDK");
        EditorPrefs.SetString("AndroidNdkRoot", "/opt/Unity/Editor/Data/PlaybackEngines/AndroidPlayer/NDK");
        EditorPrefs.SetString("AndroidNdkRootR16b", "/opt/Unity/Editor/Data/PlaybackEngines/AndroidPlayer/NDK");
        PlayerSettings.Android.useCustomKeystore = false;

        var scenes = EditorBuildSettings.scenes;

        BuildReport firstBuildReport = null;

        if (!HasArg("-otbtSkipWindows"))
        {
            while (EditorApplication.isUpdating)
                System.Threading.Thread.Sleep(100); // Sleep for a short time to prevent high CPU usage
            if (EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
            {
                BuildReport report = BuildPipeline.BuildPlayer(scenes, $"./Builds/Windows/{Application.productName}.exe", BuildTarget.StandaloneWindows64, BuildOptions.CleanBuildCache | BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.DetailedBuildReport);
                peytonMessage += "\n" + EvaluateBuildReport("Windows", report, true);
                firstBuildReport = (firstBuildReport == null ? (report.summary.result == BuildResult.Succeeded ? report : null) : firstBuildReport);
            }
        }
        if (!HasArg("-otbtSkipMac"))
        {
            while (EditorApplication.isUpdating)
                System.Threading.Thread.Sleep(100); // Sleep for a short time to prevent high CPU usage
            if (EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX))
            {
                BuildReport report = BuildPipeline.BuildPlayer(scenes, $"./Builds/MacOSX/{Application.productName}.x64", BuildTarget.StandaloneOSX, BuildOptions.CleanBuildCache | BuildOptions.Development | BuildOptions.AllowDebugging);
                peytonMessage += "\n" + EvaluateBuildReport("Mac", report);
                firstBuildReport = (firstBuildReport == null ? (report.summary.result == BuildResult.Succeeded ? report : null) : firstBuildReport);
            }
        }
        if (!HasArg("-otbtSkipAndroid"))
        {
            while (EditorApplication.isUpdating)
                System.Threading.Thread.Sleep(100); // Sleep for a short time to prevent high CPU usage
            if (EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                BuildReport report = BuildPipeline.BuildPlayer(scenes, $"./Builds/Android/{Application.productName}.apk", BuildTarget.Android, BuildOptions.CleanBuildCache | BuildOptions.Development | BuildOptions.AllowDebugging);
                peytonMessage += "\n" + EvaluateBuildReport("Android", report);
                firstBuildReport = (firstBuildReport == null ? (report.summary.result == BuildResult.Succeeded ? report : null) : firstBuildReport);
            }
        }
        if (!HasArg("-otbtSkipWebGL"))
        {
            while (EditorApplication.isUpdating)
                System.Threading.Thread.Sleep(100); // Sleep for a short time to prevent high CPU usage
            if (EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            {
                BuildReport report = BuildPipeline.BuildPlayer(scenes, $"./Builds/Web GL", BuildTarget.WebGL, BuildOptions.CleanBuildCache);
                peytonMessage += "\n" + EvaluateBuildReport("WebGL", report);
                firstBuildReport = (firstBuildReport == null ? (report.summary.result == BuildResult.Succeeded ? report : null) : firstBuildReport);
            }
        }

        if (firstBuildReport != null)
        {
            peytonMessage += "\n\n:floppy_disk: **Top 5 largest files in build:**";
            List<(ulong, string)> newFiles = new List<(ulong, string)>();
            List<PackedAssets> files = new List<PackedAssets>(firstBuildReport.packedAssets);
            ulong totalPackedSize = 0;
            foreach(PackedAssets file in files)
            {
                foreach (PackedAssetInfo fileInfo in file.contents)
                {
                    newFiles.Add((fileInfo.packedSize, fileInfo.sourceAssetPath));
                    totalPackedSize += fileInfo.packedSize;
                }
            }
            newFiles.Sort((a, b) => -(a.Item1.CompareTo(b.Item1)));
            for (int i = 0; i < 5; i++)
            {
                peytonMessage += "\n  " + ConvertBytesToMegabytes((long)newFiles[i].Item1).ToString("0.##") + " MB" + " (" + ((float)newFiles[i].Item1 / (float)totalPackedSize).ToString("0.##") + " %): " + newFiles[i].Item2;
            }
        }

        peytonMessage += (verifyMessage.Equals("") ? "" : ("\n\n" + verifyMessage));
        peytonMessage += "\n**The successful builds and a full log are now available in the cloud: https://cloud.beatentrack.games/f/101077**";
        peytonMessage += "\n(please allow up to one minute for the upload process)";

        if (!HasArg("-otbtSilent"))
            _ = PeytonUtility.PeytonPost(peytonMessage);
    }



    private static string EvaluateBuildReport(string platform, BuildReport report, bool includeAssets = false)
    {
        string ret = (report.summary.result == BuildResult.Succeeded ? "  :white_check_mark: " + platform + " succeeded!" : "  :x: " + platform + " FAILED.");
        ret += " (" + report.summary.totalErrors + " Errors, " + report.summary.totalWarnings + " Warnings)";

        if(report.summary.totalErrors > 0 || report.summary.totalWarnings > 0)
        {
            string extended = ret;
            foreach (BuildStep step in report.steps)
            {
                foreach (BuildStepMessage message in step.messages)
                {
                    if (!(message.type == LogType.Warning || message.type == LogType.Error)) continue;
                    if (message.type == LogType.Error) 
                        extended += "<br>   ERROR: " + message.content + "<br><br>";
                    if (message.type == LogType.Warning)
                        extended += "<br>   Warning: " + message.content + "<br><br>";

                    Dbg.Log(null, (message.type == LogType.Warning ? "WARNING: " : "ERROR: ") + message.content);
                }
            }
            
            // send mail for errors; for warnings the buildLog should suffice
            if(report.summary.totalErrors > 0)
                _ = PeytonUtility.PeytonMail(extended, "OTBT Build Report: Errors found");
        }
        
        return ret;
    }

    static double ConvertBytesToMegabytes(long bytes)
    {
        return (bytes / 1024f) / 1024f;
    }

    private static string PerformVerify()
    {
        string path = "./Builds/verifyReport.txt";
        if (!Directory.Exists("./Builds"))
            Directory.CreateDirectory("./Builds");
        StreamWriter writer = new StreamWriter(path, true);

        VerifyWindow verifyWindow = new VerifyWindow();

        var dict = verifyWindow.PerformCheck(VerifyResult.CheckType.All);
        int numberOfProblems = dict.Count;// VerifyWindow.CountProblems(dict);

        string output = $"**Verify found {numberOfProblems} problems in project {Application.productName}.**\n";
        writer.WriteLine($"Verify found {numberOfProblems} problems in project {Application.productName}.\n");
       
            int areaProblems = 0;
            string tempOutput = "";
            dict.Sort((a, b) => a.category.CompareTo(b.category));
            string lastCategory = "";
            foreach (VerifyResult check in dict)
            {
                if (!check.category.Equals(lastCategory))
                {
                    lastCategory = check.category;
                    tempOutput += "\n\n" + lastCategory;
                }
                areaProblems += 1;// check.NumberOfErrors();
                //foreach (VerifyResult r in check.failedChecks)
                    tempOutput += "\n" + check.toString;
            }
            if(areaProblems > 0)
                output += "   :red_circle: "+": " + areaProblems + " problems\n";
            writer.WriteLine((areaProblems > 0 ? "   XX" : "   OK") + " " + ": " + areaProblems + " problems" + tempOutput);
        

        Dbg.Log(null, output);
        writer.Close();
        return output;
    }

    private static bool HasArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name)
            {
                return true;
            }
        }
        return false;
    }

}