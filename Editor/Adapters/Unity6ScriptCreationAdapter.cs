using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Unity.IDE.Cursor.Adapters
{
   /// <summary>
   /// Script creation adapter for Unity 2022.3 LTS.
   /// Handles creation of new script files with appropriate templates and settings.
   /// </summary>
   public class Unity2022_3ScriptCreationAdapter : IScriptCreationAdapter
   {
      private const string MONOBEHAVIOUR_TEMPLATE = @"using UnityEngine;

public class #SCRIPTNAME# : MonoBehaviour
{
    void Start()
    {
        #NOTRIM#
    }

    void Update()
    {
        #NOTRIM#
    }
}";

      private const string SCRIPTABLE_OBJECT_TEMPLATE = @"using UnityEngine;

[CreateAssetMenu(fileName = ""#SCRIPTNAME#"", menuName = ""ScriptableObjects/#SCRIPTNAME#"")]
public class #SCRIPTNAME# : ScriptableObject
{
    #NOTRIM#
}";

      private const string BLANK_TEMPLATE = @"using UnityEngine;

public class #SCRIPTNAME#
{
    #NOTRIM#
}";

      public void CreateMonoBehaviourScript(string path, string className)
      {
         try
         {
            string fullPath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
            {
               Directory.CreateDirectory(directory);
            }

            string scriptContent = MONOBEHAVIOUR_TEMPLATE
                .Replace("#SCRIPTNAME#", className)
                .Replace("#NOTRIM#", "");

            File.WriteAllText(fullPath, scriptContent, Encoding.UTF8);
            AssetDatabase.Refresh();

            Debug.Log($"Created MonoBehaviour script: {fullPath}");
         }
         catch (Exception ex)
         {
            Debug.LogError($"Failed to create MonoBehaviour script: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
         }
      }

      public void CreateScriptableObjectScript(string path, string className)
      {
         try
         {
            string fullPath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
            {
               Directory.CreateDirectory(directory);
            }

            string scriptContent = SCRIPTABLE_OBJECT_TEMPLATE
                .Replace("#SCRIPTNAME#", className)
                .Replace("#NOTRIM#", "");

            File.WriteAllText(fullPath, scriptContent, Encoding.UTF8);
            AssetDatabase.Refresh();

            Debug.Log($"Created ScriptableObject script: {fullPath}");
         }
         catch (Exception ex)
         {
            Debug.LogError($"Failed to create ScriptableObject script: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
         }
      }

      public void CreateBlankScript(string path, string className)
      {
         try
         {
            string fullPath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
            {
               Directory.CreateDirectory(directory);
            }

            string scriptContent = BLANK_TEMPLATE
                .Replace("#SCRIPTNAME#", className)
                .Replace("#NOTRIM#", "");

            File.WriteAllText(fullPath, scriptContent, Encoding.UTF8);
            AssetDatabase.Refresh();

            Debug.Log($"Created blank script: {fullPath}");
         }
         catch (Exception ex)
         {
            Debug.LogError($"Failed to create blank script: {ex.Message}\nStackTrace: {ex.StackTrace}");
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