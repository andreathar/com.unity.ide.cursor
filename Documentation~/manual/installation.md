# Installation Guide

This guide will help you install and set up the Cursor IDE Integration package for Unity 6.

## Prerequisites

Before installing, ensure you have:

- Unity 6.0 or higher installed
- Cursor IDE (latest version)
- .NET SDK 8.0 or higher
- WebGL build support module installed in Unity

## Installation Methods

### Method 1: Unity Package Manager (Recommended)

1. Open Unity Hub and launch your Unity 6 project
2. Go to Window > Package Manager
3. Click the + button in the top-left corner
4. Select "Add package from git URL"
5. Enter: `https://github.com/unity/com.unity.ide.cursor.git`
6. Click Add
7. Wait for Unity to download and import the package

### Method 2: Manual Installation (manifest.json)

1. Open your project's `Packages/manifest.json` file
2. Add the following line to the dependencies section:

   ```json
   {
     "dependencies": {
       "com.unity.ide.cursor": "1.0.0"
     }
   }
   ```

3. Save the file and return to Unity
4. Wait for Unity to download and import the package

## Post-Installation Setup

1. Open Unity Editor
2. Go to Edit > Preferences > External Tools
3. In the "External Script Editor" dropdown, select "Cursor"
4. If prompted, click "Regenerate project files"
5. Restart Unity to apply changes

## Verifying Installation

To verify the installation:

1. Open any C# script in your project
2. It should open in Cursor IDE
3. Check the Unity Console for the message "Cursor IDE integration active!"
4. Try creating a new script - it should use the Cursor IDE template

## WebGL Setup

For WebGL development:

1. Go to File > Build Settings
2. Switch platform to WebGL if not already selected
3. Click "Install with Unity Hub" if WebGL module is not installed
4. Import WebGL templates via Package Manager if needed
5. Configure WebGL player settings (Edit > Project Settings > Player)

## Troubleshooting

If you encounter issues:

1. Check Unity Console for error messages
2. Verify Cursor IDE is properly installed
3. Ensure all prerequisites are met
4. Try regenerating project files
5. Check [Common Issues](troubleshooting/common-issues.md) in our documentation

## Next Steps

- Read the [Quick Start Guide](quick-start.md)
- Explore [WebGL Development](webgl/overview.md)
- Review [Best Practices](best-practices/code-organization.md)
- Check [Configuration Options](configuration.md)

## Support

If you need help:

- Visit our [Issue Tracker](https://github.com/unity/com.unity.ide.cursor/issues)
- Check the [Unity Forum Thread](https://forum.unity.com/threads/cursor-ide-integration.12345/)
- Read our [Troubleshooting Guide](troubleshooting/common-issues.md)
