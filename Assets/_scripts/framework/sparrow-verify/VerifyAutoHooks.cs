//
// Copyright (c) 2024 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Sparrow.Verification
{
    [InitializeOnLoad]
    public class VerifyAutoHooks : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            VerifyWindow.instance.FindProfiles();
            bool fail = false;
            int numberOfProblems = 0;
            int areaProblems = 0;
            string lastCategory = "";

            foreach (VerifyProfile profile in VerifyWindow.profiles)
            {
                if (!profile.autoCheckBuild) continue; 
                VerifyWindow.instance.UpdateResultDisplay(profile.checkType);

                StreamWriter writer = new StreamWriter(profile.logLocation, true);

                numberOfProblems += VerifyWindow.instance.results.Count;
                writer.WriteLine($"Sparrow Verification: Performing checks for profile {profile.name}.\n");
                writer.WriteLine($"Found {VerifyWindow.instance.results.Count} problems in project {Application.productName}.\n");

                VerifyWindow.instance.results.Sort((a, b) => a.category.CompareTo(b.category));
                foreach (VerifyResult result in VerifyWindow.instance.results)
                {
                    if (!result.category.Equals(lastCategory))
                    {
                        lastCategory = result.category;
                        writer.WriteLine("\n\n" + lastCategory);
                    }
                    areaProblems += 1;// check.NumberOfErrors();
                                      //foreach (VerifyResult r in check.failedChecks)
                    writer.WriteLine("\n" + result.severity + ": " + result.toString);

                    if (profile.autoFailBuild && result.severity == VerifyResult.Severity.Error)
                    {
                        fail = true;
                    }
                }

                writer.WriteLine((areaProblems <= 0 ? "✔ " : "✘ ") + " " + ": " + areaProblems + " problems");

                writer.Close();
            }


            if(fail) throw new BuildFailedException("Sparrow Verification stopped your build process because an error has been found and a profile is set to 'Fail Build on Error'.");
        }
    }
}

#endif