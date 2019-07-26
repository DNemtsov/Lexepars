[![NuGet](https://img.shields.io/nuget/v/lexepars.svg)](https://www.nuget.org/packages/lexepars/)

# Lexepars

Lexepars is a concise, lightweight, but powerful monadic parser combinator library. [Monadic](http://learnyouahaskell.com/a-fistful-of-monads) means that you always get conclusive parsing results. Combininig easier parsers together allows you to build however complex grammars (which are, in turn, parsers). The library can compete with the full-featured parser generation suites such as Antlr while not possessing their wordiness and the overhead of dealing with the custom grammar specification languages, multiple tools in the chain and the code "bestowed upon you" in your project.
What makes the Lexepars so powerful is its __separate tokenization and parsing phases__. A perfect illustration of that is __the only dotnet parser combinator lib capable of parsing [the off-side rule languages](https://en.wikipedia.org/wiki/Off-side_rule)__ where the indents are part of the syntax, even as tricky as [YAML](https://en.wikipedia.org/wiki/YAML). Check out the [YAML Grammar](https://github.com/DNemtsov/Lexepars.Grammars.Yaml/blob/master/src/Lexepars.Grammars.Yaml/YamlGrammar.cs)!
It's written entirely in C# and is fully [CLS-compliant](https://stackoverflow.com/questions/1828575/why-should-i-write-cls-compliant-code), so you can create your grammars right in your code in any dotnet language. And don't forget to set up the [NuGet symbol server](https://devblogs.microsoft.com/nuget/improved-package-debugging-experience-with-the-nuget-org-symbol-server) to make debugging a breeze:)
Lexepars is deemed as the natural evolution of Patrick Lioi's [Parsley](https://github.com/plioi/parsley) with an emphasis made on memory and speed optimizations for very big input files, as well as client code conciseness and flexibility of usage. It can parse context-sensitive, infinite look-ahead grammars, but it performs best on [LL(1) grammars and languages](http://www.csd.uwo.ca/~moreno/CS447/Lectures/Syntax.html/node14.html).

## Installation

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install [the Lexepars package](https://www.nuget.org/packages/Lexepars/) from the package manager console:

    PM> Install-Package Lexepars
    
It's just a small single assembly, with no strings attached:)

## Tokenization (Lexing)

The lexer phase is performed with a prioritized list of regex patterns, and parser grammars are expressed in terms of the tokens produced by the lexer.
Input text being parsed is represented with a [`InputText`](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/InputText/InputText.cs) or, better yet, a [`LinedInputText`](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/InputText/LinedInputText.cs) instance, which tracks the current parsing position:

        var text = new Text("some input to parse");

The lexer phase is implemented by anything that produces an `IEnumerable<`[Token](https://github.com/DNemtsov/Lexepars/tree/master/src/Lexepars/Token)`>`.  The default implementation, [`Lexer`](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/Lexer/LinedLexer.cs), builds the series of tokens when given a prioritized series of [`TokenKind`](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/Token/TokenKind.cs) token recognizers.
The most common `TokenKind` implementation is [`PatternTokenKind`](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/Token/TokenKinds/PatternTokenKind.cs), which recognizes tokens via regular expressions, but there are many [more](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/Token/TokenKinds). `TokenKind`s can be marked as skippable, if you want them to be recognized but discarded:

        var text = new Text("1 2 3 a b c");
        var lexer = new Lexer(new Pattern("letter", @"[a-z]"),
                              new Pattern("number", @"[0-9]+"),
                              new Pattern("whitespace", @"\s+", skippable: true));

        Token[] tokens = lexer.ToArray();

Above, the array `tokens` will contain 6 `Token` objects. Each `Token` contains the lexeme ("1", "a", etc), the `TokenKind` that matched it, and the [`Position`](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/Token/Position.cs) where the lexeme was found.

The collection of `Token` produced by the lexer phase is wrapped in a [`TokenStream`](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/Token/TokenStream.cs), which allows the rest of the system to traverse the collection of tokens in an immutable fashion.

## Parsing

[`Parser`](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/IParser.cs) consumes a [`TokenStream`](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/Token/TokenStream.cs) and produces a parsed value based on the lexemes of the parsed tokens:

    public interface IParser<out TValue> : IGeneralParser
    {
        IReply<TValue> Parse(TokenStream tokens);
    }
    
When the value is not needed, e.g. for grammar validation purposes, [`IGeneralParser`](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/IGeneralParser.cs) comes into play as a valuable means of optimization.
    
    public interface IGeneralParser
    {
        IGeneralReply ParseGenerally(TokenStream tokens);
    }

[`IReply<out TValue>`](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/Reply/IReply.cs) and [`IGeneralReply`](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/Reply/IGeneralReply.cs) describe whether or not the parser succeeded, the parsed value (on success), a possibly-empty error message list, and a reference to the `TokenStream` representing the remaining unparsed tokens.

## Grammars

Grammars should inherit from [`Grammar`](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/Grammar/Grammar.cs) to take advantage of the numerous parser [primitives](https://github.com/DNemtsov/Lexepars/tree/master/src/Lexepars/Parsers) and [extensions](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/ParserExtensions.cs) that serve as the building blocks for the [rules](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/Grammar/GrammarRule.cs).  Grammars should define each grammar rule in terms of these primitives, ultimately exposing the entry rule as some `Parser<TValue>`.  Grammar rule bodies may consist of LINQ queries, which allow you to glue together other grammar rules in sequence:

See the [integration tests](https://github.com/DNemtsov/Lexepars.Grammars.Json/blob/master/src/Lexepars.Grammars.Json.Tests/JsonGrammarTests.cs) for a [sample JSON grammar](https://github.com/DNemtsov/Lexepars.Grammars.Json/blob/master/src/Lexepars.Grammars.Json/JsonGrammar.cs).

Finally, we can put all these pieces together to parse some text:

        var input = "{\"zero\" : 0, \"one\" : 1, \"two\" : 2}";
        var lexer = new JsonLexer();
        var tokens = lexer.Tokenize(input);
        var jobject = (JObject)JsonGrammar.Json.Parse(tokens).Value;
        
## Documentation

Because of the fast evolution of the library, it would be hard to maintain standalone documentation. Therefore, much attention is paid to keeping the code either self-explanatory, or furnished with the sufficient documentation comments with the advantage of having them available in the compiled assembly.
Here are the summarized points of interest to go through:
* Core functionality:
    * [Token](https://github.com/DNemtsov/Lexepars/tree/master/src/Lexepars/Token)
    * [Lexer](https://github.com/DNemtsov/Lexepars/tree/master/src/Lexepars/Lexer)
    * [Parser](https://github.com/DNemtsov/Lexepars/tree/master/src/Lexepars/Parsers)
    * [Grammar](https://github.com/DNemtsov/Lexepars/tree/master/src/Lexepars/Grammar) (truly the hub for everything)
      * Of special interest for __BIG__ input texts
        * [LinedInputText](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/InputText/LinedInputText.cs)
        * [LinedLexer](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/Lexer/LinedLexer.cs)
        * [TokenStreamWithCancellation](https://github.com/DNemtsov/Lexepars/blob/master/src/Lexepars/Token/TokenStreamWithCancellation.cs)
* Samples and instructive code
    * [Core unit tests](https://github.com/DNemtsov/Lexepars/tree/master/src/Lexepars.Tests)
    * [JSON Grammar](https://github.com/DNemtsov/Lexepars.Grammars.Json/blob/master/src/Lexepars.Grammars.Json/JsonGrammar.cs) (also available as a [![NuGet](https://img.shields.io/nuget/v/Lexepars.Grammars.Json.svg)](https://www.nuget.org/packages/Lexepars.Grammars.Json/)) with [unit tests](https://github.com/DNemtsov/Lexepars.Grammars.Json/blob/master/src/Lexepars.Grammars.Json.Tests/JsonGrammarTests.cs)
    * [YAML Grammar](https://github.com/DNemtsov/Lexepars.Grammars.Yaml/blob/master/src/Lexepars.Grammars.Yaml/YamlGrammar.cs) (also available as a [![NuGet](https://img.shields.io/nuget/v/Lexepars.Grammars.Yaml.svg)](https://www.nuget.org/packages/Lexepars.Grammars.Yaml/)) with [unit tests](https://github.com/DNemtsov/Lexepars.Grammars.Yaml/blob/master/src/Lexepars.Grammars.Yaml.Tests/YamlGrammarTests.cs)
* [Unit test fixtures ![NuGet](https://img.shields.io/nuget/v/Lexepars.TestFixtures.svg)](https://www.nuget.org/packages/Lexepars.TestFixtures/) (I bet you'll need tons of tests fot your lexers and grammars!)
* [Symbols](https://symbols.nuget.org/download/symbols)

*Enjoy and happy time coding!*