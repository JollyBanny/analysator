## Pascal Compiler

Development language: C#

Developer: FEFU student Б9120-09.03.03пикд Ponomarenko Maxim

## Usage

```console
dotnet run -- <mode> <option>
```

Mode
| Flag | Description                      |
| ---- | -------------------------------- |
|  -l  | Run lexical analysis             |
|  -sp | Run simple syntax parser         |
|  -p  | Run syntax parser                |
|  -s  | Run parser with semantics cheker |

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
