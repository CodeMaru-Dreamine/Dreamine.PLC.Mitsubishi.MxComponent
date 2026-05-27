# Dreamine.PLC.Mitsubishi.MxComponent

Dreamine PLC 통신을 위한 Mitsubishi MX Component 어댑터 경계 패키지입니다.

## 중요 벤더 런타임 안내

이 패키지는 Mitsubishi MX Component DLL, 설치 파일, 샘플, 라이선스가 필요한 Runtime 파일을 재배포하면 안 됩니다.

사용자는 Mitsubishi Electric의 라이선스 조건에 따라 MX Component를 별도로 설치하고 정식 라이선스를 보유해야 합니다.

이 패키지는 사용자 PC에 설치된 벤더 Runtime과 연동하기 위한 어댑터 코드만 포함할 수 있습니다.

## 현재 상태

이 패키지는 벤더 Runtime 어댑터 경계이며, 현재 시뮬레이터로 검증된 직접 프로토콜 라인에는 포함되지 않습니다.

권장 운영 경로:

- 직접 MC TCP/UDP 통신은 `Dreamine.PLC.Mitsubishi.MC`를 사용합니다.
- 프로젝트에서 명시적으로 MX Component 연동이 필요한 경우에만 이 패키지를 사용합니다.

## 라이선스

Dreamine 어댑터 코드: MIT License.

Mitsubishi MX Component: 이 패키지에 포함되지 않으며, 이 패키지의 라이선스 대상도 아닙니다.
