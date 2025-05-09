using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Unity.IDE.Cursor
{
   /// <summary>
   /// Core compatibility layer for handling Unity version-specific functionality.
   /// Provides version detection, API abstractions, and compatibility utilities.
   /// </summary>
   public static class UnityVersionCompatibility
   {
      private static readonly Version Unity2022_3_19 = new Version(2022, 3, 19);
      private static Version? _cachedUnityVersion;

      /// <summary>
      /// Gets the current Unity version as a Version object.
      /// </summary>
      public static Version UnityVersion
      {
         get
         {
            if (_cachedUnityVersion == null)
            {
               // Parse version string, handling alpha/beta suffixes
               string versionStr = Application.unityVersion;
               Match match = Regex.Match(versionStr, @"^(\d+)\.(\d+)\.(\d+)");

               if (match.Success)
               {
                  _cachedUnityVersion = new Version(
                      int.Parse(match.Groups[1].Value),
                      int.Parse(match.Groups[2].Value),
                      int.Parse(match.Groups[3].Value)
                  );
               }
               else
               {
                  Debug.LogError($"Failed to parse Unity version: {versionStr}");
                  _cachedUnityVersion = new Version(0, 0, 0);
               }
            }

            return _cachedUnityVersion;
         }
      }

      /// <summary>
      /// Checks if the current Unity version is 2022.3.19f or newer.
      /// </summary>
      public static bool IsUnity2022_3_19OrNewer() => UnityVersion >= Unity2022_3_19;

      /// <summary>
      /// Executes the appropriate method based on Unity version.
      /// </summary>
      /// <param name="unity2022_3Method">Method to execute for Unity 2022.3.19f+</param>
      /// <param name="fallbackMethod">Method to execute for older Unity versions</param>
      public static void ExecuteCompatibleMethod(Action unity2022_3Method, Action fallbackMethod)
      {
         try
         {
            if (IsUnity2022_3_19OrNewer())
               unity2022_3Method();
            else
               fallbackMethod();
         }
         catch (Exception ex)
         {
            Debug.LogError($"Error in compatibility layer: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
         }
      }

      /// <summary>
      /// Executes the appropriate function based on Unity version and returns the result.
      /// </summary>
      public static T ExecuteCompatibleFunction<T>(Func<T> unity2022_3Function, Func<T> fallbackFunction)
      {
         try
         {
            return IsUnity2022_3_19OrNewer() ? unity2022_3Function() : fallbackFunction();
         }
         catch (Exception ex)
         {
            Debug.LogError($"Error in compatibility layer: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
         }
      }
   }

   /// <summary>
   /// Interface for version-specific script creation implementations.
   /// </summary>
   public interface IScriptCreationAdapter
   {
      void CreateMonoBehaviourScript(string path, string className);
      void CreateScriptableObjectScript(string path, string className);
      void CreateBlankScript(string path, string className);
   }

   /// <summary>
   /// Interface for version-specific assembly management implementations.
   /// </summary>
   public interface IAssemblyAdapter
   {
      void UpdateAssemblyReferences();
      bool ValidateAssemblyDefinition(string path);
      void ProcessScriptUpdates();
   }

   /// <summary>
   /// Interface for version-specific build system implementations.
   /// </summary>
   public interface IBuildAdapter
   {
      void ConfigureBuildSettings();
      void UpdateBuildProfile(string profileName);
      string[] GetAvailableBuildProfiles();
   }

   /// <summary>
   /// Factory for creating version-specific adapters.
   /// </summary>
   public static class CompatibilityAdapterFactory
   {
      public static IScriptCreationAdapter CreateScriptAdapter()
      {
         return UnityVersionCompatibility.ExecuteCompatibleFunction(
             () => new Unity2022_3ScriptCreationAdapter(),
             () => new LegacyScriptCreationAdapter()
         );
      }

      public static IAssemblyAdapter CreateAssemblyAdapter()
      {
         return UnityVersionCompatibility.ExecuteCompatibleFunction(
             () => new Unity2022_3AssemblyAdapter(),
             () => new LegacyAssemblyAdapter()
         );
      }

      public static IBuildAdapter CreateBuildSystemAdapter()
      {
         return UnityVersionCompatibility.ExecuteCompatibleFunction(
             () => new Unity2022_3BuildAdapter(),
             () => new LegacyBuildAdapter()
         );
      }
   }

   // Placeholder adapter implementations - these will be implemented in separate files
   internal class Unity6ScriptAdapter : IScriptCreationAdapter
   {
      public void CreateMonoBehaviourScript(string path, string className) => throw new NotImplementedException();
      public void CreateScriptableObjectScript(string path, string className) => throw new NotImplementedException();
      public void CreateBlankScript(string path, string className) => throw new NotImplementedException();
   }

   internal class LegacyScriptAdapter : IScriptCreationAdapter
   {
      public void CreateMonoBehaviourScript(string path, string className) => throw new NotImplementedException();
      public void CreateScriptableObjectScript(string path, string className) => throw new NotImplementedException();
      public void CreateBlankScript(string path, string className) => throw new NotImplementedException();
   }

   internal class Unity6AssemblyAdapter : IAssemblyAdapter
   {
      public void UpdateAssemblyReferences() => throw new NotImplementedException();
      public bool ValidateAssemblyDefinition(string path) => throw new NotImplementedException();
      public void ProcessScriptUpdates() => throw new NotImplementedException();
   }

   internal class LegacyAssemblyAdapter : IAssemblyAdapter
   {
      public void UpdateAssemblyReferences() => throw new NotImplementedException();
      public bool ValidateAssemblyDefinition(string path) => throw new NotImplementedException();
      public void ProcessScriptUpdates() => throw new NotImplementedException();
   }

   internal class Unity6BuildSystemAdapter : IBuildSystemAdapter
   {
      public void ConfigureBuildSettings() => throw new NotImplementedException();
      public void UpdateBuildProfile(string profileName) => throw new NotImplementedException();
      public string[] GetAvailableBuildProfiles() => throw new NotImplementedException();
   }

   internal class LegacyBuildSystemAdapter : IBuildSystemAdapter
   {
      public void ConfigureBuildSettings() => throw new NotImplementedException();
      public void UpdateBuildProfile(string profileName) => throw new NotImplementedException();
      public string[] GetAvailableBuildProfiles() => throw new NotImplementedException();
   }
}