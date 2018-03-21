# Interpreter

- Course: Compilers https://courses.helsinki.fi/fi/csm14204/119559595
- Studet: Risto Mikkola
- Project: Interpreter
- Date of delivery: ??

## Architecture
- Describe the overall architecture of your language processor with, e.g., UML diagrams.
- Explain your diagrams.
- Tell about possible shortcomings of your program (if well documented they might be partly forgiven).

## Testing
- Clearly describe your testing, and the design of test data.
- Tell about possible shortcomings of your program (if well documented they might be partly forgiven).

## Building and running
- Developed using:
  - Ubuntu 16.04
  - Mono 5.8.0.129
  - MonoDevelop Version 7.4 (build 1035)
  - NUnit 3.10.1

#### Environment
- You must first install the development environment.
  - Mono installation: http://www.mono-project.com/download/stable/
    - Use `sudo apt-get install mono-complete`
  - Monodevelop installation: http://www.monodevelop.com/download/
- Download or clone the Interpreter repository.
- Open MonoDevelop and in the opening screen select to open a solution.
- In the dialog that opens find `Interpreter.sln` which is inside the repository clone, select it and click open.
- Make sure that you have internet connection as you open the sln file since NUnit is downloaded if it does not exist.
  - If NUnit it is not installed automatically, you should make sure you have NuGet package manager installed in MonoDevelop and then install NUnit package with it.

#### Development
- For development
  - edit `TestCode.txt` inside the cloned repository to contain your code.
    - the file is in gitignore and will not be in source control.
    - the file is preconfigured to be used by default when the interpreter is run through MonoDevelop.
  - select `Debug` from the dropdown on the top left.
  - press the play button to compile and run the interpreter with `TestCode.txt` as the input file.
  - To run NUnit tests from the top navigation in MonoDevelop select `Run` and under it select `Run unit tests`.

#### Releasing
- For release
  - to build the program select `Release` from the top left.
  - click `build` at the top menu bar and select `build all`.
  - After building go to `/Interpreter/bin/Release` inside your repository clone and in it you can see `Interpreter.exe`.
  - You can move the `Interpreter.exe` to some other folder if you want to.
  - To run the program you must first enable executing rights to the `Interpreter.exe`. This can usually be done by right clicking it, selecting properties, selecting permissions and ticking the box for allowing to execute the file as a program.
  - Create a file you want to execute next to the `Interpreter.exe`, for example `code.txt` containing `print "Hello world\n";`.
  - Open a command prompt in the same folder as the `Interpreter.exe`
  - Then use the command `./Interpreter.exe code.txt` to run the file you created.
    - The program takes exactly one argument, which is the absolute or relative path to the file to interpret.
    - The interpreter will print errors if it finds any. The first error printed is the first error found. Any errors printed after that may be caused by the first error detected.
    - If no errors are detected the code is run and any errors during execution halt the program execution. Assert WILL NOT halt the program execution.

## LL1 parser grammar
```
STMTS -> STMT ";" STMTSTAIL
STMTSTAIL -> STMTS | ε
STMT -> "var" IDENT ":" TYPE VARTAIL | IDENT ":=" EXPR | "for" IDENT "in" EXPR ".." EXPR "do" STMTS "end" "for" | "read" IDENT | "print" EXPR | "assert" "(" EXPR ")"
VARTAIL -> ":=" EXPR | ε
EXPR -> OPND EXPRTAIL | UNARYOP OPND
EXPRTAIL -> OP OPND | ε
OPND -> INT | STRING | IDENT | "(" EXPR ")"
TYPE -> "int" | "string" | "bool"
```

### Verification of the grammar
Verification results:
- All nonterminals are reachable and realizable
- The nullable nonterminals are: STMTSTAIL VARTAIL EXPRTAIL
- The endable nonterminals are: STMTS STMTSTAIL
- No cycles
- The grammar is LL1
- [View results](http://smlweb.cpsc.ucalgary.ca/vital-stats.php?grammar=STMTS+-%3E+STMT+%3B+STMTSTAIL.%0D%0ASTMTSTAIL+-%3E+STMTS+%7C+.%0D%0ASTMT+-%3E+var+IDENT+%3A+TYPE+VARTAIL+%7C+IDENT+%3Aq+EXPR+%7C+for+IDENT+in+EXPR+dd+EXPR+do+STMTS+end+for+%7C+read+IDENT+%7C+print+EXPR+%7C+assert+%28+EXPR+%29.%0D%0AVARTAIL+-%3E+%3Aq+EXPR+%7C+.%0D%0AEXPR+-%3E+OPND+EXPRTAIL+%7C+UNARYOP+OPND.%0D%0AEXPRTAIL+-%3E+OP+OPND+%7C+.%0D%0AOPND+-%3E+INT+%7C+STRING+%7C+IDENT+%7C+%28+EXPR+%29.%0D%0ATYPE+-%3E+int+%7C+string.%0D%0AINT+-%3E+intliteral.%0D%0ASTRING+-%3E+stringliteral.%0D%0AIDENT+-%3E+identifier.%0D%0AUNARYOP+-%3E+unaryop.%0D%0AOP+-%3E+op.)
- [View LL1 parsing table](http://smlweb.cpsc.ucalgary.ca/ll1-table.php?grammar=STMTS+-%3E+STMT+%3B+STMTSTAIL.%0ASTMTSTAIL+-%3E+STMTS+%7C+.%0ASTMT+-%3E+var+IDENT+%3A+TYPE+VARTAIL+%7C+IDENT+%3Aq+EXPR+%7C+for+IDENT+in+EXPR+dd+EXPR+do+STMTS+end+for+%7C+read+IDENT+%7C+print+EXPR+%7C+assert+%28+EXPR+%29.%0AVARTAIL+-%3E+%3Aq+EXPR+%7C+.%0AEXPR+-%3E+OPND+EXPRTAIL+%7C+UNARYOP+OPND.%0AEXPRTAIL+-%3E+OP+OPND+%7C+.%0AOPND+-%3E+INT+%7C+STRING+%7C+IDENT+%7C+%28+EXPR+%29.%0ATYPE+-%3E+int+%7C+string.%0AINT+-%3E+intliteral.%0ASTRING+-%3E+stringliteral.%0AIDENT+-%3E+identifier.%0AUNARYOP+-%3E+unaryop.%0AOP+-%3E+op.&substs=)

Grammar verified at [http://smlweb.cpsc.ucalgary.ca/start.html](http://smlweb.cpsc.ucalgary.ca/start.html).
Grammar converted to suitable form for verification:
```
STMTS -> STMT ; STMTSTAIL.
STMTSTAIL -> STMTS | .
STMT -> var IDENT : TYPE VARTAIL | IDENT :q EXPR | for IDENT in EXPR dd EXPR do STMTS end for | read IDENT | print EXPR | assert ( EXPR ).
VARTAIL -> :q EXPR | .
EXPR -> OPND EXPRTAIL | UNARYOP OPND.
EXPRTAIL -> OP OPND | .
OPND -> INT | STRING | IDENT | ( EXPR ).
TYPE -> int | string | bool.
INT -> intliteral.
STRING -> stringliteral.
IDENT -> identifier.
UNARYOP -> unaryop.
OP -> op.
```
