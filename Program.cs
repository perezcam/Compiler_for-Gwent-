using System;
using System.Collections.Generic;
using Interpeter;

class Program
{
    static void Main()
    {
        #region CasosPrueba
        string input = 
        @"
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
                   {
                        target.Power -= 1+32*4-(35/7 - 3)/45 + 23;
                        context.Hand[4];
                   };
                };
            }
        }
        card
        {
            Type: ""Oro"",
            Name: ""Beluga@@Azul"",
            Faction: ""Northern Realms"",
            Power: 10,
            Range: [""Melee"",""Ranged""],
            OnActivation:
            [
                {
                    Effect: 
                    {
                        Name: ""Damage"",
                        Amount: 5 + ""azul"",
                    },
                    Selector:
                    {
                        Source: ""board"",
                        Single: false,
                        Predicate: (unit) => unit.Faction == ""Northern"" @@ ""Realms""@@""Azul""
                    },
                    PostAction:
                    {
                        Effect: ""Return to Deck"",
                    }

                }
            ]
        }
        effect
        {
            Name:""Draw"",
            Action: (targets,context) => 
            {
                topCard = context.Deck.Pop();
                context.Hand.Add(topCard);
                context.Hand.Shuffle();
            }
        }";
        #endregion

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
        
        SemanticChecker checker = new SemanticChecker(programNode);
    }
}