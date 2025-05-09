using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Unity.IDE.Cursor
{
   /// <summary>
   /// Implements assembly management functionality for pre-Unity 6 versions,
   /// maintaining compatibility with the older assembly system.
   /// </summary>
   internal class LegacyAssemblyAdapter : IAssemblyAdapter
   {
      private const string ASSEMBLY_DEFINITION_EXTENSION = ".asmdef";
      private static readonly Regex ValidAssemblyNameRegex = new Regex(@"^[a-zA-Z][a-zA-Z0-9\._]*$");

      public void UpdateAssemblyReferences()
      {
         try
         {
            // In legacy versions, we primarily work with the default assembly
            // and manually trigger recompilation

            // Refresh the asset database to ensure all scripts are up to date
            AssetDatabase.Refresh();

            // Request script compilation
            EditorApplication.LockReloadAssemblies();
            try
            {
               // Force Unity to recompile all scripts
               CompilationPipeline.RequestScriptCompilation();
            }
            finally
            {
               EditorApplication.UnlockReloadAssemblies();
            }

            Debug.Log("Updated assembly references (Legacy mode)");
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
            // For legacy versions, we'll provide basic validation and warnings
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

            // Check for legacy-specific issues
            if (asmdef.overrideReferences)
            {
               Debug.LogWarning($"Override references setting may not work correctly in legacy Unity versions: {path}");
            }

            if (asmdef.versionDefines != null && asmdef.versionDefines.Length > 0)
            {
               Debug.LogWarning($"Version defines are not supported in legacy Unity versions: {path}");
            }

            // Basic reference validation
            if (asmdef.references != null)
            {
               foreach (string reference in asmdef.references)
               {
                  if (string.IsNullOrWhiteSpace(reference))
                  {
                     Debug.LogError($"Empty reference in {path}");
                     return false;
                  }
               }
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
               string updatedContent = ProcessLegacyAPIs(content);

               if (content != updatedContent)
               {
                  File.WriteAllText(scriptPath, updatedContent);
                  needsRecompile = true;

                  string relativePath = "Assets" + scriptPath.Substring(Application.dataPath.Length);
                  Debug.Log($"Updated legacy APIs in: {relativePath}");
               }
            }

            if (needsRecompile)
            {
               AssetDatabase.Refresh();
               EditorApplication.LockReloadAssemblies();
               try
               {
                  CompilationPipeline.RequestScriptCompilation();
               }
               finally
               {
                  EditorApplication.UnlockReloadAssemblies();
               }
            }
         }
         catch (Exception ex)
         {
            Debug.LogError($"Failed to process script updates: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
         }
      }

      private string ProcessLegacyAPIs(string content)
      {
         // Example legacy API updates (add more as needed)
         var updates = new Dictionary<string, string>
            {
                // Update UnityWebRequest to WWW for legacy compatibility
                {@"UnityWebRequest\.Get\s*\((.*?)\)", "new WWW($1)"},
                
                // Update SceneManager to Application.LoadLevel
                {@"SceneManager\.LoadScene\s*\((.*?)\)", "Application.LoadLevel($1)"},
                {@"SceneManager\.LoadSceneAsync\s*\((.*?)\)", "Application.LoadLevelAsync($1)"},
                
                // Update Input System to legacy Input
                {@"Input\.GetAxis\s*\((.*?)\)", "Input.GetAxisRaw($1)"}
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