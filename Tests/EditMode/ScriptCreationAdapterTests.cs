using System;
using System.IO;
using NUnit.Framework;
using Unity.IDE.Cursor.Adapters;
using UnityEditor;
using UnityEngine;

namespace Unity.IDE.Cursor.Tests
{
   public class ScriptCreationAdapterTests
   {
      private string testScriptsPath;
      private Unity2022_3ScriptCreationAdapter _2022_3Adapter;
      private LegacyScriptCreationAdapter _legacyAdapter;

      [SetUp]
      public void Setup()
      {
         testScriptsPath = Path.Combine("Assets", "Tests", "TestScripts");
         if (!Directory.Exists(testScriptsPath))
         {
            Directory.CreateDirectory(testScriptsPath);
         }

         _2022_3Adapter = new Unity2022_3ScriptCreationAdapter();
         _legacyAdapter = new LegacyScriptCreationAdapter();
      }

      [TearDown]
      public void Cleanup()
      {
         if (Directory.Exists(testScriptsPath))
         {
            Directory.Delete(testScriptsPath, true);
            File.Delete(testScriptsPath + ".meta");
            AssetDatabase.Refresh();
         }
      }

      [Test]
      public void Unity2022_3Adapter_CreateMonoBehaviourScript_CreatesValidScript()
      {
         string scriptPath = Path.Combine(testScriptsPath, "TestMonoBehaviour.cs");
         _2022_3Adapter.CreateMonoBehaviourScript(scriptPath, "TestMonoBehaviour");

         Assert.That(File.Exists(scriptPath), Is.True);
         string content = File.ReadAllText(scriptPath);

         StringAssert.Contains("public class TestMonoBehaviour : MonoBehaviour", content);
         StringAssert.Contains("void Start()", content);
         StringAssert.Contains("void Update()", content);
         StringAssert.DoesNotContain("System.Collections", content);
      }

      [Test]
      public void Unity2022_3Adapter_CreateScriptableObjectScript_CreatesValidScript()
      {
         string scriptPath = Path.Combine(testScriptsPath, "TestScriptableObject.cs");
         _2022_3Adapter.CreateScriptableObjectScript(scriptPath, "TestScriptableObject");

         Assert.That(File.Exists(scriptPath), Is.True);
         string content = File.ReadAllText(scriptPath);

         StringAssert.Contains("public class TestScriptableObject : ScriptableObject", content);
         StringAssert.Contains("[CreateAssetMenu", content);
         StringAssert.DoesNotContain("System.Collections", content);
      }

      [Test]
      public void Unity2022_3Adapter_CreateBlankScript_CreatesValidScript()
      {
         string scriptPath = Path.Combine(testScriptsPath, "TestBlankScript.cs");
         _2022_3Adapter.CreateBlankScript(scriptPath, "TestBlankScript");

         Assert.That(File.Exists(scriptPath), Is.True);
         string content = File.ReadAllText(scriptPath);

         StringAssert.Contains("public class TestBlankScript", content);
         StringAssert.DoesNotContain("MonoBehaviour", content);
         StringAssert.DoesNotContain("ScriptableObject", content);
      }

      [Test]
      public void LegacyAdapter_CreateMonoBehaviourScript_CreatesValidScript()
      {
         string scriptPath = Path.Combine(testScriptsPath, "TestLegacyMonoBehaviour.cs");
         _legacyAdapter.CreateMonoBehaviourScript(scriptPath, "TestLegacyMonoBehaviour");

         Assert.That(File.Exists(scriptPath), Is.True);
         string content = File.ReadAllText(scriptPath);

         StringAssert.Contains("public class TestLegacyMonoBehaviour : MonoBehaviour", content);
         StringAssert.Contains("System.Collections", content);
         StringAssert.Contains("// Use this for initialization", content);
         StringAssert.Contains("// Update is called once per frame", content);
      }

      [Test]
      public void LegacyAdapter_CreateScriptableObjectScript_CreatesValidScript()
      {
         string scriptPath = Path.Combine(testScriptsPath, "TestLegacyScriptableObject.cs");
         _legacyAdapter.CreateScriptableObjectScript(scriptPath, "TestLegacyScriptableObject");

         Assert.That(File.Exists(scriptPath), Is.True);
         string content = File.ReadAllText(scriptPath);

         StringAssert.Contains("public class TestLegacyScriptableObject : ScriptableObject", content);
         StringAssert.Contains("System.Collections", content);
         StringAssert.Contains("OnEnable", content);
      }

      [Test]
      public void LegacyAdapter_CreateBlankScript_CreatesMonoBehaviourScript()
      {
         string scriptPath = Path.Combine(testScriptsPath, "TestLegacyBlankScript.cs");
         _legacyAdapter.CreateBlankScript(scriptPath, "TestLegacyBlankScript");

         Assert.That(File.Exists(scriptPath), Is.True);
         string content = File.ReadAllText(scriptPath);

         // Legacy adapter should create a MonoBehaviour for blank scripts
         StringAssert.Contains("public class TestLegacyBlankScript : MonoBehaviour", content);
         StringAssert.Contains("System.Collections", content);
      }

      [Test]
      public void BothAdapters_InvalidScriptName_ThrowsArgumentException()
      {
         string scriptPath = Path.Combine(testScriptsPath, "Test.cs");

         // Test empty name
         Assert.Throws<ArgumentException>(() => _2022_3Adapter.CreateBlankScript(scriptPath, ""));
         Assert.Throws<ArgumentException>(() => _legacyAdapter.CreateBlankScript(scriptPath, ""));

         // Test invalid starting character
         Assert.Throws<ArgumentException>(() => _2022_3Adapter.CreateBlankScript(scriptPath, "1InvalidName"));
         Assert.Throws<ArgumentException>(() => _legacyAdapter.CreateBlankScript(scriptPath, "1InvalidName"));

         // Test invalid characters
         Assert.Throws<ArgumentException>(() => _2022_3Adapter.CreateBlankScript(scriptPath, "Invalid-Name"));
         Assert.Throws<ArgumentException>(() => _legacyAdapter.CreateBlankScript(scriptPath, "Invalid-Name"));
      }

      [Test]
      public void BothAdapters_InvalidPath_ThrowsException()
      {
         // Test invalid characters in path
         string invalidPath = Path.Combine(testScriptsPath, "Invalid<>Path", "Test.cs");

         Assert.Throws<Exception>(() => _2022_3Adapter.CreateBlankScript(invalidPath, "Test"));
         Assert.Throws<Exception>(() => _legacyAdapter.CreateBlankScript(invalidPath, "Test"));
      }
   }
}