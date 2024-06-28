using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net.Http.Headers;
public enum TokenType
{
    // Keywords
    Effect,Card,Name,Params,Action,Type,Faction,Power,Range,OnActivation,EffectKeyword,Selector,
    PostAction,Source,Single,Predicate,For,In,While,Hand,Deck,Board,HandOfPlayer,FieldOfPlayer,GraveyardOfPlayer,DeckOfPlayer,
    TriggerPlayer,Find,Push,SendBottom,Pop,Remove,Shuffle, 

    // Data Types
    NumberType,StringType,BoolType,
    // Symbols
    OpenParen,CloseParen,OpenBrace,CloseBrace,OpenBracket,CloseBracket,Colon,Arrow,Comma,Equals,PlusPlus,MinusMinus,Dot,DotCom,
    //Math Operator
    Plus,Minus,Multiply,Divide,MinusEqual,PlusEqual,MultiplyEqual,DivideEqual,
    //Boolean Operator
    And,Or,EqualValue,NotEquals,LessThan,GreaterThan,LessThanOrEquals,
    GreaterThanOrEquals,True,False,
    // Identifiers and Literals
    Identifier,Number,String,

    // Whitespace
    Whitespace,

    // Comments
    LineComment,
    BlockComment,

    // Unknown
    Unknown
}
public class Token
{
    public TokenType Type { get; }
    public string Value { get; }
    public int Row { get; }
    public int Column { get; }

    public Token(TokenType type, string value, int row, int column)
    {
        Type = type;
        Value = value;
        Row = row;
        Column = column;
    }

    public override string ToString()
    {
        return $"{Type}: {Value} (Row: {Row}, Column: {Column})";
    }
}

public class Lexer
{
    private static readonly Dictionary<TokenType, string> TokenPatterns = new Dictionary<TokenType, string>
    {
        // Keywords
        { TokenType.Effect, @"\beffect\b" },
        { TokenType.Card, @"\bcard\b" },
        { TokenType.Name, @"\bName\b" },
        { TokenType.Params, @"\bParams\b" },
        { TokenType.Action, @"\bAction\b" },
        { TokenType.Type, @"\bType\b" },
        { TokenType.Faction, @"\bFaction\b" },
        //{ TokenType.Power, @"\bPower\b" },
        { TokenType.Range, @"\bRange\b" },
        { TokenType.OnActivation, @"\bOnActivation\b" },
        { TokenType.EffectKeyword, @"\bEffect\b" },
        { TokenType.Selector, @"\bSelector\b" },
        { TokenType.PostAction, @"\bPostAction\b" },
        { TokenType.Source, @"\bSource\b" },
        { TokenType.Single, @"\bSingle\b" },
        { TokenType.Predicate, @"\bPredicate\b" },
        { TokenType.For, @"\bfor\b" },
        { TokenType.In, @"\bin\b" },
        { TokenType.While, @"\bwhile\b" },
        { TokenType.HandOfPlayer, @"\bHandOfPlayer"},
        { TokenType.DeckOfPlayer, @"\bDeckOfPlayer"},
        { TokenType.FieldOfPlayer, @"\bFieldOfPlayer"},
        { TokenType.GraveyardOfPlayer, @"\bGraveyardOfPlayer"},    
        { TokenType.Hand, @"\bhand\b" },
        { TokenType.Deck, @"\bdeck\b" },
        { TokenType.Board, @"\bboard\b" },
        { TokenType.True, @"\btrue\b" },
        { TokenType.False, @"\bfalse\b" },
        { TokenType.TriggerPlayer, @"\bTriggerPlayer\b" },
        { TokenType.Find, @"\bFind\b" },
        { TokenType.Push, @"\bPush\b" },
        { TokenType.SendBottom, @"\bSendBottom\b" },
        { TokenType.Pop, @"\bPop\b" },
        { TokenType.Remove, @"\bRemove\b" },
        { TokenType.Shuffle, @"\bShuffle\b" },
        
        // Data Types
        { TokenType.NumberType, @"\bNumber\b" },
        { TokenType.StringType, @"\bString\b" },
        { TokenType.BoolType, @"\bBool\b"},

        // Booleans
        { TokenType.And, @"&&" },
        { TokenType.Or, @"\|\|"},
        { TokenType.EqualValue, @"==" },
        { TokenType.NotEquals, @"!=" },
        { TokenType.LessThan, @"<" }, 
        { TokenType.GreaterThan, @">" },
        { TokenType.LessThanOrEquals, @"<=" },
        { TokenType.GreaterThanOrEquals, @">=" },
        // Symbols
        { TokenType.OpenParen, @"\(" },
        { TokenType.CloseParen, @"\)" },
        { TokenType.OpenBrace, @"\{" },
        { TokenType.CloseBrace, @"\}" },
        { TokenType.OpenBracket, @"\[" },
        { TokenType.CloseBracket, @"\]" },
        { TokenType.Colon, @":" },
        { TokenType.DotCom, @"\;"},
        { TokenType.Comma, @"," },
        { TokenType.Arrow, @"=>" },
        { TokenType.Equals, @"=" },
        { TokenType.PlusPlus,@"\+\+"},
        { TokenType.MinusMinus,@"\-\-"},
        { TokenType.Dot, @"\."},
        
        //MathOperator
        { TokenType.PlusEqual, @"\+="},
        { TokenType.MinusEqual, @"\-="},
        { TokenType.MultiplyEqual, @"\*="},
        { TokenType.DivideEqual, @"/="},
        { TokenType.Plus, @"\+" },
        { TokenType.Minus, @"\-" },
        { TokenType.Multiply, @"\*" },
        { TokenType.Divide, @"\/" },

        // Identifiers
        { TokenType.Identifier, @"[a-zA-Z_][\w]*" },

        // Numbers (integers and decimals)
        { TokenType.Number, @"\b\d+(\.\d+)?\b" },

        // Strings (double-quoted)
        { TokenType.String, @"""[^""]*""" },


        // Whitespace
        { TokenType.Whitespace, @"\s+" },

        // Comments
        { TokenType.LineComment, @"//[^\n]*" },
        { TokenType.BlockComment, @"/\*.*?\*/" }
    };

    private readonly Regex _tokenRegex;

    public Lexer()
    {
        // Crear una expresión regular combinada para todos los patrones
        string pattern = string.Join("|", TokenPatterns.Select(kv => $"(?<{kv.Key}>{kv.Value})"));
        _tokenRegex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Singleline);
    }

    public List<Token> Tokenize(string input)
    {
        List<Token> tokens = new List<Token>();
        var matches = _tokenRegex.Matches(input);

        int row = 1;
        int column = 1;

        foreach (Match match in matches)
        {
            foreach (var groupName in TokenPatterns.Keys.Select(x => x.ToString()))
            {
                if (match.Groups[groupName].Success)
                {
                    TokenType tokenType = (TokenType)Enum.Parse(typeof(TokenType), groupName);
                    string value = match.Value;

                    // Calcular la posición del token
                    int tokenRow = row;
                    int tokenColumn = column;

                    // Actualizar la fila y columna
                    foreach (char c in value)
                    {
                        if (c == '\n')
                        {
                            row++;
                            column = 1;
                        }
                        else
                        {
                            column++;
                        }
                    }
                    // Ignorar comentarios y espacios en blanco
                    if (tokenType != TokenType.Whitespace && tokenType != TokenType.LineComment && tokenType != TokenType.BlockComment)
                    {
                        tokens.Add(new Token(tokenType, value, tokenRow, tokenColumn));
                    }
                    break;
                }
            }
        }
        return tokens;
    }
} 
