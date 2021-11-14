@echo off
:A
tools\spasm -E -T ascent.asm bin\ascent.8xp
Pause
Goto A