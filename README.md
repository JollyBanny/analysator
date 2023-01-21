## Pascal Compiler

Development language: C#

Developer: FEFU student Б9120-09.03.03пикд Ponomarenko Maxim

## Usage

```console
dotnet run -- <mode> <option>
```

Mode
| Flag              | Description                      |
| ----------------- | -------------------------------- |
|  -l, --lexer      | Run lexical analysis             |
|  -p, --parser     | Run syntax parser                |
|  -s, --semantics  | Run parser with semantics cheker |
|  -g, --generator  | Run code generator               |

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
