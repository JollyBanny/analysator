@echo off

nasm -fwin32 .\tests\asm\program.asm -o .\tests\asm\program.obj
gcc -m32 .\tests\asm\program.obj -o .\tests\asm\program.exe
.\tests\asm\program.exe