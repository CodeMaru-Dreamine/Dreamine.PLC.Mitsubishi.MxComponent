# Dreamine.PLC.Mitsubishi.MxComponent

[![CI](https://github.com/CodeMaru-Dreamine/Dreamine.PLC.Mitsubishi.MxComponent/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/CodeMaru-Dreamine/Dreamine.PLC.Mitsubishi.MxComponent/actions/workflows/ci.yml)
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=CodeMaru-Dreamine_Dreamine.PLC.Mitsubishi.MxComponent&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=CodeMaru-Dreamine_Dreamine.PLC.Mitsubishi.MxComponent)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=CodeMaru-Dreamine_Dreamine.PLC.Mitsubishi.MxComponent&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=CodeMaru-Dreamine_Dreamine.PLC.Mitsubishi.MxComponent)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=CodeMaru-Dreamine_Dreamine.PLC.Mitsubishi.MxComponent&metric=coverage)](https://sonarcloud.io/summary/new_code?id=CodeMaru-Dreamine_Dreamine.PLC.Mitsubishi.MxComponent)

[![License](https://img.shields.io/badge/license-MIT-2496ED.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![NuGet](https://img.shields.io/nuget/v/Dreamine.PLC.Mitsubishi.MxComponent.svg)](https://www.nuget.org/packages/Dreamine.PLC.Mitsubishi.MxComponent)
[![Downloads](https://img.shields.io/nuget/dt/Dreamine.PLC.Mitsubishi.MxComponent.svg)](https://www.nuget.org/packages/Dreamine.PLC.Mitsubishi.MxComponent)

[![Docs](https://img.shields.io/badge/%F0%9F%93%98%20Docs-dreamine.kr-2496ED)](https://dreamine.kr/libraries?lang=en)
[![Guide](https://img.shields.io/badge/%F0%9F%93%98%20Guide-dreamine.kr-2496ED)](https://dreamine.kr/guide?lang=en)
[![Playground](https://img.shields.io/badge/%F0%9F%8E%AE%20Playground-dreamine.kr-7B2CBF)](https://dreamine.kr/playground?lang=en)
[![Book](https://img.shields.io/badge/%F0%9F%93%96%20Book-Practical%20MVVM%20Architecture-black)](https://bookk.co.kr/bookStore/69c0f1b41461ec1ae849a0f6)

[Korean documentation](./README_KO.md)

Mitsubishi MX Component adapter boundary for Dreamine PLC communication.

## Important vendor runtime notice

This package must not redistribute Mitsubishi MX Component DLLs, installers, samples, or licensed runtime files.

Users must install and license Mitsubishi MX Component separately according to Mitsubishi Electric's license terms.

This package may only contain adapter code that integrates with a user-installed vendor runtime.

## Current status

This package provides a late-bound COM adapter without redistributing or directly referencing the vendor runtime.

Main types:

- `MitsubishiMxComponentPlcClient`
- `MitsubishiMxComponentOptions`
- `MitsubishiMxDeviceNameFormatter`

The default ProgID follows the current process bitness:

- `x86`: `ActUtlType.ActUtlType`
- `x64`: `ActUtlType64.ActUtlWrap`

The adapter uses the MX Component `LogicalStationNumber` and calls `Open`/`Close`. Word block access first tries `ReadDeviceBlock2`/`WriteDeviceBlock2`; when COM late binding rejects block array arguments, it falls back to repeated `GetDevice`/`SetDevice` calls.

Sample:

- Open the `SampleSmart` PLC Monitor page and select `MxComponent`.
- Confirm `MX ProgID` and `MX LS`, then run `Use Client` -> `Connect`.
- For the standard MX Component path, run SampleSmart as `x86` and use `ActUtlType.ActUtlType`.
- The Mitsubishi `DotUtlType64` wrapper can require legacy .NET Framework WCF types; if it fails under `net8.0-windows x64`, use the `x86` path or a separate .NET Framework bridge process.

Recommended production path:

- Use `Dreamine.PLC.Mitsubishi.MC` for direct MC TCP/UDP protocol communication.
- Use this package only when a project explicitly requires MX Component integration.

## License

Dreamine adapter code: MIT License.

Mitsubishi MX Component: not included and not licensed by this package.
