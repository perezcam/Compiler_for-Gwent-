using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    { ///Crea un nodo para guardar la lista de direcciones d un llamado como propiedades target.Power 
        string input = @"
        effect 
        {
            Name: ""Damage"",
            Params: 
            {
                
            },
            Action: (targets, context) =>
            {
                for (target in targets) 
                {
                   while(i++ < Amount)
                        target.Power -= 1,
                },
            },
        }
        ";  
        // card
        // {
        //     Type: ""Oro"",
        //     Name: ""Beluga"",
        //     Faction: ""Northern Realms"",
        //     Power: 10,
        //     Range: [""Melee"",""Ranged""],
        //     OnActivation:
        //     [
        //         {
        //             Effect: 
        //             {
        //                 Name: ""Damage"",
        //                 Amount: 5,
        //             },
        //             Selector:
        //             {
        //                 Source: ""board"",
        //                 Single: false,
        //                 Predicate: (unit) => unit.Faction == ""Northern"" @@ ""Realms"",
        //             },
        //             PostAction:
        //             {
        //                 Type: ""Return to Deck"",

        //             }

        //         }
        //     ]
        // }
        // ";
        Lexer lexer = new Lexer();
        List<Token> tokens = lexer.Tokenize(input);

        Console.WriteLine("Tokens:");
        foreach (var token in tokens)
        {
            Console.WriteLine(token);
        }

        Parser parser = new Parser(tokens);
        ProgramNode programNode = parser.Parse();

        Console.WriteLine("\nAST:");
        ASTPrinter.PrintAST(programNode);
    }

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

            if (node is ProgramNode programNode)
            {
                for (int i = 0; i < programNode.Statements.Count; i++)
                {
                    PrintAST(programNode.Statements[i], indent, i == programNode.Statements.Count - 1);
                }
            }
            else if (node is EffectNode effectNode)
            {
                Console.Write(indent);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"Name: {effectNode.Name}");
                Console.ForegroundColor = ConsoleColor.White;

                for (int i = 0; i < effectNode.assignments.Count; i++)
                {
                    PrintAST(effectNode.assignments[i], indent, i == effectNode.assignments.Count - 1);
                }
                PrintAST(effectNode.Action, indent, true);
            }
            else if (node is CardNode cardNode)
            {
                Console.WriteLine($"{indent}Name: {cardNode.Name}");
                Console.WriteLine($"{indent}Type: {cardNode.Type}");
                Console.WriteLine($"{indent}Effect: {cardNode.Effect}");
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
                Console.WriteLine($"{indent}Variable: {forStatementNode.Variable}");
                Console.WriteLine($"{indent}Collection: {forStatementNode.Collection}");
                PrintAST(forStatementNode.Body, indent, true);
            }
            else if (node is WhileStatementNode whileStatementNode)
            {
                PrintAST(whileStatementNode.Condition, indent, false);
                PrintAST(whileStatementNode.Body, indent, true);
            }
            else if (node is AssignmentNode assignmentNode)
            {
                Console.WriteLine($"{indent}Variable: {assignmentNode.Variable}");
                PrintAST(assignmentNode.Value, indent, true);
            }
            else if (node is IdentifierNode identifierNode)
            {
                Console.WriteLine($"{indent}Name: {identifierNode.Name}");
            }
            else if (node is UnaryExpressionNode unaryExpressionNode)
            {
                Console.WriteLine($"{indent}Operator: {unaryExpressionNode.Operator}");
                PrintAST(unaryExpressionNode.Operand, indent, true);
            }
            else if (node is BinaryExpressionNode binaryExpressionNode)
            {
                PrintAST(binaryExpressionNode.Left, indent, false);
                Console.WriteLine($"{indent}Operator: {binaryExpressionNode.Operator}");
                PrintAST(binaryExpressionNode.Right, indent, true);
            }
            else if (node is BooleanLiteralNode booleanLiteralNode)
            {
                Console.WriteLine($"{indent}Value: {booleanLiteralNode.Value}");
            }
            else if (node is BooleanBinaryExpressionNode booleanBinaryExpressionNode)
            {
                PrintAST(booleanBinaryExpressionNode.Left, indent, false);
                Console.WriteLine($"{indent}Operator: {booleanBinaryExpressionNode.Operator}");
                PrintAST(booleanBinaryExpressionNode.Right, indent, true);
            }
            else if (node is NumberNode numberNode)
            {
                Console.WriteLine($"{indent}Value: {numberNode.Value}");
            }
            else if (node is StringNode stringNode)
            {
                Console.WriteLine($"{indent}Value: {stringNode.Value}");
            }
        }
    }
}
