public static class ASTPrinter
{
    public static void PrintAST(ASTNode node, string indent = "", bool isLast = true)
    {
        if (node == null) return;

        Console.Write(indent);
        if (isLast)
        {
            Console.Write("└─");
            indent += "  ";
        }
        else
        {
            Console.Write("├─");
            indent += "| ";
        }

        Console.WriteLine(node.GetType().Name);

        PrintNodeDetails(node, indent);

        if (node is ProgramNode programNode)
        {
            for (int i = 0; i < programNode.Statements.Count; i++)
            {
                PrintAST(programNode.Statements[i], indent, i == programNode.Statements.Count - 1);
            }
        }
        else if (node is EffectNode effectNode)
        {
            for (int i = 0; i < effectNode.Params.Count; i++)
            {
                PrintAST(effectNode.Params[i], indent, i == effectNode.Params.Count - 1);
            }
            PrintAST(effectNode.Action!, indent, true);
        }
        else if (node is ActionBlockNode actionBlockNode)
        {
            for (int i = 0; i < actionBlockNode.Statements.Count; i++)
            {
                PrintAST(actionBlockNode.Statements[i], indent, i == actionBlockNode.Statements.Count - 1);
            }
        }
        else if (node is BlockNode blockNode)
        {
            for (int i = 0; i < blockNode.Statements.Count; i++)
            {
                PrintAST(blockNode.Statements[i], indent, i == blockNode.Statements.Count - 1);
            }
        }
        else if (node is ForStatementNode forStatementNode)
        {
            PrintAST(forStatementNode.Body!, indent, true);
        }
        else if (node is WhileStatementNode whileStatementNode)
        {
            PrintAST(whileStatementNode.Condition!, indent, false);
            PrintAST(whileStatementNode.Body!, indent, true);
        }
        else if (node is AssignmentNode assignmentNode)
        {
            PrintAST(assignmentNode.Value!, indent, true);
        }
        else if (node is UnaryExpressionNode unaryExpressionNode)
        {
            PrintAST(unaryExpressionNode.Operand!, indent, true);
        }
        else if (node is MathBinaryExpressionNode binaryExpressionNode)
        {
            PrintAST(binaryExpressionNode.Left!, indent, false);
            PrintAST(binaryExpressionNode.Right!, indent, true);
        }
        else if (node is BooleanBinaryExpressionNode booleanBinaryExpressionNode)
        {
            PrintAST(booleanBinaryExpressionNode.Left!, indent, false);
            PrintAST(booleanBinaryExpressionNode.Right!, indent, true);
        }
    }

    private static void PrintNodeDetails(ASTNode node, string indent)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        switch (node)
        {
            case ProgramNode programNode:
                break;
            case EffectNode effectNode:
                Console.WriteLine($"{indent}Name: {effectNode.Name}");
                break;
            case CardNode cardNode:
                Console.WriteLine($"{indent}Name: {cardNode.Name}");
                Console.WriteLine($"{indent}Type: {cardNode.Type}");
                Console.WriteLine($"{indent}Effect: {cardNode.Effect}");
                break;
            case ForStatementNode forStatementNode:
                Console.WriteLine($"{indent}Variable: {forStatementNode.Variable}");
                Console.WriteLine($"{indent}Collection: {forStatementNode.Collection}");
                break;
            case WhileStatementNode whileStatementNode:
                break;
            case AssignmentNode assignmentNode:
                Console.WriteLine($"{indent}Variable: {assignmentNode.Variable}");
                break;
            case IdentifierNode identifierNode:
                Console.WriteLine($"{indent}Name: {identifierNode.Name}");
                break;
            case UnaryExpressionNode unaryExpressionNode:
                Console.WriteLine($"{indent}Operator: {unaryExpressionNode.Operator}");
                break;
            case MathBinaryExpressionNode binaryExpressionNode:
                Console.WriteLine($"{indent}Operator: {binaryExpressionNode.Operator}");
                break;
            case BooleanLiteralNode booleanLiteralNode:
                Console.WriteLine($"{indent}Value: {booleanLiteralNode.Value}");
                break;
            case BooleanBinaryExpressionNode booleanBinaryExpressionNode:
                Console.WriteLine($"{indent}Operator: {booleanBinaryExpressionNode.Operator}");
                break;
            case NumberNode numberNode:
                Console.WriteLine($"{indent}Value: {numberNode.Value}");
                break;
            case StringNode stringNode:
                Console.WriteLine($"{indent}Value: {stringNode.Value}");
                break;
            default:
                break;
        }
        Console.ForegroundColor = ConsoleColor.White;
    }
}
