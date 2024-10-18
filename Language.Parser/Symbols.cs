using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Language.Parser
{
    public class Symbols
    {
        // key words

        public static readonly Symbols KW_DECLARE = new Symbols("declare", SymbolTypes.KeyWord);
        public static readonly Symbols KW_IF = new Symbols("if", SymbolTypes.KeyWord);
        public static readonly Symbols KW_AS = new Symbols("as", SymbolTypes.KeyWord);
        public static readonly Symbols KW_ELSE = new Symbols("else", SymbolTypes.KeyWord);
        public static readonly Symbols KW_TYPE = new Symbols("type", SymbolTypes.KeyWord);
        public static readonly Symbols KW_CONST = new Symbols("const", SymbolTypes.KeyWord);
        public static readonly Symbols KW_FUNCTION = new Symbols("function", SymbolTypes.KeyWord);
        public static readonly Symbols KW_GET = new Symbols("get", SymbolTypes.KeyWord);
        public static readonly Symbols KW_SET = new Symbols("set", SymbolTypes.KeyWord);

        public static readonly Symbols KW_VAR = new Symbols("var", SymbolTypes.KeyWord);
        public static readonly Symbols KW_RETURN = new Symbols("return", SymbolTypes.KeyWord);
        public static readonly Symbols KW_BREAK = new Symbols("break", SymbolTypes.KeyWord);
        public static readonly Symbols KW_CONTINUE = new Symbols("continue", SymbolTypes.KeyWord);
        public static readonly Symbols KW_ENUM = new Symbols("enum", SymbolTypes.KeyWord);
        public static readonly Symbols KW_FOR = new Symbols("for", SymbolTypes.KeyWord);
        public static readonly Symbols KW_NEW = new Symbols("new", SymbolTypes.KeyWord);
        public static readonly Symbols KW_THIS = new Symbols("this", SymbolTypes.KeyWord);
        public static readonly Symbols KW_WHILE = new Symbols("while", SymbolTypes.KeyWord);
        public static readonly Symbols KW_PRIVATE = new Symbols("private", SymbolTypes.KeyWord);
        public static readonly Symbols KW_PROTECTED = new Symbols("protected", SymbolTypes.KeyWord);
        public static readonly Symbols KW_PUBLIC = new Symbols("public", SymbolTypes.KeyWord);
        public static readonly Symbols KW_STATIC = new Symbols("static", SymbolTypes.KeyWord);
        public static readonly Symbols KW_CLASS = new Symbols("class", SymbolTypes.KeyWord);

        public static readonly Symbols KW_IMPORT = new Symbols("import", SymbolTypes.KeyWord);
        public static readonly Symbols KW_FROM = new Symbols("from", SymbolTypes.KeyWord);
        public static readonly Symbols KW_EXPORT = new Symbols("export", SymbolTypes.KeyWord);
        public static readonly Symbols KW_SEALED = new Symbols("sealed", SymbolTypes.KeyWord);
        public static readonly Symbols KW_INTERNAL = new Symbols("internal", SymbolTypes.KeyWord);

        public static readonly Symbols KW_EXTENDS = new Symbols("extends", SymbolTypes.KeyWord);
        public static readonly Symbols KW_IMPLEMENTS = new Symbols("implements", SymbolTypes.KeyWord);





        // types
        public static readonly Symbols TYPED_OBJECT = new Symbols("object", SymbolTypes.Identifier);

        public static readonly Symbols TYPED_VOID = new Symbols("void", SymbolTypes.Identifier);
        public static readonly Symbols TYPED_BOOLEAN = new Symbols("boolean", SymbolTypes.Identifier);
        public static readonly Symbols TYPED_STRING = new Symbols("string", SymbolTypes.Identifier);

        // byte char short ushort long ulong float double
        // number = double
        public static readonly Symbols TYPED_NUMBER = new Symbols("number", SymbolTypes.Identifier);

        //public readonly static Symbols Byte = new Symbols("byte", SymbolTypes.Typed);

        public static readonly Symbols KW_COROUTINE = new Symbols("coroutine", SymbolTypes.Punctuator);

        /// <summary>
        /// token typeof
        /// </summary>
        public static readonly Symbols OP_TYPEOF = new Symbols("typeof", SymbolTypes.Punctuator);

        // Punctuator
        /// <summary>
        /// token {
        /// </summary>
        public static readonly Symbols PT_LEFTBRACE = new Symbols("{", SymbolTypes.Punctuator);

        /// <summary>
        /// token }
        /// </summary>
        public static readonly Symbols PT_RIGHTBRACE = new Symbols("}", SymbolTypes.Punctuator);

        /// <summary>
        /// token (
        /// </summary>
        public static readonly Symbols PT_LEFTPARENTHESIS = new Symbols("(", SymbolTypes.Punctuator);

        /// <summary>
        /// token )
        /// </summary>
        public static readonly Symbols PT_RIGHTPARENTHESIS = new Symbols(")", SymbolTypes.Punctuator);

        /// <summary>
        /// token [
        /// </summary>
        public static readonly Symbols PT_LEFTBRACKET = new Symbols("[", SymbolTypes.Punctuator);

        /// <summary>
        /// token ]
        /// </summary>
        public static readonly Symbols PT_RIGHTBRACKET = new Symbols("]", SymbolTypes.Punctuator);

        /// <summary>
        /// token ;
        /// </summary>
        public static readonly Symbols PT_SEMICOLON = new Symbols(";", SymbolTypes.Punctuator);

        /// <summary>
        /// token ,
        /// </summary>
        public static readonly Symbols PT_COMMA = new Symbols(",", SymbolTypes.Punctuator);

        /// <summary>
        /// token .
        /// </summary>
        public static readonly Symbols PT_DOT = new Symbols(".", SymbolTypes.Punctuator);

        /// <summary>
        /// token :
        /// </summary>
        public static readonly Symbols PT_COLON = new Symbols(":", SymbolTypes.Punctuator);

        /// <summary>
        /// token =>
        /// </summary>
        public static readonly Symbols PT_LAMBDA = new Symbols("=>", SymbolTypes.Operator);

        // Operators
        /// <summary>
        /// token <
        /// </summary>
        public static readonly Symbols OP_LESSTHAN = new Symbols("<", SymbolTypes.Operator);

        /// <summary>
        /// token >
        /// </summary>
        public static readonly Symbols OP_GREATERTHAN = new Symbols(">", SymbolTypes.Operator);

        /// <summary>
        /// token <=
        /// </summary>
        public static readonly Symbols OP_LESSTHANOREQUAL = new Symbols("<=", SymbolTypes.Operator);

        /// <summary>
        /// token >=
        /// </summary>
        public static readonly Symbols OP_GREATERTHANOREQUal = new Symbols(">=", SymbolTypes.Operator);

        /// <summary>
        /// token ==
        /// </summary>
        public static readonly Symbols OP_EQUALITY = new Symbols("==", SymbolTypes.Operator);

        /// <summary>
        /// token !=
        /// </summary>
        public static readonly Symbols OP_INEQUALITY = new Symbols("!=", SymbolTypes.Operator);

        /// <summary>
        /// token +
        /// </summary>
        public static readonly Symbols OP_PLUS = new Symbols("+", SymbolTypes.Operator);

        /// <summary>
        /// token -
        /// </summary>
        public static readonly Symbols OP_MINUS = new Symbols("-", SymbolTypes.Operator);

        /// <summary>
        /// token *
        /// </summary>
        public static readonly Symbols OP_MULTIPLY = new Symbols("*", SymbolTypes.Operator);

        /// <summary>
        /// token /
        /// </summary>
        public static readonly Symbols OP_DIVIDE = new Symbols("/", SymbolTypes.Operator);

        /// <summary>
        /// token %
        /// </summary>
        public static readonly Symbols OP_MODULO = new Symbols("%", SymbolTypes.Operator);

        /// <summary>
        /// token ...
        /// </summary>
        public static readonly Symbols OP_SPREAD = new Symbols("...", SymbolTypes.Operator);

        /// <summary>
        /// token ++
        /// </summary>
        public static readonly Symbols OP_INCREMENT = new Symbols("++", SymbolTypes.Operator);

        /// <summary>
        /// token --
        /// </summary>
        public static readonly Symbols OP_DECREMENT = new Symbols("--", SymbolTypes.Operator);

        /// <summary>
        /// token "<<"
        /// </summary>
        public static readonly Symbols OP_LEFTSHIFT = new Symbols("<<", SymbolTypes.Operator);

        /// <summary>
        /// token >>
        /// </summary>
        public static readonly Symbols OP_SIGNEDRIGHTSHIFT = new Symbols(">>", SymbolTypes.Operator);

        /// <summary>
        /// token &
        /// </summary>
        public static readonly Symbols OP_BITWISEAND = new Symbols("&", SymbolTypes.Operator);

        /// <summary>
        /// token |
        /// </summary>
        public static readonly Symbols OP_BITWISEOR = new Symbols("|", SymbolTypes.Operator);

        /// <summary>
        /// token ^
        /// </summary>
        public static readonly Symbols OP_BITWISEXOR = new Symbols("^", SymbolTypes.Operator);

        /// <summary>
        /// token !
        /// </summary>
        public static readonly Symbols OP_LOGICALNOT = new Symbols("!", SymbolTypes.Operator);

        /// <summary>
        /// token ~
        /// </summary>
        public static readonly Symbols OP_BITWISENOT = new Symbols("~", SymbolTypes.Operator);

        /// <summary>
        /// token &&
        /// </summary>
        public static readonly Symbols OP_LOGICALAND = new Symbols("&&", SymbolTypes.Operator);

        /// <summary>
        /// token ||
        /// </summary>
        public static readonly Symbols OP_LOGICALOR = new Symbols("||", SymbolTypes.Operator);

        /// <summary>
        /// token ?
        /// </summary>
        public static readonly Symbols OP_CONDITIONAL = new Symbols("?", SymbolTypes.Operator);

        /// <summary>
        /// token =
        /// </summary>
        public static readonly Symbols OP_ASSIGNMENT = new Symbols("=", SymbolTypes.Operator);

        /// <summary>
        /// token +=
        /// </summary>
        public static readonly Symbols OP_COMPOUNDADD = new Symbols("+=", SymbolTypes.Operator);

        /// <summary>
        /// token -=
        /// </summary>
        public static readonly Symbols OP_COMPOUNDSUBTRACT = new Symbols("-=", SymbolTypes.Operator);

        /// <summary>
        /// token *=
        /// </summary>
        public static readonly Symbols OP_COMPOUNDMULTIPLY = new Symbols("*=", SymbolTypes.Operator);

        /// <summary>
        /// token /=
        /// </summary>
        public static readonly Symbols OP_COMPOUNDDIVIDE = new Symbols("/=", SymbolTypes.Operator);

        /// <summary>
        /// token %=
        /// </summary>
        public static readonly Symbols OP_COMPOUNDMODULO = new Symbols("%=", SymbolTypes.Operator);



        /// <summary>
        /// token #
        /// </summary>
        public static readonly Symbols KW_SHARP = new Symbols("#", SymbolTypes.Operator);


        /// <summary>
        /// token End of File
        /// </summary>
        public static readonly Symbols KW_EOF = new Symbols("END OF FILE", SymbolTypes.Operator);

        /// <summary>
        /// token true
        /// </summary>
        public static readonly Symbols VALUE_TRUE = new Symbols("true", SymbolTypes.BooleanValue);

        /// <summary>
        /// token false
        /// </summary>
        public static readonly Symbols VALUE_FALSE = new Symbols("false", SymbolTypes.BooleanValue);

        /// <summary>
        /// token null
        /// </summary>
        public static readonly Symbols VALUE_NULL = new Symbols("null", SymbolTypes.NullValue);

        private static readonly Dictionary<string, Symbols> _SymbolMaps = new Dictionary<string, Symbols>();

        static Symbols()
        {
            var type = typeof(Symbols);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.FieldType == typeof(Symbols));
            foreach (var field in fields)
            {
                var symbol = field.GetValue(null) as Symbols;
                _SymbolMaps.Add(symbol.Name, symbol);
            }
        }

        /// <summary>
        /// prase symbol from string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Symbols FromString(string name)
        {
            _SymbolMaps.TryGetValue(name, out var symbol);
            return symbol;
        }

        /// <summary>
        /// get symbol name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// get symbol type
        /// </summary>
        internal SymbolTypes Type { get; private set; }

        private Symbols(string name, SymbolTypes type)
        {
            Name = name;
            Type = type;
        }

        public override string ToString()
        {
            return $"{Name}:{Type}";
        }
    }
}
