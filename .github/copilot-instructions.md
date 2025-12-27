# Python to C# Conversion Guidelines

- Use ILogger for logging
- Prefer c# keywords and .net primitives over custom classes
- Prefer .net framework and Microsoft nuget packages over 3rd party packages

## Migration Tracking

As you migrate Python classes to C#:

1. Check the migration checklist at the top of `docs/ClassDiagram.md`
2. Find the class you're migrating in the checklist
3. Once the C# implementation is complete and tested:
   - Change `[ ]` to `[x]` in the checklist
   - Add the C# file path after the arrow (→)
4. If the functionality is merged into another class, note it as "(merged into ClassName.cs)"
5. If functionality is split across multiple classes, list all relevant classes

Example:
```markdown
- [x] TemplateMatching (template.py, recon_utils.py) → TemplateMatching.cs
- [x] CrashHandler (reconnect/crash.py) → ArkWindow/CrashHandler.cs
```

This helps track progress and ensures no Python functionality is left unconverted.
