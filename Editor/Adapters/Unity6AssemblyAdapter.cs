using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Unity.IDE.Cursor.Adapters
{
   /// <summary>
   /// Assembly management adapter for Unity 2022.3 LTS.
   /// Handles assembly definition validation, platform checks, and API updates.
   /// </summary>
   public class Unity2022_3AssemblyAdapter : IAssemblyAdapter
   {
      private const string ASSEMBLY_DEFINITION_EXTENSION = ".asmdef";
      private static readonly Regex ValidAssemblyNameRegex = new Regex(@"^[a-zA-Z][a-zA-Z0-9\._]*$");

      public void UpdateAssemblyReferences()
      {
         try
         {
            // Get all assembly definition files in the project
            string[] asmdefPaths = Directory.GetFiles(
                Application.dataPath,
                $"*{ASSEMBLY_DEFINITION_EXTENSION}",
                SearchOption.AllDirectories
            );

            foreach (string asmdefPath in asmdefPaths)
            {
               if (!ValidateAssemblyDefinition(asmdefPath))
               {
                  Debug.LogError($"Invalid assembly definition file: {asmdefPath}");
                  continue;
               }

               // Force Unity to recompile this assembly
               string relativePath = "Assets" + asmdefPath.Substring(Application.dataPath.Length);
               AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
            }

            // Trigger a full recompile to ensure all references are updated
            CompilationPipeline.RequestScriptCompilation();
         }
         catch (Exception ex)
         {
            Debug.LogError($"Failed to update assembly references: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
         }
      }

      public bool ValidateAssemblyDefinition(string path)
      {
         try
         {
            if (!File.Exists(path))
            {
               Debug.LogError($"Assembly definition file not found: {path}");
               return false;
            }

            string json = File.ReadAllText(path);
            var asmdef = JsonUtility.FromJson<AssemblyDefinitionData>(json);

            // Validate assembly name
            if (string.IsNullOrWhiteSpace(asmdef.name) || !ValidAssemblyNameRegex.IsMatch(asmdef.name))
            {
               Debug.LogError($"Invalid assembly name in {path}: {asmdef.name}");
               return false;
            }

            // Validate references
            if (asmdef.references != null)
            {
               foreach (string reference in asmdef.references)
               {
                  if (string.IsNullOrWhiteSpace(reference))
                  {
                     Debug.LogError($"Empty reference in {path}");
                     return false;
                  }

                  // Check if the referenced assembly exists
                  string[] existingAssemblies = CompilationPipeline.GetAssemblies()
                      .Select(a => a.name)
                      .ToArray();

                  if (!existingAssemblies.Contains(reference))
                  {
                     Debug.LogWarning($"Referenced assembly not found: {reference} in {path}");
                  }
               }
            }

            // Validate platform includes/excludes
            if (asmdef.includePlatforms != null && asmdef.excludePlatforms != null &&
                asmdef.includePlatforms.Intersect(asmdef.excludePlatforms).Any())
            {
               Debug.LogError($"Platform conflict in {path}: Same platform in both include and exclude lists");
               return false;
            }

            return true;
         }
         catch (Exception ex)
         {
            Debug.LogError($"Failed to validate assembly definition: {ex.Message}\nStackTrace: {ex.StackTrace}");
            return false;
         }
      }

      public void ProcessScriptUpdates()
      {
         try
         {
            // Get all script files in the project
            string[] scriptPaths = Directory.GetFiles(
                Application.dataPath,
                "*.cs",
                SearchOption.AllDirectories
            );

            bool needsRecompile = false;

            foreach (string scriptPath in scriptPaths)
            {
               string content = File.ReadAllText(scriptPath);
               string updatedContent = ProcessObsoleteAPIs(content);

               if (content != updatedContent)
               {
                  File.WriteAllText(scriptPath, updatedContent);
                  needsRecompile = true;

                  string relativePath = "Assets" + scriptPath.Substring(Application.dataPath.Length);
                  Debug.Log($"Updated obsolete APIs in: {relativePath}");
               }
            }

            if (needsRecompile)
            {
               AssetDatabase.Refresh();
               CompilationPipeline.RequestScriptCompilation();
            }
         }
         catch (Exception ex)
         {
            Debug.LogError($"Failed to process script updates: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
         }
      }

      private string ProcessObsoleteAPIs(string content)
      {
         // Example API updates (add more as needed)
         var updates = new Dictionary<string, string>
            {
                // Update WWW to UnityWebRequest
                {@"new\s+WWW\s*\((.*?)\)", "UnityWebRequest.Get($1)"},
                
                // Update Application.LoadLevelAsync to SceneManager
                {@"Application\.LoadLevelAsync\s*\((.*?)\)", "SceneManager.LoadSceneAsync($1)"},
                
                // Update GUI.HorizontalSlider to EditorGUILayout.Slider for Editor scripts
                {@"GUI\.HorizontalSlider\s*\((.*?)\)", "EditorGUILayout.Slider($1)"}
            };

         string updatedContent = content;
         foreach (var update in updates)
         {
            updatedContent = Regex.Replace(updatedContent, update.Key, update.Value);
         }

         return updatedContent;
      }

      private class AssemblyDefinitionData
      {
         public string name;
         public string[] references;
         public string[] includePlatforms;
         public string[] excludePlatforms;
         public bool allowUnsafeCode;
         public bool overrideReferences;
         public string[] precompiledReferences;
         public bool autoReferenced;
         public string[] defineConstraints;
         public string[] versionDefines;
         public bool noEngineReferences;
      }
   }
}