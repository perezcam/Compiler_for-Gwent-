using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection.Emit;

public abstract class ASTNode{}
public class ProgramNode : ASTNode
{
    public List<ASTNode> Statements { get; } = new List<ASTNode>();
}
public class EffectNode : ASTNode
{
    public string ?Name { get; set; }
    public List<AssignmentNode> assignments = new List<AssignmentNode>();

    public ActionBlockNode ?Action { get; set; }
}
public class CardNode : ASTNode
{
    public string ?Name { get; set; }
    public string ?Type { get; set; }
    public List<string> Effect  = new List<string>();
    public string ?Faction{get;set;}
    public ExpressionNode ?Power{get;set;}
    public List<string> Range = new List<string>();
    //Agregar el OnActivation block
    //crear el nodo On Activation Block
}
public class ActivationBlockNode: ASTNode
{
    //preparar el activation block nmode
}
public class ActionBlockNode : ASTNode
{
    //analizar si necesito recibir los parametrps del bloque Accion y en cAO DE NECESITARLOS CREAR LAETRUCTURA
    public List<StatementNode> Statements { get; } = new List<StatementNode>();
    public AssignmentNode target = new AssignmentNode();
    public AssignmentNode context = new AssignmentNode();
}
public abstract class StatementNode : ASTNode {}
public class ForStatementNode : StatementNode
{
    public string ?Variable { get; set; }
    public string ?Collection { get; set; }
    public BlockNode ?Body { get; set; }
}
public class WhileStatementNode : StatementNode
{
    public ExpressionNode ?Condition { get; set; }
    public BlockNode ?Body { get; set; }
}
public class AssignmentNode : StatementNode
{
    public string ?Variable { get; set;}
    public TokenType ? type{get;set;}
    public ExpressionNode ?Value { get; set;}
}
public abstract class ExpressionNode : ASTNode {}
public class ExpressionStatementNode : StatementNode
{
    public ExpressionNode ?Expression { get; set; }
}

public class IdentifierNode : ExpressionNode
{
    public string ?Name {get; set;}
}
public class UnaryExpressionNode : ExpressionNode
{
    public TokenType Operator {get;set;}
    public ExpressionNode ?Operand {get;set;}
    public bool Atend {get;set;}
}
public class BinaryExpressionNode : ExpressionNode
{
    public static Dictionary<TokenType, int> Levels = new Dictionary<TokenType, int>
    {
        { TokenType.Plus, 1 },
        { TokenType.Minus, 1 },
        { TokenType.Multiply, 2 },
        { TokenType.Divide, 2 },
        { TokenType.And, 1 },
        { TokenType.Or, 1 },
        { TokenType.Equals, 0 },
        { TokenType.EqualValue,1},
        { TokenType.NotEquals, 1 },
        { TokenType.LessThan, 1 },
        { TokenType.GreaterThan, 1 },
        { TokenType.LessThanOrEquals, 1 },
        { TokenType.GreaterThanOrEquals, 1 }
    };
    public ExpressionNode ?Left { get; set; }
    public TokenType Operator { get; set; }
    public ExpressionNode ?Right { get; set; }
}
public class BooleanLiteralNode : ExpressionNode
{
    public bool Value { get; set; }
}

public class BooleanBinaryExpressionNode : ExpressionNode
{
    public ExpressionNode ?Left { get; set; }
    public TokenType Operator { get; set; }
    public ExpressionNode ?Right { get; set; }
}
public class NumberNode : ExpressionNode
{
    public int Value { get; set; }
}
public class DataTypeNode: ExpressionNode
{
    public TokenType type {get;set;}
}
public class StringNode : ExpressionNode
{
    public string ?Value { get; set; }
}

public class BlockNode : ASTNode
{
    public List<StatementNode> Statements { get; } = new List<StatementNode>();
}
public class PropertyAccessNode : ExpressionNode
{
    public ExpressionNode ?Target { get; set; }
    public string ?PropertyName { get; set; }
}
public class CompoundAssignmentNode : AssignmentNode
{
    public ExpressionNode ?Target { get; set; }
    public TokenType Operator { get; set; }
    public ExpressionNode ?Value { get; set; }
}
public class MethodCallNode : ExpressionNode
{
    public ExpressionNode ?Target { get; set; }
    public string ?MethodName { get; set; }
    public List<ExpressionNode> ?Arguments { get; set; }
}
public class Parser
{
    private List<Token> tokens;
    private int position;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
        this.position = 0;
    }
    private Token? CurrentToken => position < tokens.Count ? tokens[position] : null;
    private Token? GetNextToken()
    {
        if(position+1 < tokens.Count)
            return tokens[position+1];
        return null;
    }    
    private Token Match(TokenType type)
    {
        Token ?token = CurrentToken;
        if (token != null && token.Type == type)
        {
            position++;
            return token;
        }
        else
        {   
            throw new Exception($"Esperaba un {type} en la linea {CurrentToken?.Row} pero recibio {CurrentToken?.Type}");
        }
    }
    public ProgramNode Parse()
    {
        ProgramNode program = new ProgramNode();
        while (CurrentToken != null)
        {
            if (CurrentToken.Type == TokenType.Effect)
            {
                program.Statements.Add(ParseEffect());
            }
            else if (CurrentToken.Type == TokenType.Card)
            {
                program.Statements.Add(ParseCard());
            }
            else
            {
                throw new Exception($"Recibio {CurrentToken.Type} pero esperaba effect o card");
            }
        }
        return program;
    }
    private EffectNode ParseEffect()
    {
        EffectNode effect = new EffectNode();
        Match(TokenType.Effect);
        Match(TokenType.OpenBrace);

        while (CurrentToken.Type != TokenType.CloseBrace)
        {
            Token propertyToken = CurrentToken;
            switch (propertyToken.Type)
            {
                case TokenType.Name:
                    Match(TokenType.Name);
                    Match(TokenType.Colon);
                    effect.Name = Match(TokenType.String).Value;
                    Match(TokenType.Comma);
                    break;

                case TokenType.Params:
                    Match(TokenType.Params);
                    Match(TokenType.Colon);
                    Match(TokenType.OpenBrace);
                    while (CurrentToken != null && CurrentToken.Type != TokenType.CloseBrace)
                    {
                       effect.assignments.Add((AssignmentNode)ParseAssignment());
                    }
                    Match(TokenType.CloseBrace);
                    Match(TokenType.Comma);
                    break;
                
                case TokenType.Action:
                    Match(TokenType.Action);
                    Match(TokenType.Colon);
                    effect.Action = ParseActionBlock();
                    break;

                default:
                    throw new Exception($"Token inesperado en las valores de Effect, se recibio {CurrentToken.Type}, en la fila {CurrentToken.Row}");
            }
        }
        Match(TokenType.CloseBrace);
        
        return effect;
    }
    private CardNode ParseCard()
    {
        CardNode card = new CardNode();
        Match(TokenType.Card);
        Match(TokenType.OpenBrace);
        while (CurrentToken.Type != TokenType.CloseBrace)
        {
            Token propertyToken = CurrentToken;
            switch (propertyToken.Type)
            {
                case TokenType.Name:
                    Match(TokenType.Name);
                    Match(TokenType.Colon);
                    card.Name = Match(TokenType.String).Value;
                    break;

                case TokenType.Type:
                    Match(TokenType.Type);
                    Match(TokenType.Colon);
                    card.Type = Match(TokenType.String).Value;
                    break;
                
                case TokenType.EffectKeyword:
                    Match(TokenType.EffectKeyword);
                    Match(TokenType.Colon);
                    card.Effect.Add(Match(TokenType.String).Value);
                    break;
                case TokenType.Range:
                    Match(TokenType.Range);
                    Match(TokenType.Colon);
                    card.Range.Add(Match(TokenType.String).Value);
                    break;
                case TokenType.Power:
                    Match(TokenType.Power);
                    Match(TokenType.Colon);
                    card.Power = ParseExpression();
                    break;
                //Falta agregar aqui el OnActivation Block
                default:
                    throw new Exception("Token inesperado en las valores de Effect");
            }
        }
        return card;
    }
    private ActionBlockNode ParseActionBlock()
    {
        ActionBlockNode actionBlock = new ActionBlockNode();
        Match(TokenType.OpenParen);
        actionBlock.target.Variable = Match(TokenType.Identifier).Value;
        Match(TokenType.Comma);
        actionBlock.context.Variable = Match(TokenType.Identifier).Value;
        Match(TokenType.CloseParen);
        Match(TokenType.Arrow);
        
        Match(TokenType.OpenBrace);
        while (CurrentToken != null && CurrentToken.Type != TokenType.CloseBrace)
        {
            actionBlock.Statements.Add(ParseStatement());
        }
        // Match(TokenType.CloseBrace);
        return actionBlock;
    }
    private StatementNode ParseStatement()
    {
        TokenType nextToken = GetNextToken()?.Type ?? TokenType.Unknown;
        if (CurrentToken?.Type == TokenType.For)
        {
            return ParseForStatement();
        }
        else if (CurrentToken?.Type == TokenType.While)
        {
            return ParseWhileStatement();
        }
        else if (nextToken == TokenType.Equals || nextToken == TokenType.PlusEqual || nextToken == TokenType.MinusEqual || nextToken == TokenType.MultiplyEqual || nextToken == TokenType.DivideEqual)
        {
            return ParseAssignment();
        }
        else if (CurrentToken?.Type == TokenType.Identifier)
        {
            // Puede ser una llamada a método o una propiedad con acceso
            ExpressionNode expr = ParseExpression();
            if (expr is MethodCallNode)
            {
                return new ExpressionStatementNode { Expression = expr };
            }
            else if ( expr is PropertyAccessNode)
            {
                if (IsAssignmentOperator(CurrentToken.Type))
                {
                    TokenType op = CurrentToken.Type;
                    Match(op);
                    CompoundAssignmentNode left = new CompoundAssignmentNode
                    {
                        Target = expr,
                        Operator = GetSimpleOperator(op),
                        Value = ParseExpression(),
                    };
                    Match(TokenType.Comma);
                    return left;
                }
                return new ExpressionStatementNode{ Expression = expr };
            }
        }
        throw new Exception($"Token inesperado en Parse Statement: {CurrentToken?.Type}");
    }

    private ForStatementNode ParseForStatement()
    {
        ForStatementNode forStatement = new ForStatementNode();
        Match(TokenType.For);
        Match(TokenType.OpenParen);
        forStatement.Variable = Match(TokenType.Identifier).Value;
        Match(TokenType.In);
        forStatement.Collection = Match(TokenType.Identifier).Value;
        Match(TokenType.CloseParen);
        forStatement.Body = ParseBlock();
        return forStatement;
    }
    private WhileStatementNode ParseWhileStatement()
    {
        WhileStatementNode whileStatement = new WhileStatementNode();
        Match(TokenType.While);
        Match(TokenType.OpenParen);
        whileStatement.Condition = ParseExpression();
        Match(TokenType.CloseParen);
        whileStatement.Body = ParseBlock();
        return whileStatement;
    }
    private StatementNode ParseAssignment()
    {
        ExpressionNode left = ParsePrimaryExpression();
        if (left is IdentifierNode || left is PropertyAccessNode)
        {
            TokenType op = CurrentToken?.Type ?? TokenType.Unknown;
            if (IsAssignmentOperator(op))
            {
                Match(op);
                ExpressionNode right = ParseExpression();
                if (op == TokenType.Equals || op == TokenType.Colon)
                {
                    Match(TokenType.Comma);
                    return new AssignmentNode
                    {
                        Variable = GetVariableFromExpression(left),
                        Value = right
                    };
                }
                else
                {
                    Match(TokenType.Comma);
                    return new CompoundAssignmentNode
                    {
                        Target = left,
                        Operator = GetSimpleOperator(op),
                        Value = right
                    };
                }
            }
        }
        throw new Exception("Asignacion no valida");
    }

    private ExpressionNode ParseExpression(int precedence = 0)
    {
        ExpressionNode left = ParsePrimaryExpression();

        while (CurrentToken != null && IsOperator(CurrentToken.Type) && BinaryExpressionNode.Levels[CurrentToken.Type] > precedence)
        {
            TokenType op = CurrentToken.Type;
            Match(op);
            ExpressionNode right = ParseExpression(BinaryExpressionNode.Levels[op]);

             if (IsBooleanOperator(op))
            {
                left = new BooleanBinaryExpressionNode { Left = left, Operator = op, Right = right };
            }
            else
            {
                left = new BinaryExpressionNode { Left = left, Operator = op, Right = right };
            }
        }
        return left;
    }

    private bool IsAssignmentOperator(TokenType type)
    {
        return type == TokenType.Colon ||type == TokenType.Equals || type == TokenType.PlusEqual || type == TokenType.MinusEqual || type == TokenType.MultiplyEqual || type == TokenType.DivideEqual;
    }

    private TokenType GetSimpleOperator(TokenType compoundOperator)
    {
        switch (compoundOperator)
        {
            case TokenType.PlusEqual: return TokenType.Plus;
            case TokenType.MinusEqual: return TokenType.Minus;
            case TokenType.MultiplyEqual: return TokenType.Multiply;
            case TokenType.DivideEqual: return TokenType.Divide;
            default: throw new Exception("Operacion Compuesta Invalida");
        }
    }

    private string GetVariableFromExpression(ExpressionNode expr)
    {
        if (expr is IdentifierNode idNode)
        {
            return idNode.Name;
        }
        else if (expr is PropertyAccessNode propNode)
        {
            return $"{GetVariableFromExpression(propNode.Target)}.{propNode.PropertyName}";
        }
        else
        {
            throw new Exception("Parte izq invalida");
        }
    }

    private bool IsBooleanOperator(TokenType type)
    {
        return type == TokenType.And || type == TokenType.Or || type == TokenType.Equals || type == TokenType.NotEquals || type == TokenType.EqualValue||
            type == TokenType.LessThan || type == TokenType.GreaterThan || type == TokenType.LessThanOrEquals || type == TokenType.GreaterThanOrEquals;
    }
    private bool IsOperator(TokenType type)
    {
        return BinaryExpressionNode.Levels.ContainsKey(type);
    }

    private ExpressionNode ParsePrimaryExpression()
    {
        ExpressionNode expr = ParseBasicPrimaryExpression();
        
        while (CurrentToken != null && CurrentToken.Type == TokenType.Dot)
        {
            Match(TokenType.Dot);
            string propertyName = Match(TokenType.Identifier).Value;
            
            if (CurrentToken?.Type == TokenType.OpenParen)
            {
                Match(TokenType.OpenParen);
                List<ExpressionNode> arguments = new List<ExpressionNode>();
                if (CurrentToken?.Type != TokenType.CloseParen)
                {
                    do
                    {
                        arguments.Add(ParseExpression());
                    } while (Match(TokenType.Comma) != null);
                }
                Match(TokenType.CloseParen);
                expr = new MethodCallNode { Target = expr, MethodName = propertyName, Arguments = arguments };
            }
            else
            {
                expr = new PropertyAccessNode { Target = expr, PropertyName = propertyName };
            }
        }
        
        return expr;
    }

    private ExpressionNode ParseBasicPrimaryExpression()
    {
        if (CurrentToken?.Type == TokenType.Minus ||CurrentToken?.Type == TokenType.PlusPlus ||CurrentToken?.Type == TokenType.MinusMinus)
        {
            TokenType op = CurrentToken.Type;
            Match(op);
            ExpressionNode operand = ParsePrimaryExpression();
            return new UnaryExpressionNode { Operator = op, Operand = operand, Atend = false};
        }
        if (CurrentToken?.Type == TokenType.Identifier)
        {
            string ID = Match(TokenType.Identifier).Value;
            IdentifierNode Id = new IdentifierNode { Name = ID} ;
            if (CurrentToken.Type == TokenType.MinusMinus || CurrentToken.Type == TokenType.PlusPlus)
            {
                Token op = CurrentToken;
                Match(CurrentToken.Type);
                return new UnaryExpressionNode{Operator = op.Type, Operand = Id, Atend = true};
            }
            return Id;

        }
        else if (CurrentToken?.Type == TokenType.Number)
        {
            return new NumberNode { Value = int.Parse(Match(TokenType.Number).Value) };
        }
        else if (CurrentToken?.Type == TokenType.String)
        {
            return new StringNode { Value = Match(TokenType.String).Value };
        }
        else if (CurrentToken?.Type == TokenType.True || CurrentToken?.Type == TokenType.False)
        {
            BooleanLiteralNode boolean = new BooleanLiteralNode { Value = CurrentToken.Type == TokenType.True };
            Match(CurrentToken.Type);
            return boolean;
        }
        else if (CurrentToken?.Type == TokenType.OpenParen)
        {
            Match(TokenType.OpenParen);
            var expr = ParseExpression();
            Match(TokenType.CloseParen);
            return expr;
        }
        else if (CurrentToken?.Type == TokenType.NumberType || CurrentToken?.Type == TokenType.StringType || CurrentToken?.Type == TokenType.BoolType)
        {
            DataTypeNode expresion = new DataTypeNode();
            expresion.type = Match(CurrentToken.Type).Type;
            return expresion;
        }
        //ver si tengo q parsear expresion tipo metodo aqui
        else
        {
            throw new Exception($"Se esperaba expresión primaria y se recibió: {CurrentToken?.Type}");
        }
    }
    private BlockNode ParseBlock()
    {
        BlockNode block = new BlockNode();
        //Bloque de mas de una instruccion con llave
        if (CurrentToken?.Type == TokenType.OpenBrace)
        {
            Match(TokenType.OpenBrace);
            while (CurrentToken != null && CurrentToken.Type != TokenType.CloseBrace)
            {
                block.Statements.Add(ParseStatement());
            }
            Match(TokenType.CloseBrace);
            Match(TokenType.Comma);                                     
            return block;
        }
        //Bloque de instruccion unica
        while (CurrentToken != null && CurrentToken.Type != TokenType.Comma)
        {
            if(CurrentToken.Type == TokenType.CloseBrace)
            {
                Match(TokenType.CloseBrace);
                break;
            }
            block.Statements.Add(ParseStatement());
        }
        Match(TokenType.Comma);
        return block;
    }

}

    // private MethodCallNode ParseMethodCall()
    // {
    //     MethodCallNode methodCall = new MethodCallNode();
    //     while(GetNextToken()?.Type == TokenType.Dot || IsContextMethod())
    //     {
    //         if (IsContextMethod())
    //         {
    //             MethodNode method  = new MethodNode();
    //             method.Signature = Match(CurrentToken.Type);
    //             Match(TokenType.OpenParen);
    //             if(CurrentToken.Type != TokenType.CloseParen)
    //                 method.Params = ParseExpression();
    //             Match(TokenType.CloseParen);

    //             methodCall.context?.Add(method);
    //         }
    //         else
    //             methodCall.context?.Add(ParseExpression());
    //         Match(TokenType.Dot);
    //     } 
    //     while(CurrentToken?.Type != TokenType.DotCom)
    //     {
    //         MethodNode method = new MethodNode();
    //         method.Signature = Match(CurrentToken.Type);
    //         Match(TokenType.OpenParen);
    //         if(CurrentToken.Type != TokenType.CloseParen)
    //             method.Params = ParseExpression();
    //         Match(TokenType.CloseParen);
    //         methodCall.actions?.Add(method);
    //     }
    //     return methodCall;
    // }
    // private bool IsContextMethod()
    // {
    //     return CurrentToken?.Type == TokenType.HandOfPlayer|| CurrentToken?.Type == TokenType.DeckOfPlayer || CurrentToken?.Type == TokenType.GraveyardOfPlayer || CurrentToken?.Type == TokenType.FieldOfPlayer;
    // }