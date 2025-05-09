using System;
using System.IO;
using NUnit.Framework;
using Unity.IDE.Cursor.Adapters;
using UnityEditor;
using UnityEngine;

namespace Unity.IDE.Cursor.Tests
{
   public class AssemblyAdapterTests
   {
      private Unity2022_3AssemblyAdapter _2022_3Adapter;
      private LegacyAssemblyAdapter _legacyAdapter;
      private string testAssemblyPath;

      [SetUp]
      public void Setup()
      {
         testAssemblyPath = Path.Combine("Assets", "Tests", "TestAssemblies");
         if (!Directory.Exists(testAssemblyPath))
         {
            Directory.CreateDirectory(testAssemblyPath);
         }

         _2022_3Adapter = new Unity2022_3AssemblyAdapter();
         _legacyAdapter = new LegacyAssemblyAdapter();
      }

      [TearDown]
      public void Cleanup()
      {
         if (Directory.Exists(testAssemblyPath))
         {
            Directory.Delete(testAssemblyPath, true);
            File.Delete(testAssemblyPath + ".meta");
            AssetDatabase.Refresh();
         }
      }

      [Test]
      public void Unity2022_3Adapter_ValidatesAssemblyDefinition_ValidatesCorrectly()
      {
         string asmdefPath = Path.Combine(testAssemblyPath, "TestAssembly.asmdef");
         string validAsmdef = @"{
                ""name"": ""TestAssembly"",
                ""references"": [
                    ""UnityEngine"",
                    ""UnityEditor""
                ],
                ""includePlatforms"": [
                    ""Editor""
                ],
                ""excludePlatforms"": [],
                ""allowUnsafeCode"": false,
                ""overrideReferences"": false
            }";

         File.WriteAllText(asmdefPath, validAsmdef);
         AssetDatabase.Refresh();

         bool isValid = _2022_3Adapter.ValidateAssemblyDefinition(asmdefPath);
         Assert.That(isValid, Is.True);
      }

      [Test]
      public void Unity2022_3Adapter_ValidatesAssemblyDefinition_DetectsInvalidName()
      {
         string asmdefPath = Path.Combine(testAssemblyPath, "TestAssembly.asmdef");
         string invalidAsmdef = @"{
                ""name"": ""1InvalidName"",
                ""references"": []
            }";

         File.WriteAllText(asmdefPath, invalidAsmdef);
         AssetDatabase.Refresh();

         bool isValid = _2022_3Adapter.ValidateAssemblyDefinition(asmdefPath);
         Assert.That(isValid, Is.False);
      }

      [Test]
      public void Unity2022_3Adapter_ValidatesAssemblyDefinition_DetectsPlatformConflicts()
      {
         string asmdefPath = Path.Combine(testAssemblyPath, "TestAssembly.asmdef");
         string conflictingAsmdef = @"{
                ""name"": ""TestAssembly"",
                ""includePlatforms"": [""Editor""],
                ""excludePlatforms"": [""Editor""]
            }";

         File.WriteAllText(asmdefPath, conflictingAsmdef);
         AssetDatabase.Refresh();

         bool isValid = _2022_3Adapter.ValidateAssemblyDefinition(asmdefPath);
         Assert.That(isValid, Is.False);
      }

      [Test]
      public void Unity2022_3Adapter_ProcessScriptUpdates_UpdatesObsoleteAPIs()
      {
         string scriptPath = Path.Combine(testAssemblyPath, "TestScript.cs");
         string oldCode = @"
                using UnityEngine;
                public class TestScript : MonoBehaviour
                {
                    void Start()
                    {
                        var www = new WWW(""http://example.com"");
                        Application.LoadLevelAsync(1);
                    }
                }";

         File.WriteAllText(scriptPath, oldCode);
         AssetDatabase.Refresh();

         _2022_3Adapter.ProcessScriptUpdates();

         string updatedContent = File.ReadAllText(scriptPath);
         StringAssert.Contains("UnityWebRequest.Get", updatedContent);
         StringAssert.Contains("SceneManager.LoadSceneAsync", updatedContent);
         StringAssert.DoesNotContain("new WWW", updatedContent);
         StringAssert.DoesNotContain("LoadLevelAsync", updatedContent);
      }

      [Test]
      public void LegacyAdapter_ProcessScriptUpdates_UpdatesModernAPIs()
      {
         string scriptPath = Path.Combine(testAssemblyPath, "TestScript.cs");
         string modernCode = @"
                using UnityEngine;
                public class TestScript : MonoBehaviour
                {
                    void Start()
                    {
                        var request = UnityWebRequest.Get(""http://example.com"");
                        SceneManager.LoadScene(1);
                    }
                }";

         File.WriteAllText(scriptPath, modernCode);
         AssetDatabase.Refresh();

         _legacyAdapter.ProcessScriptUpdates();

         string updatedContent = File.ReadAllText(scriptPath);
         StringAssert.Contains("new WWW", updatedContent);
         StringAssert.Contains("Application.LoadLevel", updatedContent);
         StringAssert.DoesNotContain("UnityWebRequest.Get", updatedContent);
         StringAssert.DoesNotContain("SceneManager.LoadScene", updatedContent);
      }

      [Test]
      public void BothAdapters_HandleMissingAssemblyDefinition()
      {
         string nonExistentPath = Path.Combine(testAssemblyPath, "NonExistent.asmdef");

         Assert.That(_2022_3Adapter.ValidateAssemblyDefinition(nonExistentPath), Is.False);
         Assert.That(_legacyAdapter.ValidateAssemblyDefinition(nonExistentPath), Is.False);
      }

      [Test]
      public void BothAdapters_HandleInvalidJson()
      {
         string asmdefPath = Path.Combine(testAssemblyPath, "Invalid.asmdef");
         string invalidJson = "{ invalid json content }";

         File.WriteAllText(asmdefPath, invalidJson);
         AssetDatabase.Refresh();

         Assert.That(_2022_3Adapter.ValidateAssemblyDefinition(asmdefPath), Is.False);
         Assert.That(_legacyAdapter.ValidateAssemblyDefinition(asmdefPath), Is.False);
      }
   }
}