GLOBAL _main
EXTERN _printf
EXTERN _scanf
EXTERN _malloc
SECTION .bss
SECTION .text
_main:
PUSH const_val_1
PUSH const_val_2
CALL _printf
ADD ESP, 8
PUSH const_val_3
PUSH const_val_4
CALL _printf
ADD ESP, 8
PUSH const_val_5
PUSH const_val_6
CALL _printf
ADD ESP, 8
MOV EAX, 0
RET 
SECTION .data
double_minus: DQ -1.000000000
const_val_1: DB "Hello, ", 0
const_val_2: DB "%s", 0
const_val_3: DB "world!", 0
const_val_4: DB "%s",10,  0
const_val_5: DB "Hello, world!", 0
const_val_6: DB "%s", 0
