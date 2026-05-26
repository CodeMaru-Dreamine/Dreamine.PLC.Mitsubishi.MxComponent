# Dreamine.PLC.Mitsubishi.MxComponent

This package provides a Mitsubishi MX Component adapter boundary for the Dreamine PLC communication stack.

## Purpose

`Dreamine.PLC.Mitsubishi.MxComponent` is part of the Dreamine PLC package family.

The package is designed to keep PLC communication code separated by responsibility:

- Abstractions define contracts.
- Core provides shared runtime infrastructure.
- Vendor adapters implement device-specific communication.
- WPF provides monitoring and diagnostic UI components.

## Features

- MX Component adapter boundary
- Vendor-specific connection option model
- Dreamine.PLC.Core integration point
- PLC read/write operation adapter structure
- Isolation of vendor dependency from application layers


## Project References

- `Dreamine.PLC.Abstractions`
- `Dreamine.PLC.Core`

## Target Framework

```xml
<TargetFramework>net8.0</TargetFramework>
```

## Package Metadata

| Item | Value |
|---|---|
| PackageId | `Dreamine.PLC.Mitsubishi.MxComponent` |
| Version | `1.0.0` |
| License | `MIT` |
| Repository | `https://github.com/CodeMaru-Dreamine/Dreamine.PLC.Mitsubishi.MxComponent` |
| Project URL | `https://github.com/CodeMaru-Dreamine/Dreamine.PLC.FullKit` |

## Architecture Rule

This repository must not reference application-level projects.

Dependency direction must remain one-way:

```text
Abstractions
    ▲
    │
Core
    ▲
    │
Vendor Adapter / WPF UI Component
```

## License

This project is licensed under the MIT License.
