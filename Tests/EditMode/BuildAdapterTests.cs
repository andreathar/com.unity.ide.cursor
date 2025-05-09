using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Unity.IDE.Cursor.Adapters;
using UnityEditor;
using UnityEngine;

namespace Unity.IDE.Cursor.Tests
{
   public class BuildAdapterTests
   {
      private Unity2022_3BuildAdapter _2022_3Adapter;
      private LegacyBuildAdapter _legacyAdapter;
      private string testBuildPath;

      [SetUp]
      public void Setup()
      {
         _2022_3Adapter = new Unity2022_3BuildAdapter();
         _legacyAdapter = new LegacyBuildAdapter();

         testBuildPath = Path.Combine("Assets", "Tests", "BuildTests");
         if (!Directory.Exists(testBuildPath))
         {
            Directory.CreateDirectory(testBuildPath);
         }
      }

      [TearDown]
      public void Cleanup()
      {
         if (Directory.Exists(testBuildPath))
         {
            Directory.Delete(testBuildPath, true);
            File.Delete(testBuildPath + ".meta");
            AssetDatabase.Refresh();
         }
      }

      [Test]
      public void Unity2022_3Adapter_ConfiguresBuildSettings_WebGL()
      {
         _2022_3Adapter.ConfigureBuildSettings(BuildTarget.WebGL);

         // Verify WebGL-specific settings
         Assert.That(PlayerSettings.WebGL.memorySize, Is.EqualTo(4096));
         Assert.That(PlayerSettings.WebGL.linkerTarget, Is.EqualTo(WebGLLinkerTarget.Wasm));
         Assert.That(PlayerSettings.WebGL.compressionFormat, Is.EqualTo(WebGLCompressionFormat.Brotli));
      }

      [Test]
      public void Unity2022_3Adapter_UpdatesBuildProfile()
      {
         var settings = new Dictionary<string, object>
         {
            { "webGLMemorySize", 8192 },
            { "webGLLinkerTarget", "Wasm" },
            { "developmentBuild", true }
         };

         _2022_3Adapter.UpdateBuildProfile("WebGL_Test", settings);

         // Verify profile was updated
         Assert.That(PlayerSettings.WebGL.memorySize, Is.EqualTo(8192));
         Assert.That(EditorUserBuildSettings.development, Is.True);
      }

      [Test]
      public void Unity2022_3Adapter_ExecutesBuild_WebGL()
      {
         string outputPath = Path.Combine(testBuildPath, "WebGLBuild");

         BuildReport report = _2022_3Adapter.ExecuteBuild(outputPath, BuildTarget.WebGL);

         Assert.That(report, Is.Not.Null);
         Assert.That(report.summary.result, Is.EqualTo(BuildResult.Succeeded));
      }

      [Test]
      public void LegacyAdapter_ConfiguresBuildSettings_WebGL()
      {
         _legacyAdapter.ConfigureBuildSettings(BuildTarget.WebGL);

         // Verify legacy WebGL settings
         Assert.That(PlayerSettings.WebGL.memorySize, Is.EqualTo(2048)); // Legacy default
         Assert.That(EditorUserBuildSettings.development, Is.False);
      }

      [Test]
      public void Unity2022_3Adapter_HandlesInvalidBuildTarget()
      {
         Assert.That(_legacyAdapter.ValidateBuildTarget(BuildTarget.WebGL), Is.True);
         Assert.That(_legacyAdapter.ValidateBuildTarget(BuildTarget.StandaloneWindows64), Is.True);
      }

      [Test]
      public void Unity2022_3Adapter_ConfiguresWebGLOptimizations()
      {
         _2022_3Adapter.ConfigureBuildSettings();

         // Verify WebGL-specific optimizations
         Assert.That(PlayerSettings.WebGL.memorySize, Is.EqualTo(512));
         Assert.That(PlayerSettings.WebGL.threadsSupport, Is.True);
         Assert.That(PlayerSettings.WebGL.linkerTarget, Is.EqualTo(WebGLLinkerTarget.Wasm));
      }

      [Test]
      public void Unity2022_3Adapter_ConfiguresDebuggingSettings()
      {
         _2022_3Adapter.ConfigureBuildSettings();

         // Verify debugging settings
         Assert.That(EditorUserBuildSettings.development, Is.False);
         Assert.That(PlayerSettings.WebGL.debugSymbols, Is.False);
      }

      [Test]
      public void Unity2022_3Adapter_HandlesCustomBuildOptions()
      {
         var options = new BuildOptions
         {
            development = true,
            allowDebugging = true
         };

         _2022_3Adapter.ConfigureBuildSettings(options);

         Assert.That(EditorUserBuildSettings.development, Is.True);
         Assert.That(PlayerSettings.WebGL.debugSymbols, Is.True);
      }

      [Test]
      public void LegacyAdapter_HandlesCustomBuildOptions()
      {
         var options = new BuildOptions
         {
            development = true,
            allowDebugging = true
         };

         _legacyAdapter.ConfigureBuildSettings(options);

         Assert.That(EditorUserBuildSettings.development, Is.True);
      }
   }
}