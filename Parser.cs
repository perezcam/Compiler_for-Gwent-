using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection.Emit;
using Interpeter;

public abstract class ASTNode{
    public abstract void Accept(IVisitor visitor,Scope scope);
    public int priority{get;set;}
}
public class ProgramNode : ASTNode
{
    public List<ASTNode> Statements { get;set; } = new List<ASTNode>();
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class EffectNode : ASTNode
{
    public EffectNode()
    {
        priority = 1;
    } 
    public string ?Name { get; set; }
    public List<AssignmentNode> Params = new List<AssignmentNode>();
    public ActionBlockNode ?Action {get;set;}
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class CardNode : ASTNode
{
    public CardNode()
    {
        priority = 0;
    }
    public ExpressionNode ?Name { get; set; } 
    public string ?Type { get; set; }
    public List<string> Effect  = new List<string>();
    public string ?Faction{get;set;}
    public ExpressionNode ?Power{get;set;}
    public List<string> Range = new List<string>();
    public List<ActivationBlockNode> OnActivationBlock = new List<ActivationBlockNode>();
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class ActivationBlockNode: ASTNode
{
    public ActivationBlockNode(ActivationBlockNode? parent = null)
    {   
        this.parent = parent!;
    }
    public ActivationBlockNode parent;
    public EffectBuilderNode ?effect {get;set;}
    public SelectorNode ?selector {get;set;} 
    public ActivationBlockNode ?postAction {get;set;}
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class ErrorBlockNode : StatementNode
{
    public string Message { get; }
    public List<Token> Tokens { get; }

    public ErrorBlockNode(string message, List<Token> tokens)
    {
        Message = message;
        Tokens = tokens;
    }

    public override string ToString()
    {
        return $"{Message}: {string.Join(" ", Tokens.Select(t => t.Value))}";
    }
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}

public class EffectBuilderNode:StatementNode
{
    public string ?Name{get;set;}
    public List<StatementNode> assingments{get;set;} = new List<StatementNode>();
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class SelectorNode:StatementNode
{
    public IdentifierNode ?source{get;set;}
    public ExpressionNode ?single{get;set;}
    public PredicateFunction ?predicate{get;set;} 
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class PredicateFunction: ExpressionNode
{
    public IdentifierNode ?target {get;set;}
    public ExpressionNode ?filter {get;set;}
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class ActionBlockNode : ASTNode
{
    public List<StatementNode> Statements { get; } = new List<StatementNode>();
    public AssignmentNode target = new AssignmentNode();
    public AssignmentNode context = new AssignmentNode();
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public abstract class StatementNode : ASTNode {}
public class ForStatementNode : StatementNode
{
    public ExpressionNode ?Variable { get; set; }
    public ExpressionNode ?Collection { get; set; }
    public BlockNode ?Body { get; set; }
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class WhileStatementNode : StatementNode
{
    public ExpressionNode ?Condition { get; set; }
    public BlockNode ?Body { get; set; }
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class AssignmentNode : StatementNode
{
    public ExpressionNode ?Variable { get; set;}
    public TokenType ? type{get;set;}
    public ExpressionNode ?Value { get; set;}
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public abstract class ExpressionNode : ASTNode {}
public class AccesExpressionNode : StatementNode
{
    public ExpressionNode ?Expression { get; set; }
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}

public class IdentifierNode : ExpressionNode
{
    public IdentifierNode(string name)
    {
        Name = name;
    }
    public string ?Name {get; set;}
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class UnaryExpressionNode : ExpressionNode
{
    public TokenType Operator {get;set;}
    public ExpressionNode ?Operand {get;set;}
    public bool Atend {get;set;}
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class MathBinaryExpressionNode : ExpressionNode
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
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class BooleanLiteralNode : ExpressionNode
{
    public bool Value { get; set; }
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}

public class BooleanBinaryExpressionNode : ExpressionNode
{
    public ExpressionNode ?Left { get; set; }
    public TokenType Operator { get; set; }
    public ExpressionNode ?Right { get; set; }
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class NumberNode : ExpressionNode
{
    public long Value { get; set; }
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class DataTypeNode: ExpressionNode
{
    public TokenType type {get;set;}
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class StringNode : ExpressionNode
{
    public string ?Value { get; set; }
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}

public class BlockNode : ASTNode
{
    public List<StatementNode> Statements { get; } = new List<StatementNode>();
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class PropertyAccessNode : ExpressionNode
{
    public ExpressionNode ?Target { get; set;}
    public IdentifierNode ?PropertyName { get; set; }
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class CollectionIndexingNode : ExpressionNode
{
    public ExpressionNode ?Collection { get; set; }
    public ExpressionNode ?Index { get; set; }
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }

}
public class CompoundAssignmentNode : AssignmentNode
{
    public ExpressionNode ?Target { get; set; }
    public TokenType Operator { get; set; }
    public ExpressionNode ?value { get; set; }
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class MethodCallNode : ExpressionNode
{
    public ExpressionNode ?Target {get;set;}
    public IdentifierNode ?MethodName {get;set;}
    public List<ExpressionNode> ?Arguments {get;set;}
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
}
public class ConcatExpresion: ExpressionNode
{
    public ExpressionNode ?right{get;set;}
    public ExpressionNode ?left {get;set;}
    public bool IsComp{get;set;}
    public override void Accept(IVisitor visitor,Scope scope)
    {
        visitor.Visit(this,scope);
    }
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
                throw new Exception($"Recibio {CurrentToken.Type} pero esperaba effect o card en la linea {CurrentToken.Row}");
            }
        }
        program.Statements = program.Statements.OrderByDescending((x)=>x.priority).ToList();
        return program;
    }
    private EffectNode ParseEffect()
    {
        EffectNode effect = new EffectNode();
        Match(TokenType.Effect);
        Match(TokenType.OpenBrace);

        while (CurrentToken!.Type != TokenType.CloseBrace)
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
                       effect.Params.Add((AssignmentNode)ParseAssignment());
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
        while (CurrentToken!.Type != TokenType.CloseBrace)
        {
            Token propertyToken = CurrentToken;
            switch (propertyToken.Type)
            {
                case TokenType.Name:
                    Match(TokenType.Name);
                    Match(TokenType.Colon);
                    card.Name = ParseExpression();
                    Match(TokenType.Comma);
                    break;

                case TokenType.Type:
                    Match(TokenType.Type);
                    Match(TokenType.Colon);
                    card.Type = Match(TokenType.String).Value;
                    Match(TokenType.Comma);
                    break;
                
                case TokenType.EffectKeyword:
                    Match(TokenType.EffectKeyword);
                    Match(TokenType.Colon);
                    card.Effect.Add(Match(TokenType.String).Value);
                    Match(TokenType.Comma);
                    break;
                case TokenType.Range:
                    Match(TokenType.Range);
                    Match(TokenType.Colon);
                    Match(TokenType.OpenBracket);
                    while(CurrentToken.Type != TokenType.CloseBracket)
                    {
                        card.Range.Add(Match(TokenType.String).Value);
                        if(CurrentToken.Type != TokenType.CloseBracket)
                            Match(TokenType.Comma);
                    }
                    Match(TokenType.CloseBracket);
                    Match(TokenType.Comma);
                    break;
                case TokenType.Power:
                    Match(TokenType.Power);
                    Match(TokenType.Colon);
                    card.Power = ParseExpression();
                    Match(TokenType.Comma);
                    break;
                case TokenType.Faction:
                    Match(TokenType.Faction);
                    Match(TokenType.Colon);
                    card.Faction = Match(TokenType.String).Value;
                    Match(TokenType.Comma);
                    break;
                case TokenType.OnActivation:
                    Match(TokenType.OnActivation);
                    Match(TokenType.Colon);
                    Match(TokenType.OpenBracket);
                    while(CurrentToken.Type != TokenType.CloseBracket)
                    {
                        card.OnActivationBlock.Add(ParseActivationBlock());
                        if(CurrentToken.Type == TokenType.Comma)
                            Match(TokenType.Comma);
                    }
                    Match(TokenType.CloseBracket);
                    break;
                default:
                    throw new Exception($"Token inesperado en las valores de ParseCard en la fila {CurrentToken.Row} recibio {CurrentToken}");
            }
        }
        Match(TokenType.CloseBrace);
        return card;
    }
    private ActivationBlockNode ParseActivationBlock(ActivationBlockNode? parent = null)
    {
        ActivationBlockNode actionBlock = new ActivationBlockNode(parent);
        Match(TokenType.OpenBrace);
        while(CurrentToken?.Type != TokenType.CloseBrace)
        {
            if(CurrentToken?.Type == TokenType.EffectKeyword)
            {
                actionBlock.effect = ParseEffectBuilderNode();
            }
            else if(CurrentToken?.Type == TokenType.Selector)
            {
                actionBlock.selector = ParseSelectorNode();
            }
            else if(CurrentToken?.Type == TokenType.PostAction)
            {
                Match(TokenType.PostAction);
                Match(TokenType.Colon);
                actionBlock.postAction = ParseActivationBlock(actionBlock);
            }
            else
                throw new Exception($"Esperaba EffectKW,Selector o PostAction pero recibio {CurrentToken?.Value} en PActivationBlockNode");
        }
        Match(TokenType.CloseBrace);
        return actionBlock;
    }
    private EffectBuilderNode ParseEffectBuilderNode()
    {
        EffectBuilderNode effectBuild = new EffectBuilderNode();
        Match(TokenType.EffectKeyword);
        Match(TokenType.Colon);
        //Sintactic Sugar
        if(CurrentToken?.Type == TokenType.String)
        {
            effectBuild.Name = Match(CurrentToken.Type).Value;
            Match(TokenType.Comma);
            return effectBuild;
        }
        Match(TokenType.OpenBrace);
        while(CurrentToken?.Type != TokenType.CloseBrace)
        {
            if(CurrentToken?.Type == TokenType.Name)
            {
                Match(TokenType.Name);
                Match(TokenType.Colon);
                effectBuild!.Name = Match(CurrentToken.Type).Value;
                Match(TokenType.Comma);
            }
            else
            {
                effectBuild.assingments.Add(ParseAssignment());
            }
        }
        Match(TokenType.CloseBrace);
        Match(TokenType.Comma);
        return effectBuild!;
    }
    private SelectorNode ParseSelectorNode()
    {
        SelectorNode selector = new SelectorNode();
        Match(TokenType.Selector);
        Match(TokenType.Colon);
        Match(TokenType.OpenBrace);
        while(CurrentToken?.Type != TokenType.CloseBrace)
        {
            if(CurrentToken?.Type == TokenType.Source)
            {
                Match(TokenType.Source);
                Match(TokenType.Colon);
                IdentifierNode source = new IdentifierNode(Match(CurrentToken.Type).Value);
                selector.source = source;
                Match(TokenType.Comma);
            }
            else if(CurrentToken?.Type == TokenType.Single)
            {
                Match(TokenType.Single);
                Match(TokenType.Colon);
                selector.single = ParseExpression();
                if (selector.single is not BooleanBinaryExpressionNode && selector.single is not BooleanLiteralNode)
                    throw new Exception($"Esperaba una expresion boolena y recibio {selector.single} en ParseSelectorNode_Single"); 
                Match(TokenType.Comma);
            }
            else if(CurrentToken?.Type == TokenType.Predicate)
            {
                PredicateFunction predicate = new PredicateFunction();
                Match(TokenType.Predicate);
                Match(TokenType.Colon);
                Match(TokenType.OpenParen);
                IdentifierNode cardID = new IdentifierNode(Match(CurrentToken!.Type).Value); 
                predicate.target = cardID;
                Match(TokenType.CloseParen);
                Match(TokenType.Arrow);
                predicate.filter = ParseExpression();
                if (predicate.filter is not BooleanBinaryExpressionNode && selector.single is not BooleanLiteralNode)
                    throw new Exception($"Esperaba una expresion boolena y recibio {predicate.filter} en ParseSelectorNode_Predicate");
            }
        }
        Match(TokenType.CloseBrace);
        Match(TokenType.Comma);
        return selector;
    }
    private ActionBlockNode ParseActionBlock()
    {
        ActionBlockNode actionBlock = new ActionBlockNode();
        Match(TokenType.OpenParen);
        IdentifierNode target = new IdentifierNode(Match(TokenType.Identifier).Value);
        actionBlock.target.Variable = target;
        Match(TokenType.Comma);
        IdentifierNode context = new IdentifierNode(Match(TokenType.Identifier).Value);
        actionBlock.context.Variable = context;
        Match(TokenType.CloseParen);
        Match(TokenType.Arrow);
        
        Match(TokenType.OpenBrace);
        while (CurrentToken != null && CurrentToken.Type != TokenType.CloseBrace)
        {
            actionBlock.Statements.Add(ParseStatement());
        }
        Match(TokenType.CloseBrace);
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
                Match(TokenType.DotCom);
                return new AccesExpressionNode { Expression = expr };
            }
            else if ( expr is PropertyAccessNode || expr is CollectionIndexingNode)
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
                    Match(TokenType.DotCom);
                    return left;
                }
                Match(TokenType.DotCom);
                return new AccesExpressionNode{ Expression = expr };
            }
        }
        throw new Exception($"Token inesperado en Parse Statement: {CurrentToken?.Type}recibido en la linea {CurrentToken?.Row}");
    }

    private ForStatementNode ParseForStatement()
    {
        ForStatementNode forStatement = new ForStatementNode();
        Match(TokenType.For);
        Match(TokenType.OpenParen);
        forStatement.Variable = ParseExpression();
        Match(TokenType.In);
        forStatement.Collection = ParseExpression();
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
                if (op == TokenType.Equals)
                {
                    Match(TokenType.DotCom);
                    return new AssignmentNode
                    {
                        Variable = GetVariableFromExpression(left),
                        Value = right
                    };
                }
                else if(op == TokenType.Colon)
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
        throw new Exception($"Asignacion no valida en la linea {CurrentToken?.Row} lo q recibio fue {CurrentToken}");
    }

    private ExpressionNode ParseExpression(int precedence = 0)
    {
        ExpressionNode left = ParsePrimaryExpression();

        while (CurrentToken != null && IsOperator(CurrentToken.Type) && MathBinaryExpressionNode.Levels[CurrentToken.Type] > precedence)
        {
            TokenType op = CurrentToken.Type;
            Match(op);
            ExpressionNode right = ParseExpression(MathBinaryExpressionNode.Levels[op]);

             if (IsBooleanOperator(op))
            {
                left = new BooleanBinaryExpressionNode { Left = left, Operator = op, Right = right };
            }
            else
            {
                left = new MathBinaryExpressionNode { Left = left, Operator = op, Right = right };
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
            default: throw new Exception($"Operacion Compuesta Invalida en la linea {CurrentToken?.Row}");
        }
    }

    private ExpressionNode GetVariableFromExpression(ExpressionNode expr)
    {
        if (expr is IdentifierNode idNode)
        {
            return idNode!;
        }
        else if (expr is PropertyAccessNode propNode)
        {
             
            return propNode;
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
        return MathBinaryExpressionNode.Levels.ContainsKey(type);
    }
    private ExpressionNode ParsePrimaryExpression()
    {
        ExpressionNode expr = ParseBasicPrimaryExpression();
        //Parseo de Acceso a Propiedad o Metodo
        while (CurrentToken != null && CurrentToken.Type == TokenType.Dot)
        {
            Match(TokenType.Dot);
            IdentifierNode propertyName = (IdentifierNode)ParseBasicPrimaryExpression();

            if (CurrentToken?.Type == TokenType.OpenParen)
            {
                Match(TokenType.OpenParen);
                List<ExpressionNode> arguments = new List<ExpressionNode>();
                if (CurrentToken?.Type != TokenType.CloseParen)
                {
                    do
                    {
                      arguments.Add(ParseExpression());
                    } while(CurrentToken?.Type != TokenType.CloseParen);
                }
                Match(TokenType.CloseParen);
                expr = new MethodCallNode { Target = expr, MethodName = propertyName, Arguments = arguments };
            }
            else
            {
                expr = new PropertyAccessNode { Target = expr, PropertyName = propertyName };
            }
        }
        //Parsear Predicados en Metodos
        if(CurrentToken?.Type == TokenType.OpenParen)
        {
                PredicateFunction predicate = new PredicateFunction();
                Match(TokenType.OpenParen);
                IdentifierNode objectID = new IdentifierNode(Match(CurrentToken!.Type).Value); 
                predicate.target = objectID;
                Match(TokenType.CloseParen);
                Match(TokenType.Arrow);
                predicate.filter = ParseExpression();
                if (predicate.filter is not BooleanBinaryExpressionNode)
                    throw new Exception($"Esperaba una expresion boolena y recibio {predicate.filter} en ParseSelectorNode_Predicate");
                return predicate;
        }
        else if(CurrentToken?.Type == TokenType.OpenBracket)
        {
            CollectionIndexingNode Indexing = new CollectionIndexingNode();
            Match(TokenType.OpenBracket);
            Indexing.Collection = expr;
            Indexing.Index = ParseExpression();
            Match(TokenType.CloseBracket);
            return Indexing;
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
            IdentifierNode Id = new IdentifierNode(ID) ;
            if (CurrentToken.Type == TokenType.MinusMinus || CurrentToken.Type == TokenType.PlusPlus)
            {
                Token op = CurrentToken;
                Match(CurrentToken.Type);
                return new UnaryExpressionNode{Operator = op.Type, Operand = Id, Atend = true};
            }
            else if(CurrentToken.Type == TokenType.SimpleConcat)
            {
                ConcatExpresion expresion = new ConcatExpresion();
                expresion.left = Id;
                Match(TokenType.SimpleConcat);
                expresion.right = ParseExpression();
                return expresion;
            }
            else if(CurrentToken.Type == TokenType.CompConcat)
            {
                ConcatExpresion expresion = new ConcatExpresion();
                expresion.left = Id;
                Match(TokenType.CompConcat);
                expresion.right = ParseExpression();
                expresion.IsComp = true;
                return expresion;
            }
            return Id;
        }
        else if (CurrentToken?.Type == TokenType.Number)
        {
            return new NumberNode { Value = int.Parse(Match(TokenType.Number).Value) };
        }
        else if (CurrentToken?.Type == TokenType.String)
        {
            StringNode str  = new StringNode { Value = Match(TokenType.String).Value };
            if(CurrentToken.Type == TokenType.SimpleConcat)
            {
                ConcatExpresion expresion = new ConcatExpresion();
                expresion.left = str;
                Match(TokenType.SimpleConcat);
                expresion.right = ParseExpression();
                return expresion;
            }
            else if(CurrentToken.Type == TokenType.CompConcat)
            {
                ConcatExpresion expresion = new ConcatExpresion();
                expresion.left = str;
                Match(TokenType.CompConcat);
                expresion.right = ParseExpression();
                expresion.IsComp = true;
                return expresion;
            }
            return str;
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
        } // Para asignacion de propiedades
        else if(CurrentToken?.Type == TokenType.Power || CurrentToken?.Type == TokenType.Name||CurrentToken?.Type == TokenType.Type ||CurrentToken?.Type == TokenType.Range|| CurrentToken?.Type == TokenType.Faction|| CurrentToken?.Type == TokenType.Identifier)
        {
            IdentifierNode prop = new IdentifierNode(Match(CurrentToken.Type).Value);
            return prop;
        }
        else
        {
            throw new Exception($"Se esperaba expresión primaria y se recibió: {CurrentToken?.Type} en la linea {CurrentToken?.Row}");
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
            Match(TokenType.DotCom);                                     
            return block;
        }
        //Bloque de instruccion unica
        while (CurrentToken != null && CurrentToken.Type != TokenType.Comma)
        {
            if(CurrentToken.Type == TokenType.CloseBrace)
            {
                // Match(TokenType.CloseBrace);
                break;
            }
            block.Statements.Add(ParseStatement());
        }
        // Match(TokenType.Comma);
        return block;
    }
}
