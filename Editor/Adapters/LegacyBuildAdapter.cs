using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.IDE.Cursor
{
   /// <summary>
   /// Implements build system functionality for pre-Unity 6 versions,
   /// maintaining compatibility with the older build settings system.
   /// </summary>
   internal class LegacyBuildAdapter : IBuildAdapter
   {
      private const string LEGACY_BUILD_SETTINGS_PATH = "ProjectSettings/BuildSettings.asset";

      public void ConfigureBuildSettings(BuildTarget target, string profileName = null)
      {
         try
         {
            // Set the build target
            if (EditorUserBuildSettings.activeBuildTarget != target)
            {
               if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(target), target))
               {
                  throw new Exception($"Failed to switch to build target: {target}");
               }
            }

            // In legacy versions, we use the default build settings
            // Apply default settings based on target platform
            ApplyDefaultBuildSettings(target);

            Debug.Log($"Build settings configured for {target} (Legacy mode)");
         }
         catch (Exception ex)
         {
            Debug.LogError($"Failed to configure build settings: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
         }
      }

      public void UpdateBuildProfile(string profileName, Dictionary<string, object> settings)
      {
         try
         {
            // In legacy versions, we directly update EditorUserBuildSettings
            foreach (var setting in settings)
            {
               switch (setting.Key)
               {
                  case "developmentBuild":
                     EditorUserBuildSettings.development = Convert.ToBoolean(setting.Value);
                     break;
                  case "connectProfiler":
                     EditorUserBuildSettings.connectProfiler = Convert.ToBoolean(setting.Value);
                     break;
                  case "buildAppBundle":
                     if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                     {
                        EditorUserBuildSettings.buildAppBundle = Convert.ToBoolean(setting.Value);
                     }
                     break;
                  case "webGLMemorySize":
                     if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
                     {
                        PlayerSettings.WebGL.memorySize = Convert.ToInt32(setting.Value);
                     }
                     break;
                  default:
                     Debug.LogWarning($"Unknown or unsupported build setting in legacy mode: {setting.Key}");
                     break;
               }
            }

            // Save the settings
            AssetDatabase.SaveAssets();
            Debug.Log("Updated build settings (Legacy mode)");
         }
         catch (Exception ex)
         {
            Debug.LogError($"Failed to update build settings: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
         }
      }

      public BuildReport ExecuteBuild(string outputPath, BuildTarget target, string profileName = null)
      {
         try
         {
            // Configure build settings first
            ConfigureBuildSettings(target, profileName);

            // Get all enabled scenes from the build settings
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
               throw new InvalidOperationException("No scenes are included in the build settings.");
            }

            // Create build options based on current settings
            BuildOptions options = BuildOptions.None;
            if (EditorUserBuildSettings.development)
               options |= BuildOptions.Development;
            if (EditorUserBuildSettings.connectProfiler)
               options |= BuildOptions.ConnectWithProfiler;

            // Execute the build
            BuildReport report = BuildPipeline.BuildPlayer(
                scenes,
                outputPath,
                target,
                options
            );

            if (report.summary.result == BuildResult.Succeeded)
            {
               Debug.Log($"Build completed successfully: {outputPath}");
            }
            else
            {
               Debug.LogError($"Build failed: {report.summary.result}");
               // Legacy versions might not have detailed step information
               if (report.steps != null)
               {
                  foreach (var step in report.steps)
                  {
                     if (step.messages.Any(m => m.type == LogType.Error))
                     {
                        Debug.LogError($"Build step '{step.name}' failed:");
                        foreach (var message in step.messages.Where(m => m.type == LogType.Error))
                        {
                           Debug.LogError($"  {message.content}");
                        }
                     }
                  }
               }
            }

            return report;
         }
         catch (Exception ex)
         {
            Debug.LogError($"Failed to execute build: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
         }
      }

      private void ApplyDefaultBuildSettings(BuildTarget target)
      {
         // Set default settings based on target platform
         EditorUserBuildSettings.development = false;
         EditorUserBuildSettings.connectProfiler = false;

         switch (target)
         {
            case BuildTarget.Android:
               EditorUserBuildSettings.buildAppBundle = false;
               PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
               break;

            case BuildTarget.WebGL:
               // Legacy WebGL settings
               PlayerSettings.WebGL.memorySize = 256; // Default for older versions
               PlayerSettings.WebGL.dataCaching = true;
               break;

            case BuildTarget.iOS:
               PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
               break;

            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
               PlayerSettings.defaultScreenWidth = 1024;
               PlayerSettings.defaultScreenHeight = 768;
               PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
               break;
         }

         // Common settings
         PlayerSettings.stripUnusedMeshComponents = true;
         PlayerSettings.bakeCollisionMeshes = true;

         // Save the settings
         AssetDatabase.SaveAssets();
      }
   }
}