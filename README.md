# Cursor IDE Integration for Unity 6

Enhance your Unity 6 development workflow with seamless Cursor IDE integration, featuring advanced WebGL optimization tools and automated task management.

## Features

- **Unity 6 Native Integration**
  - Seamless project synchronization
  - Advanced code completion
  - Real-time error detection
  - Smart code navigation

- **WebGL Development Optimization**
  - Automated build pipeline
  - Browser testing automation
  - Performance profiling tools
  - Memory optimization helpers

- **Code Quality Tools**
  - Automated code analysis
  - Best practices enforcement
  - Unity-specific linting rules
  - Code style consistency checks

- **Task Automation**
  - Build process automation
  - Asset optimization
  - Scene validation
  - Deployment helpers

## Requirements

- Unity 6.0 or higher
- Cursor IDE (latest version)
- .NET SDK 8.0 or higher
- WebGL build support module installed

## Installation

1. Open Unity Package Manager
2. Click the + button in the top-left corner
3. Select "Add package from git URL"
4. Enter: `https://github.com/unity/com.unity.ide.cursor.git`

Or add to your `manifest.json`:

```json
{
  "dependencies": {
    "com.unity.ide.cursor": "1.0.0"
  }
}
```

## Quick Start

1. Install the package
2. Open your project in Unity 6
3. Go to Edit > Preferences > External Tools
4. Select Cursor as your external script editor
5. Restart Unity to apply changes

## Usage

### Basic Integration

```csharp
// Your scripts will automatically work with Cursor IDE
public class ExampleScript : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Cursor IDE integration active!");
    }
}
```

### WebGL Optimization

```csharp
// Use the included WebGL optimization attributes
[WebGLOptimized]
public class WebGLAwareComponent : MonoBehaviour
{
    // Your WebGL-optimized code here
}
```

## Documentation

- [User Manual](Documentation~/manual/index.md)
- [API Reference](Documentation~/api/index.md)
- [WebGL Development Guide](Documentation~/manual/webgl-development.md)
- [Best Practices](Documentation~/manual/best-practices.md)

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Support

- [Issue Tracker](https://github.com/unity/com.unity.ide.cursor/issues)
- [Forum Thread](https://forum.unity.com/threads/cursor-ide-integration.12345/)
- [Documentation](Documentation~/index.md)

## Acknowledgments

- Unity Technologies
- Cursor IDE Team
- All our contributors
