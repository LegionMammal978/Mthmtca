***Note:* For a more fleshed-out version of this idea, offering a better compression ratio, see [Sledgehammer](https://github.com/tkwa/Sledgehammer).**

# Mthmtca
A version of Mathematica for golfing purposes. The interpreter is currently designed to work with UNIX-like systems.
## Setup
Setup is simple. First, ensure that you have Mono and a working Mathematica kernel installed. Then, just navigate to this directory in a command prompt and execute `make`. *DO NOT DELETE ANY FILES, OR THE INTERPRETER WILL NOT RUN.*
## Usage
To find the index of a symbol, run `./mthmtca <-s|--symbol> <symbol name>`. To run a Mthmtca program, run `./mthmtca [filename]`. If `[filename]` is omitted, then the program will be read from STDIN.
## Specification
The input program is interpreted as a sequence of 16 bit or 8 bit words, which are interpreted as a single expression. Depending on the possible values of the first 16 bits:

first 16 bits | Result expression
--- | ---
`00xx xxxx` `xxxx xxxx` | Represents a compound expression. The remaining data is read as `nn expr1 expr2 ... exprn`. Interpreted as Mathematica command `X[expr1, expr2, ..., exprn]`, where `nn` gives the number of arguments, and `X = Names["*"][[x+1]]`.
`011s xxxx` `xxxx xxxx` | Represents the integer `x` if `s = 0`, or `-x` if `s = 1`.
`010s xxxx` `xxxx xxxx` | Represents a big integer. Read the next `x` bytes (big endian) to form the number, and negate the result if `s = 1`.
`10xx xxxx` `xxxx xxxx` | Represents a string. Read the next `x` bytes to form a string.
`11xx xxxx` `xxxx xxxx` | Represents a symbol `X = Names["*"][[x+1]]`.
