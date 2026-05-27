# Dreamine.PLC.Mitsubishi.MxComponent

Mitsubishi MX Component adapter boundary for Dreamine PLC communication.

## Important vendor runtime notice

This package must not redistribute Mitsubishi MX Component DLLs, installers, samples, or licensed runtime files.

Users must install and license Mitsubishi MX Component separately according to Mitsubishi Electric's license terms.

This package may only contain adapter code that integrates with a user-installed vendor runtime.

## Current status

This package is a vendor runtime adapter boundary and is not part of the current simulator-validated protocol line.

Recommended production path:

- Use `Dreamine.PLC.Mitsubishi.MC` for direct MC TCP/UDP protocol communication.
- Use this package only when a project explicitly requires MX Component integration.

## License

Dreamine adapter code: MIT License.

Mitsubishi MX Component: not included and not licensed by this package.
