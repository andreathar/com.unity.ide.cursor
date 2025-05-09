using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.IDE.Cursor.Adapters
{
   /// <summary>
   /// Build system adapter for Unity 2022.3 LTS.
   /// Handles build settings, platform-specific configurations, and build pipeline integration.
   /// </summary>
   public class Unity2022_3BuildAdapter : IBuildAdapter
   {
      private const string BUILD_PROFILES_PATH = "ProjectSettings/BuildProfiles";

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

            // Load or create build profile
            string profilePath = profileName != null
                ? Path.Combine(BUILD_PROFILES_PATH, $"{profileName}.json")
                : Path.Combine(BUILD_PROFILES_PATH, $"{target.ToString()}_Default.json");

            if (!Directory.Exists(BUILD_PROFILES_PATH))
            {
               Directory.CreateDirectory(BUILD_PROFILES_PATH);
            }

            if (!File.Exists(profilePath))
            {
               CreateDefaultBuildProfile(profilePath, target);
            }

            // Apply the build profile
            ApplyBuildProfile(profilePath);

            Debug.Log($"Build settings configured for {target} using profile: {Path.GetFileNameWithoutExtension(profilePath)}");
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
            string profilePath = Path.Combine(BUILD_PROFILES_PATH, $"{profileName}.json");
            if (!File.Exists(profilePath))
            {
               throw new FileNotFoundException($"Build profile not found: {profileName}");
            }

            var profile = JsonUtility.FromJson<BuildProfileData>(File.ReadAllText(profilePath));

            foreach (var setting in settings)
            {
               switch (setting.Key)
               {
                  case "developmentBuild":
                     profile.isDevelopmentBuild = Convert.ToBoolean(setting.Value);
                     break;
                  case "connectProfiler":
                     profile.connectProfiler = Convert.ToBoolean(setting.Value);
                     break;
                  case "buildAppBundle":
                     profile.buildAppBundle = Convert.ToBoolean(setting.Value);
                     break;
                  case "webGLMemorySize":
                     profile.webGLMemorySize = Convert.ToInt32(setting.Value);
                     break;
                  case "webGLLinkerTarget":
                     profile.webGLLinkerTarget = setting.Value.ToString();
                     break;
                  default:
                     Debug.LogWarning($"Unknown build setting: {setting.Key}");
                     break;
               }
            }

            File.WriteAllText(profilePath, JsonUtility.ToJson(profile, true));
            ApplyBuildProfile(profilePath);

            Debug.Log($"Updated build profile: {profileName}");
         }
         catch (Exception ex)
         {
            Debug.LogError($"Failed to update build profile: {ex.Message}\nStackTrace: {ex.StackTrace}");
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

            // Create build options based on profile
            BuildOptions options = BuildOptions.None;
            string profilePath = profileName != null
                ? Path.Combine(BUILD_PROFILES_PATH, $"{profileName}.json")
                : Path.Combine(BUILD_PROFILES_PATH, $"{target.ToString()}_Default.json");

            if (File.Exists(profilePath))
            {
               var profile = JsonUtility.FromJson<BuildProfileData>(File.ReadAllText(profilePath));
               if (profile.isDevelopmentBuild)
                  options |= BuildOptions.Development;
               if (profile.connectProfiler)
                  options |= BuildOptions.ConnectWithProfiler;
            }

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

            return report;
         }
         catch (Exception ex)
         {
            Debug.LogError($"Failed to execute build: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
         }
      }

      private void CreateDefaultBuildProfile(string profilePath, BuildTarget target)
      {
         var profile = new BuildProfileData
         {
            buildTarget = target.ToString(),
            isDevelopmentBuild = false,
            connectProfiler = false,
            buildAppBundle = target == BuildTarget.Android,
            webGLMemorySize = 4096, // 4GB default for Unity 6
            webGLLinkerTarget = "Wasm"
         };

         File.WriteAllText(profilePath, JsonUtility.ToJson(profile, true));
         Debug.Log($"Created default build profile: {profilePath}");
      }

      private void ApplyBuildProfile(string profilePath)
      {
         var profile = JsonUtility.FromJson<BuildProfileData>(File.ReadAllText(profilePath));

         // Apply build settings from profile
         EditorUserBuildSettings.development = profile.isDevelopmentBuild;
         EditorUserBuildSettings.connectProfiler = profile.connectProfiler;

         if (profile.buildTarget == BuildTarget.Android.ToString())
         {
            EditorUserBuildSettings.buildAppBundle = profile.buildAppBundle;
         }
         else if (profile.buildTarget == BuildTarget.WebGL.ToString())
         {
            PlayerSettings.WebGL.memorySize = profile.webGLMemorySize;
            // Set other WebGL-specific settings
         }
      }

      private class BuildProfileData
      {
         public string buildTarget;
         public bool isDevelopmentBuild;
         public bool connectProfiler;
         public bool buildAppBundle;
         public int webGLMemorySize;
         public string webGLLinkerTarget;
      }
   }
}