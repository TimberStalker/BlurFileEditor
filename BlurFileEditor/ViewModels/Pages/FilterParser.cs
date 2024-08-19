using BlurFileEditor.Utils.FIlter;
using BlurFormats.Serialization.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace BlurFileEditor.ViewModels.Pages;

public static class FilterParser
{
    public static IEntityFilter? CreateFilter(string filterString)
    {
        try
        {
            var tokens = Tokenizer.GetTokens(new TextWindow(filterString));
            var parser = new Parser(tokens);
            return parser.Parse();
        }
        catch
        {
            return null;
        }
    }

    class Token
    {
        public string Name { get; }
        public string Value { get; }
        public Range Range { get; }
        public Token(string name, string value, Range range)
        {
            Name = name;
            Value = value;
            Range = range;
        }
    }
    static class Tokens
    {
        public const string OpenBracket = nameof(OpenBracket);
        public const string CloseBracket = nameof(CloseBracket);
        public const string OpenParenthesis = nameof(OpenParenthesis);
        public const string CloseParenthesis = nameof(CloseParenthesis);
        public const string Colon = nameof(Colon);
        public const string Dot = nameof(Dot);
        public const string Comma = nameof(Comma);
        public const string Number = nameof(Number);
        public const string IntegerSpecifier = nameof(IntegerSpecifier);
        public const string AlphaNumeric = nameof(AlphaNumeric);
        public const string Boolean = nameof(Boolean);
        public const string Plus = nameof(Plus);
        public const string Minus = nameof(Minus);
        public const string GreaterThanSign = nameof(GreaterThanSign);
        public const string LessThanSign = nameof(LessThanSign);
        public const string EqualsSign = nameof(EqualsSign);
        public const string Combining = nameof(Combining);
        public const string Not = nameof(Not);
        public const string Tilde = nameof(Tilde);
        public const string DoubleQuote = nameof(DoubleQuote);
        public const string StringPart = nameof(StringPart);
        public const string StringEscapeSequence = nameof(StringEscapeSequence);
    }
    static class Tokenizer
    {
        public static IEnumerable<Token> GetTokens(TextWindow window)
        {
            while(!window.IsAtEnd)
            {
                switch(window.Peek())
                {
                    case '(':
                    {
                        yield return new Token(Tokens.OpenParenthesis, window.Read(1, out var range).ToString(), range);
                        break;
                    }
                    case ')':
                    {
                        yield return new Token(Tokens.CloseParenthesis, window.Read(1, out var range).ToString(), range);
                        break;
                    }
                    case '{':
                    {
                        yield return new Token(Tokens.OpenBracket, window.Read(1, out var range).ToString(), range);
                        break;
                    }
                    case '}':
                    {
                        yield return new Token(Tokens.CloseBracket, window.Read(1, out var range).ToString(), range);
                        break;
                    }
                    case ':':
                    {
                        yield return new Token(Tokens.Colon, window.Read(1, out var range).ToString(), range);
                        break;
                    }
                    case '.':
                    {
                        yield return new Token(Tokens.Dot, window.Read(1, out var range).ToString(), range);
                        break;
                    }
                    case ',':
                    {
                        yield return new Token(Tokens.Comma, window.Read(1, out var range).ToString(), range);
                        break;
                    }
                    case '>':
                    {
                        yield return new Token(Tokens.GreaterThanSign, window.Read(1, out var range).ToString(), range);
                        break;
                    }
                    case '<':
                    {
                        yield return new Token(Tokens.LessThanSign, window.Read(1, out var range).ToString(), range);
                        break;
                    }
                    case '=':
                    {
                        yield return new Token(Tokens.EqualsSign, window.Read(1, out var range).ToString(), range);
                        break;
                    }
                    case '+':
                    {
                        yield return new Token(Tokens.Plus, window.Read(1, out var range).ToString(), range);
                        break;
                    }
                    case '-':
                    {
                        yield return new Token(Tokens.Minus, window.Read(1, out var range).ToString(), range);
                        break;
                    }
                    case '~':
                    {
                        yield return new Token(Tokens.Tilde, window.Read(1, out var range).ToString(), range);
                        break;
                    }
                    case '"':
                    {
                        Range range;
                        yield return new Token(Tokens.DoubleQuote, window.Read(1, out range).ToString(), range);
                        while(window.Peek() != '"')
                        {
                            if(window.Peek() == '\\')
                            {
                                switch (window.Peek(1))
                                {
                                    case '"':
                                    case '\\':
                                    case 'n':
                                    case 'r':
                                    case 't':
                                    case '0':
                                        yield return new Token(Tokens.StringEscapeSequence, window.Read(2, out range).ToString(), range);
                                        break;
                                    default:
                                        throw new Exception();
                                }
                            }
                            else
                            {
                                int i = 0;
                                if(i !>= 0)
                                {

                                }
                                yield return new Token(Tokens.StringPart, window.ReadWhile(c => c != '"' && c != '\\', out range).ToString(), range);
                            }
                        }
                        yield return new Token(Tokens.DoubleQuote, window.Read(1, out range).ToString(), range);
                        break;
                    }
                    case '0':
                    {
                        if (window.Peek(1) is 'x' or 'b' or 'o')
                        {
                            yield return new Token(Tokens.IntegerSpecifier, window.Read(2, out var range).ToString(), range);
                        }
                        else
                        {
                            yield return new Token(Tokens.Number, window.ReadWhile(c => char.IsDigit(c) || c == 'e' || c == 'E', out var range).ToString(), range);
                        }
                        break;
                    }
                    case >= '0' and <= '9':
                    {
                        yield return new Token(Tokens.Number, window.ReadWhile(c => char.IsDigit(c) || c == '.' || c == 'e' || c =='_' || c =='+' || c == '-' || c == 'E', out var range).ToString(), range);
                        break;
                    }
                    case >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_':
                    {
                        var text = window.ReadWhile(c => char.IsLetterOrDigit(c) || c == '_', out var range).ToString();

                        yield return new Token(text switch
                        {
                            "or" => Tokens.Combining,
                            "and" => Tokens.Combining,
                            "xor" => Tokens.Combining,
                            "not" => Tokens.Not,
                            "true" => Tokens.Boolean,
                            "false" => Tokens.Boolean,
                            _ => Tokens.AlphaNumeric
                        }, text, range);
                        break;
                    }
                    case var c:
                        if(char.IsWhiteSpace(c))
                        {
                            window.ReadWhile(c => char.IsWhiteSpace(c), out _);
                            break;
                        }
                        throw new Exception($"{c} is not a recognized character.");
                }
            }
        }
    }
    class Parser
    {
        Token[] tokens;
        int index = 0;

        Token Token => tokens[index];
        bool IsAtEnd => index >= tokens.Length;
        
        public Parser(IEnumerable<Token> tokens) 
        {
            this.tokens = tokens.ToArray();
        }
        public Token Consume(string tokenType)
        {
            if (Token.Name != tokenType) throw new Exception($"Tried to consume {tokenType} but instead found {Token.Name}");
            var result = Token;
            index++;
            return result;
        }
        public bool Test(string tokenType)
        {
            if(IsAtEnd) return false;
            return Token.Name == tokenType;
        }
        public bool TryConsume(string tokenType, [NotNullWhen(true)]out Token? token)
        {
            token = null;
            if (IsAtEnd) return false;
            if (index >= tokens.Length) return false;
            if(Token.Name == tokenType)
            {
                token = Token;
                index++;
                return true;
            }
            return false;
        }
        public IEntityFilter? Parse()
        {
            IEntityFilter? filter = null;
            if(TryConsume(Tokens.AlphaNumeric, out var token))
            {
                filter = new EntityTypeFilter() { AllowSubclasses = true, Type = token.Value };
            }
            if (Test(Tokens.OpenBracket))
            {
                var next = ParseMain();
                if(filter is null)
                {
                    filter = next;
                } else
                {
                    filter = new MultiEntityFilter(filter, next);
                }
            }
            return filter;
        }
        public IEntityFilter ParseMain()
        {
            Consume(Tokens.OpenBracket);
            IEntityFilter filters = new ConstantEntityFilter(true);
            
            do
            {
                if(TryConsume(Tokens.AlphaNumeric, out var fieldName))
                {
                    Consume(Tokens.Colon);
                    var filter = ParseCombining();
                    filter = new EntityFieldFilter(filter, fieldName.Value);

                    filters = new MultiEntityFilter(filters,filter) { Type = MultiEntityFilter.FilterType.And };
                }
            } while (TryConsume(Tokens.Comma, out _));
            Consume(Tokens.CloseBracket);
            return filters;
        }
        public IEntityFilter ParseCombining()
        {
            var filter = ParseModifier();
            while(TryConsume(Tokens.Combining, out var combining))
            {
                filter = new MultiEntityFilter(filter, ParseModifier()) { Type = combining.Value switch
                {
                    "and" => MultiEntityFilter.FilterType.And,
                    "or" => MultiEntityFilter.FilterType.Or,
                    "xor" => MultiEntityFilter.FilterType.Xor,
                    _ => MultiEntityFilter.FilterType.And,
                }
                };
            }
            return filter;
        }
        public IEntityFilter ParseNot()
        {
            bool negate = TryConsume(Tokens.Not, out _);
            var filter = ParseModifier();
            if(negate)
            {
                filter = new EntityFilterNegate(filter);
            }
            return filter;
        }
        public IEntityFilter ParseModifier()
        {
            if(TryConsume(Tokens.GreaterThanSign, out _))
            {
                bool alsoEquals = TryConsume(Tokens.EqualsSign, out _);
                var number = ParseNumber();
                return new EntityContentComparisonFilter(p =>
                {
                    return p.Value switch
                    {
                        int     v => alsoEquals ? v >= number: v > number,
                        uint    v => alsoEquals ? v >= number: v > number,
                        sbyte   v => alsoEquals ? v >= number: v > number,
                        byte    v => alsoEquals ? v >= number: v > number,
                        short   v => alsoEquals ? v >= number: v > number,
                        ushort  v => alsoEquals ? v >= number: v > number,
                        long    v => alsoEquals ? v >= number: v > number,
                        ulong   v => alsoEquals ? v >= number: v > number,
                        float   v => alsoEquals ? v >= number: v > number,
                        double  v => alsoEquals ? v >= number: v > number,
                        _ => false
                    };
                });
            } else if(TryConsume(Tokens.LessThanSign, out _))
            {
                bool alsoEquals = TryConsume(Tokens.EqualsSign, out _);
                var number = ParseNumber();
                return new EntityContentComparisonFilter(p =>
                {
                    return p.Value switch
                    {
                        int     v => alsoEquals ? v <= number : v < number,
                        uint    v => alsoEquals ? v <= number : v < number,
                        sbyte   v => alsoEquals ? v <= number : v < number,
                        byte    v => alsoEquals ? v <= number : v < number,
                        short   v => alsoEquals ? v <= number : v < number,
                        ushort  v => alsoEquals ? v <= number : v < number,
                        long    v => alsoEquals ? v <= number : v < number,
                        ulong   v => alsoEquals ? v <= number : v < number,
                        float   v => alsoEquals ? v <= number : v < number,
                        double  v => alsoEquals ? v <= number : v < number,
                        _ => false
                    };
                });
            } else if(TryConsume(Tokens.Boolean, out var boolToken))
            {
                var value = bool.Parse(boolToken.Value);
                return new EntityContentComparisonFilter(p =>
                {
                    if (p.Value is not bool b) return false;
                    return b == value;
                });
            }
            else
            {
                if (TryConsume(Tokens.Tilde, out _))
                {
                    var textMatch = ParseString();
                    return new EntityContentComparisonFilter(p =>
                    {
                        if (p.Value is not string s) return false;
                        bool contains = s.Contains(textMatch, StringComparison.InvariantCultureIgnoreCase);
                        return contains;
                    });
                } else if(Test(Tokens.DoubleQuote))
                {
                    var textMatch = ParseString();
                    return new EntityContentComparisonFilter(p =>
                    {
                        if (p.Value is not string s) return false;
                        return s == textMatch;
                    });
                } else if (Test(Tokens.OpenBracket))
                {
                    return ParseMain();
                } else if(Test(Tokens.IntegerSpecifier) || Test(Tokens.Number) || Test(Tokens.Dot))
                {
                    var number = ParseNumber();
                    return new EntityContentComparisonFilter(p =>
                    {
                        return p.Value switch
                        {
                            int v       => v == number,
                            uint v      => v == number,
                            sbyte v     => v == number,
                            byte v      => v == number,
                            short v     => v == number,
                            ushort v    => v == number,
                            long v      => v == number,
                            ulong v     => v == number,
                            float v     => v == number,
                            double v    => v == number,
                            _ => false
                        };
                    });
                }
                throw new Exception("Could not find valid value part.");
            }
        }
        public string ParseString()
        {
            Consume(Tokens.DoubleQuote);
            StringBuilder text = new StringBuilder();
            while(!TryConsume(Tokens.DoubleQuote, out _))
            {
                if(TryConsume(Tokens.StringPart, out var stringPart))
                {
                    text.Append(stringPart.Value);
                }
                else if(TryConsume(Tokens.StringEscapeSequence, out var stringEscape))
                {
                    text.Append(stringEscape.Value[1] switch
                    {
                        'n' => '\n',
                        't' => '\t',
                        'r' => '\r',
                        '\\' => '\\',
                        '"' => '"',
                        '0' => '\0',
                        _ => throw new Exception("Invalid string escape sequence")
                    });
                }
                else
                {
                    throw new Exception("Invalid string token.");
                }
            }
            return text.ToString();
        }
        public double ParseNumber()
        {
            if(TryConsume(Tokens.IntegerSpecifier, out var specifier))
            {
                var intToken = Consume(Tokens.Number);
                var intText = intToken.Value.Replace("_", "");
                return specifier.Value[1] switch
                {
                    'x' => Convert.ToInt32(intText, 16),
                    'o' => Convert.ToInt32(intText, 8),
                    'b' => Convert.ToInt32(intText, 2),
                    _ => int.Parse(intText)
                };
            }
            else if(TryConsume(Tokens.Dot, out _))
            {
                var floatToken = Consume(Tokens.Number);
                var floatText = "." + floatToken.Value.Replace("_", "");
                return double.Parse(floatText);
            }
            else
            {
                var intToken = Consume(Tokens.Number);
                var intText = intToken.Value.Replace("_", "");
                if(!TryConsume(Tokens.Dot, out _))
                {
                    return int.Parse(intText);
                }
                else
                {
                    if(TryConsume(Tokens.Number, out var fractToken))
                    {
                        var fractText = fractToken.Value.Replace("_", "");
                        return double.Parse($"{intText}.{fractText}");
                    }
                    else
                    {
                        return double.Parse(intText);
                    }
                }
            }
        }
    }
    class TextWindow
    {
        char[] text;
        int position;
        public bool IsAtEnd => position >= text.Length;
        public TextWindow(string text)
        {
            this.text = text.ToCharArray();
            position = 0;
        }
        public char Read() => text[position++];
        public char Peek() => text[position];
        public char Peek(int i) => text[position+i];
        public ReadOnlySpan<char> Read(int i, out Range range)
        {
            range = position..(position + i);
            position += i;
            return text[range];
        }
        public ReadOnlySpan<char> ReadWhile(Func<char, bool> condition, out Range range)
        {
            int start = position;
            while (!IsAtEnd && condition(text[position]))
            {
                position++;
            }
            range = start..position;
            return text[range];
        }
        public ReadOnlySpan<char> ReadWhile(Func<char, char, bool> condition, out Range range)
        {
            int start = position;
            var lastChar = '\0';
            while (!IsAtEnd && condition(text[position], lastChar))
            {
                lastChar = text[position];
                position++;
            }
            range = start..position;
            return text[range];
        }
    }
}