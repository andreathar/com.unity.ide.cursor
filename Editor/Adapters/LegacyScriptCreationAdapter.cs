using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Unity.IDE.Cursor
{
   /// <summary>
   /// Implements script creation functionality for pre-Unity 6 versions,
   /// maintaining compatibility with the older unified script creation system.
   /// </summary>
   internal class LegacyScriptAdapter : IScriptCreationAdapter
   {
      private const string LEGACY_SCRIPT_TEMPLATE = @"using UnityEngine;
using System.Collections;

public class #SCRIPTNAME# : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        #NOTRIM#
    }
    
    // Update is called once per frame
    void Update()
    {
        #NOTRIM#
    }
}";

      private const string LEGACY_SCRIPTABLE_OBJECT_TEMPLATE = @"using UnityEngine;
using System.Collections;

public class #SCRIPTNAME# : ScriptableObject
{
    void OnEnable()
    {
        #NOTRIM#
    }
}";

      public void CreateMonoBehaviourScript(string path, string className)
      {
         try
         {
            ValidateScriptName(className);

            string fullPath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
            {
               Directory.CreateDirectory(directory);
            }

            // Use the legacy template which includes System.Collections by default
            string scriptContent = LEGACY_SCRIPT_TEMPLATE
                .Replace("#SCRIPTNAME#", className)
                .Replace("#NOTRIM#", "");

            File.WriteAllText(fullPath, scriptContent, Encoding.UTF8);
            AssetDatabase.Refresh();

            Debug.Log($"Created legacy MonoBehaviour script: {fullPath}");
         }
         catch (Exception ex)
         {
            Debug.LogError($"Failed to create legacy MonoBehaviour script: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
         }
      }

      public void CreateScriptableObjectScript(string path, string className)
      {
         try
         {
            ValidateScriptName(className);

            string fullPath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
            {
               Directory.CreateDirectory(directory);
            }

            string scriptContent = LEGACY_SCRIPTABLE_OBJECT_TEMPLATE
                .Replace("#SCRIPTNAME#", className)
                .Replace("#NOTRIM#", "");

            File.WriteAllText(fullPath, scriptContent, Encoding.UTF8);
            AssetDatabase.Refresh();

            Debug.Log($"Created legacy ScriptableObject script: {fullPath}");
         }
         catch (Exception ex)
         {
            Debug.LogError($"Failed to create legacy ScriptableObject script: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
         }
      }

      public void CreateBlankScript(string path, string className)
      {
         try
         {
            ValidateScriptName(className);

            string fullPath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
            {
               Directory.CreateDirectory(directory);
            }

            // For legacy versions, we'll create a basic MonoBehaviour since there's no blank script concept
            Debug.LogWarning("Creating MonoBehaviour script instead of blank script for legacy Unity version");
            CreateMonoBehaviourScript(path, className);
         }
         catch (Exception ex)
         {
            Debug.LogError($"Failed to create legacy blank script: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
         }
      }

      private void ValidateScriptName(string className)
      {
         if (string.IsNullOrWhiteSpace(className))
            throw new ArgumentException("Script name cannot be empty or whitespace.", nameof(className));

         if (!char.IsLetter(className[0]) && className[0] != '_')
            throw new ArgumentException("Script name must start with a letter or underscore.", nameof(className));

         foreach (char c in className)
         {
            if (!char.IsLetterOrDigit(c) && c != '_')
               throw new ArgumentException("Script name can only contain letters, numbers, and underscores.", nameof(className));
         }
      }
   }
}