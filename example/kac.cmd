@echo off
rem kac — Windows launcher, at the corpus root so `kac validate` works from where you usually run. Put
rem the corpus root on your PATH to run it as `kac`. `exit /b` propagates the tool's exit code. The
rem POSIX twin is kac; the tool itself is ..\tooling\kac.cs.
dotnet run "%~dp0..\tooling\kac.cs" -- %*
exit /b %errorlevel%
