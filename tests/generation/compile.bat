@echo off

nasm -fwin32 .\tests\generation\program.asm -o .\tests\generation\program.obj
gcc -m32 .\tests\generation\program.obj -o .\tests\generation\program.exe