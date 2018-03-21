# Interpreter

- Course: Compilers https://courses.helsinki.fi/fi/csm14204/119559595
- Studet: Risto Mikkola
- Project: Interpreter
- Date of delivery: ??

## Architecture
- Describe the overall architecture of your language processor with, e.g., UML diagrams.
- Explain your diagrams.
- Tell about possible shortcomings of your program (if well documented they might be partly forgiven).

![Diagram of the high level architecture](https://github.com/Rochet2/compilers/blob/master/documents/Architecture.svg)

The UML diagram above shows how relevant parts of the program are connected on a high level.
Next we will explain the purpose of each of the parts in the diagram.

- `IO` is an abstract class that IO classes inherit. The IO classes implement read and write functions to support input and output operations. The purpose of the `IO` class is to abstract away the IO implementation to allow flexibility for example for testing.
- `StringIO` implements `IO` and it is initialized with a specific input and it contains a string where it outputs any output. The purpose of it is to allow in memory printing of IO.
- `Position` is a class that contains information about a location in the source file. It has the absolute character position, line number and the character number on that line (column).
- The `InputBuffer` owns a `Position` that it updates to match the position of input it has read. The purpose of the `InputBuffer` is to read characters from the input to the buffer to allow having a lookahead that we can peek.
- `Lexeme` is a class that contains a `Position`, a token string and the type of the token. The purpose of it is to contain all relevant information from the lexical analysis of the token.
- The `Lexer` owns an `InputBuffer` and it reads characters from it. The purpose of the `Lexer` is to perform lexical analysis on the characters being read and produce `Lexeme` from them. It uses the `InputBuffer`'s lookahead to try deduce what type of token it should be expecting. It will copy the `Position` of the `InputBuffer` to the `Lexeme` it produces. The `Lexer` owns an IO instance where it outputs any error messages.
- `ASTNode` is an abstract class that contains the type of the `ASTNode` and may contain a `Lexeme`. The purpose of the `ASTNode` is to allow building the abstract syntax tree (AST) from tokens. The child classes that inherit this class can specialize themselves in different ways.
- `ASTVariableNode` is not in the diagram, but it is an abstract class that inherits `ASTNode`. The purpose of it is to act as the common high level class for all values that can be printed or stored. It contains an overridable function that returns the value of the variable.
- `Parser` is a class that owns a `Lexer` to read tokens from and an `IO` instance to output error messages to. The purpose of it is to read `Lexeme` from the `Lexer` and build an AST from them by creating `ASTNode`s according to an LL1 grammar it follows.
- `Visitor` is an abstract class that implements basic utilities for Visit pattern to be used on an AST made of `ASTNode`. It owns an `ASTNode` which is the root of the AST and an `IO` to which it outputs any error messages. The class also implements a variable store and type assertion and checking functions to be used by the child classes that inherit it. The purpose of the `Visitor` class is to provide commonly used functionality for other visitor pattern classes that walk through the AST.
- `Analysis` is a class that inherits the `Visitor` class. The purpose of it is to implement the visit functions and other functionaltiy needed for performing semantic analysis on an AST. It can throw `VisitorException`, which is catched and outputted by the `Visitor` super class.
- `ExpressionPrinter` is a class that inherits the `Visitor` class. The purpose of it is to implement the visit functions and other functionaltiy needed for printing the tokens of an `ASTNode` tree, which consists of an expression, to the given `IO`. It can throw `VisitorException`, which is catched and outputted by the `Visitor` super class.
- `Interpreter` is a class that inherits the `Visitor` class. The purpose of it is to implement the visit functions and other functionaltiy needed for interpreting an AST. It will use the inherited `IO` to read user input during execution when needed. It also uses `ExpressionPrinter` to walk the assertion condition expression for printing the expression in a diagnostic message. To catch the output of the `ExpressionPrinter` it will use `StringIO`. It can throw `VisitorException`, which is catched and outputted by the `Visitor` super class.
- The `MainClass`, which contains the program entry point, will create an instance of an `IO` and `InputBuffer` and then give them to all class instances it creates. Then it will create a `Lexer` and run it, create a `Parser` and run it, create an `Analysis` and run it and finally create an `Interpreter` and run it. At each stage it can exit if errors block it from continuing.


What is missing from the diagram are the enums, the classes that are used for testing, the classes that extend `ASTNode`, exception classes, the main program class that uses most other classes and the ConsoleIO class that the main program creates to be used as the IO when the Main function is run. Also any standard library classes are not shown. Connections that are not shown are the indirect connections for example where `Analysis` could print the `Position` of a `Lexeme`.

#### Architectural decisions
Here are some additional architectural decisions that were made.
They were not specified or not clearly defined in the specification of the interpreter.
- assert will stop the program from executing if the assertion fails.
- for-loop control variable will be at end value + 1 after the for loop ends or if for loop is never entered then it will be set to the for-loop range beginning value.
- if a non numeric value is read from input during program execution the program will keep on reading until a number is read.
- editing the control variable is prohibited through checking mutability during semantic analysis and runtime.
- nested block comments such as `/*/**/*/` are accepted, however `/*/**/` is not. All block comments must be complete.
- boolean < operator is defined so that it is true when left operand is false and right operand is true, false otherwise.
- default values for each type are: int `0`, string `""`, bool `false`.

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
