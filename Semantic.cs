public class SemanticChecker
{
    private ProgramNode program;
    private Scope globalScope;

    public SemanticChecker(ProgramNode program)
    {
        this.program = program;
        this.globalScope = new Scope();
    }

    public void CheckSemantic()
    {
        foreach (var statement in program.Statements)
        {
            CheckStatement(statement, globalScope);
        }
    }

    private void CheckStatement(ASTNode statement, Scope scope)
    {
        switch (statement)
        {
            case EffectNode effectNode:
                CheckEffectNode(effectNode, scope);
                break;
            case CardNode cardNode:
                CheckCardNode(cardNode, scope);
                break;
            default:
                throw new Exception($"Tipo de nodo no manejado: {statement.GetType()}");
        }
    }

    private void CheckEffectNode(EffectNode effect, Scope scope)
    {
        if (string.IsNullOrEmpty(effect.Name))
        {
            throw new Exception("El nombre del efecto no puede ser nulo o vacío.");
        }
        Scope effectScope = new Scope(scope); 
        
        foreach (var assignment in effect.assignments)
        {
            CheckAssignment(assignment, effectScope);
        }
        if (effect.Action != null)
        {
            CheckActionBlock(effect.Action, effectScope);
        }
    }

    private void CheckCardNode(CardNode card, Scope scope)
    {
        if (string.IsNullOrEmpty(card.Name))
        {
            throw new Exception("El nombre de la carta no puede ser nulo o vacío.");
        }
        if (string.IsNullOrEmpty(card.Type))
        {
            throw new Exception("El tipo de la carta no puede ser nulo o vacío.");
        }

        Scope cardScope = new Scope(scope);

        if (!string.IsNullOrEmpty(card.Faction))
        {
            cardScope.Declare("Faction", "string");
        }

        // Chequear el poder si existe
        if (card.Power != null)
        {
            CheckExpression(card.Power, cardScope);
        }

        // Chequear los bloques de activación
        foreach (var activationBlock in card.OnActivationBlock)
        {
            CheckActivationBlock(activationBlock, cardScope);
        }
    }

    private void CheckActivationBlock(ActivationBlockNode activationBlock, Scope scope)
    {
        Scope activationScope = new Scope(scope);

        // Chequear el selector si existe
        if (activationBlock.selector != null)
        {
            CheckSelectorNode(activationBlock.selector, activationScope);
        }

        // Chequear el efecto si existe
        if (activationBlock.effect != null)
        {
            CheckEffectBuilderNode(activationBlock.effect, activationScope);
        }

        // Chequear la acción posterior si existe
        if (activationBlock.postAction != null)
        {
            CheckActivationBlock(activationBlock.postAction, activationScope);
        }
    }

    private void CheckEffectBuilderNode(EffectBuilderNode effectBuilder, Scope scope)
    {
        // Chequear que el nombre del efecto no sea nulo
        if (string.IsNullOrEmpty(effectBuilder.Name))
        {
            throw new Exception("El nombre del constructor de efecto no puede ser nulo o vacío.");
        }

        // Crear un nuevo ámbito para el constructor de efecto
        var effectBuilderScope = new Scope(scope);

        // Chequear las asignaciones dentro del constructor de efecto
        foreach (var assignment in effectBuilder.assingments)
        {
            CheckStatement(assignment, effectBuilderScope);
        }
    }

    private void CheckSelectorNode(SelectorNode selector, Scope scope)
    {
        // Chequear el selector de fuente
        if (selector.source == null)
        {
            throw new Exception("El selector debe tener una fuente.");
        }

        // Chequear la expresión simple si existe
        if (selector.single != null)
        {
            CheckExpression(selector.single, scope);
        }

        // Chequear la función de predicado si existe
        if (selector.predicate != null)
        {
            CheckPredicateFunction(selector.predicate, scope);
        }
    }

    private void CheckPredicateFunction(PredicateFunction predicate, Scope scope)
    {
        // Chequear que el objetivo del predicado no sea nulo
        if (predicate.target == null)
        {
            throw new Exception("La función de predicado debe tener un objetivo.");
        }

        // Chequear la expresión de filtro
        if (predicate.filter != null)
        {
            CheckExpression(predicate.filter, scope);
        }
    }

    private void CheckActionBlock(ActionBlockNode actionBlock, Scope scope)
    {
        Scope actionScope = new Scope(scope);

        // Chequear que las asignaciones target y context no sean nulas
        if (actionBlock.target == null || actionBlock.context == null)
        {
            throw new Exception("Las asignaciones target y context no pueden ser nulas.");
        }

        // Declarar las variables target y context en el ámbito
        actionScope.Declare(actionBlock.target.Variable!, "variable");
        actionScope.Declare(actionBlock.context.Variable!, "variable");

        // Chequear las declaraciones dentro del bloque de acción
        foreach (var statement in actionBlock.Statements)
        {
            CheckStatement(statement, actionScope);
        }
    }

    private void CheckAssignment(AssignmentNode assignment, Scope scope)
    {
        if (string.IsNullOrEmpty(assignment.Variable))
        {
            throw new Exception("La variable de la asignación no puede ser nula o vacía.");
        }
        scope.Declare(assignment.Variable, "variable");

        // Chequear que el valor de la asignación no sea nulo
        if (assignment.Value == null)
        {
            throw new Exception("El valor de la asignación no puede ser nulo.");
        }

        // Chequear la expresión del valor
        CheckExpression(assignment.Value, scope);
    }

    private void CheckExpression(ExpressionNode expression, Scope scope)
    {
        switch (expression)
        {
            case BinaryExpressionNode binaryExpr:
                CheckExpression(binaryExpr.Left!, scope);
                CheckExpression(binaryExpr.Right!, scope);
                break;
            case UnaryExpressionNode unaryExpr:
                CheckExpression(unaryExpr.Operand!, scope);
                break;
            case BooleanBinaryExpressionNode booleanExpr:
                CheckExpression(booleanExpr.Left!, scope);
                CheckExpression(booleanExpr.Right!, scope);
                break;
            case IdentifierNode identifier:
                // Verificar que el identificador exista en el contexto
                if (string.IsNullOrEmpty(identifier.Name) || scope.Resolve(identifier.Name) == null)
                {
                    throw new Exception($"El identificador '{identifier.Name}' no está declarado en este ámbito.");
                }
                break;
            case NumberNode _:
            case StringNode _:
            case BooleanLiteralNode _:
                break;
            case MethodCallNode methodCall:
                CheckExpression(methodCall.Target!, scope);
                if (methodCall.Arguments != null)
                {
                    foreach (var arg in methodCall.Arguments)
                    {
                        CheckExpression(arg, scope);
                    }
                }
                break;
            case PropertyAccessNode propertyAccess:
                CheckExpression(propertyAccess.Target!, scope);
                if (propertyAccess.PropertyName == null || string.IsNullOrEmpty(propertyAccess.PropertyName.Name))
                {
                    throw new Exception("El nombre de la propiedad no puede ser nulo o vacío.");
                }
                break;
            case CollectionIndexingNode collectionIndexing:
                CheckExpression(collectionIndexing.Collection!, scope);
                CheckExpression(collectionIndexing.Index!, scope);
                break;
            default:
                throw new Exception($"Tipo de expresión no manejada: {expression.GetType()}");
        }
    }
}
