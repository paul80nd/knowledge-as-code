@echo off
rem breadcrumb — the SessionStart hook, Windows half. Prints the file `kac bundle` rendered beside it
rem and does nothing else. `%~dp0` is this file's own directory and already ends in a separator, so
rem nothing is assumed about the working directory the hook was run from. The POSIX twin is
rem breadcrumb, and hooks\hooks.json is what reaches either.
type "%~dp0breadcrumb.txt"
