## Pascal Compiler

Development language: C#

Developer: FEFU student Б9120-09.03.03пикд Ponomarenko Maxim

## Usage

```console
dotnet run -- <mode> <option>
```

Mode
| Flag                                      | Description                      |
| ----------------------------------------- | -------------------------------- |
|  -l, --lexer                              | Run lexical analysis             |
|  -p, --parser                             | Run syntax parser                |
|  -s, --semantics                          | Run parser with semantics cheker |
|  -g, --generator &#60;generator mode&#60; | Run code generator               |

Generator mode
| Flag             | Description                            |
| ---------------- | -------------------------------------- |
| -gen, --generate | Generate NASM code and stop            |
| -comp, --compile | Generate NASM code and compile it      |
| -exec, --execute | Generate NASM code, compile and run it |

Option
| Flag       | Arguments           | Description            |
| ---------- | ------------------- | ---------------------- |
| -t, --test | none                | Run alanyzer autotests |
| -f, --file | &#60;file path&#62; | Run alanyzer with file |

## Example

```console
dotnet run -- -l --test
dotnet run -- -l --file .\tests\lexer\01_EOF.in
```

## Dependencies
- NASM compiler for code compilation (nasm -> obj)
- GCC compiler for code compilation (obj -> exe)
