VBF
========

VBF is a set of tools/libraries for compilers. It has seperated libraries for scanners and parsers. One can easily use one component as well as use them all together.

#### How to build:
1. Install Visual Studio 2012 with .Net Framework 4.5
2. Open Options dialog, go to "Package Manager" and then check "Allow NuGet to download missing packages during build"
3. Open Project VBF/src/Compilers/Compilers.sln
4. Build

#### Components:
* VBF.Compilers.Common

	Provides source file reader, compilation error manager, etc

* VBF.Compilers.Scanners

	Provides DFA based scanner builder using regular expressions. There're multiple types of scanners to choose: a standard Scanner, a PeekableScanner with the capability to peek n tokens, a ForkableScanner that can fork to multiple scanner heads.

* VBF.Compilers.Parsers

	Provides LR based, auto error recovery parser generator. To start, inherit ParserBase<T> class and build your own parser. It is recommended for most parsers.

* VBF.Compilers.Parsers.Combinators

	Similar to Compilers.Parsers but it's an LL based parser combinator library. To start, inherit ParserFrame<T> class and build your own parser. It is used to study LL grammars.

#### Samples:
* MiniSharp: VBF\src\Samples\MiniSharp

	MiniSharp is a fully functional compiler of a very small subset of C# language. It contains parsers and scanners built with VBF and an MSIL code generator.
	
	It is a good sample for programming language and DSL authors.