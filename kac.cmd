@echo off
rem kac — Windows launcher, at the repo root so `kac validate` works from where you usually run. Put
rem the repo root on your PATH to run it as `kac`. `exit /b` propagates the tool's exit code. The
rem POSIX twin is kac; the tool itself is tooling\kac.cs.
dotnet run "%~dp0tooling\kac.cs" -- %*
exit /b %errorlevel%
