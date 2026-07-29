# Dreamine.PLC.Mitsubishi.MxComponent

[![CI](https://github.com/CodeMaru-Dreamine/Dreamine.PLC.Mitsubishi.MxComponent/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/CodeMaru-Dreamine/Dreamine.PLC.Mitsubishi.MxComponent/actions/workflows/ci.yml)
[![품질 게이트](https://sonarcloud.io/api/project_badges/measure?project=CodeMaru-Dreamine_Dreamine.PLC.Mitsubishi.MxComponent&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=CodeMaru-Dreamine_Dreamine.PLC.Mitsubishi.MxComponent)
[![보안 등급](https://sonarcloud.io/api/project_badges/measure?project=CodeMaru-Dreamine_Dreamine.PLC.Mitsubishi.MxComponent&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=CodeMaru-Dreamine_Dreamine.PLC.Mitsubishi.MxComponent)
[![테스트 커버리지](https://sonarcloud.io/api/project_badges/measure?project=CodeMaru-Dreamine_Dreamine.PLC.Mitsubishi.MxComponent&metric=coverage)](https://sonarcloud.io/summary/new_code?id=CodeMaru-Dreamine_Dreamine.PLC.Mitsubishi.MxComponent)

[![라이선스](https://img.shields.io/badge/license-MIT-2496ED.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![NuGet](https://img.shields.io/nuget/v/Dreamine.PLC.Mitsubishi.MxComponent.svg)](https://www.nuget.org/packages/Dreamine.PLC.Mitsubishi.MxComponent)
[![다운로드](https://img.shields.io/nuget/dt/Dreamine.PLC.Mitsubishi.MxComponent.svg)](https://www.nuget.org/packages/Dreamine.PLC.Mitsubishi.MxComponent)

[![API 문서](https://img.shields.io/badge/%F0%9F%93%98%20API%20%EB%AC%B8%EC%84%9C-dreamine.kr-2496ED)](https://dreamine.kr/libraries?lang=ko)
[![가이드](https://img.shields.io/badge/%F0%9F%93%98%20%EA%B0%80%EC%9D%B4%EB%93%9C-dreamine.kr-2496ED)](https://dreamine.kr/guide?lang=ko)
[![플레이그라운드](https://img.shields.io/badge/%F0%9F%8E%AE%20%ED%94%8C%EB%A0%88%EC%9D%B4%EA%B7%B8%EB%9D%BC%EC%9A%B4%EB%93%9C-dreamine.kr-7B2CBF)](https://dreamine.kr/playground?lang=ko)
[![책](https://img.shields.io/badge/%F0%9F%93%96%20%EC%B1%85-%EC%8B%A4%EC%A0%84%20MVVM%20%EC%95%84%ED%82%A4%ED%85%8D%EC%B2%98-black)](https://bookk.co.kr/bookStore/69c0f1b41461ec1ae849a0f6)

[English documentation](./README.md)

Dreamine PLC 통신을 위한 Mitsubishi MX Component 어댑터 경계 패키지입니다.

## 중요 벤더 런타임 안내

이 패키지는 Mitsubishi MX Component DLL, 설치 파일, 샘플, 라이선스가 필요한 Runtime 파일을 재배포하면 안 됩니다.

사용자는 Mitsubishi Electric의 라이선스 조건에 따라 MX Component를 별도로 설치하고 정식 라이선스를 보유해야 합니다.

이 패키지는 사용자 PC에 설치된 벤더 Runtime과 연동하기 위한 어댑터 코드만 포함할 수 있습니다.

## 현재 상태

이 패키지는 벤더 Runtime을 직접 참조하지 않는 late-bound COM 어댑터를 제공합니다.

주요 클래스:

- `MitsubishiMxComponentPlcClient`
- `MitsubishiMxComponentOptions`
- `MitsubishiMxDeviceNameFormatter`

기본 ProgID는 현재 프로세스 비트 수에 따라 달라집니다.

- `x86`: `ActUtlType.ActUtlType`
- `x64`: `ActUtlType64.ActUtlWrap`

어댑터는 MX Component에서 설정한 `LogicalStationNumber`로 `Open`/`Close`를 호출합니다. Word 블록 접근은 먼저 `ReadDeviceBlock2`/`WriteDeviceBlock2`를 시도하고, COM late-binding에서 배열 인자가 거부되면 `GetDevice`/`SetDevice`를 반복 호출하는 방식으로 fallback합니다.

샘플:

- `SampleSmart`의 PLC Monitor 화면에서 `MxComponent` 모드를 선택합니다.
- `MX ProgID`, `MX LS` 값을 확인한 뒤 `Use Client` -> `Connect` 순서로 실행합니다.
- MX Component 정석 경로는 SampleSmart를 `x86`으로 실행하고 `ActUtlType.ActUtlType`를 사용합니다.
- Mitsubishi `DotUtlType64` wrapper는 구형 .NET Framework WCF 타입을 요구할 수 있습니다. `net8.0-windows x64`에서 실패하면 `x86` 경로를 사용하거나 별도 .NET Framework 브리지 프로세스로 중계합니다.

권장 운영 경로:

- 직접 MC TCP/UDP 통신은 `Dreamine.PLC.Mitsubishi.MC`를 사용합니다.
- 프로젝트에서 명시적으로 MX Component 연동이 필요한 경우에만 이 패키지를 사용합니다.

## 라이선스

Dreamine 어댑터 코드: MIT License.

Mitsubishi MX Component: 이 패키지에 포함되지 않으며, 이 패키지의 라이선스 대상도 아닙니다.
