## Pascal Compiler

Development language: C#

Developer: FEFU student Б9120-09.03.03пикд Ponomarenko Maxim

## Usage
```console
dotnet run -- <mode> [option | path]
```
Mode
| Flag | Description          |
| ---- | -------------------- |
| -l   | Run lexical analysis |
| -p   | Run parser analysis  |

Option
| Flag   | Description           |
| ------ | --------------------- |
| --test | Run alanyzer autotest |


## Example
```console
dotnet run -- -l --test
dotnet run -- -l .\tests\lexer\01_EOF.in
```