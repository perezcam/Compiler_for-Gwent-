namespace Interpeter
{
    public interface IVisitor
    {
        void Visit(ProgramNode node,Scope scope);
        void Visit(EffectNode node,Scope scope);
        void Visit(CardNode node,Scope scope);
        void Visit(ActivationBlockNode node,Scope scope);
        void Visit(EffectBuilderNode node,Scope scope);
        void Visit(SelectorNode node,Scope scope);
        void Visit(PredicateFunction node,Scope scope);
        void Visit(ActionBlockNode node,Scope scope);
        void Visit(ForStatementNode node,Scope scope);
        void Visit(WhileStatementNode node,Scope scope);
        void Visit(AssignmentNode node,Scope scope);
        void Visit(AccesExpressionNode node,Scope scope);
        void Visit(IdentifierNode node,Scope scope);
        void Visit(UnaryExpressionNode node,Scope scope);
        void Visit(MathBinaryExpressionNode node,Scope scope);
        void Visit(BooleanLiteralNode node,Scope scope);
        void Visit(BooleanBinaryExpressionNode node,Scope scope);
        void Visit(NumberNode node,Scope scope);
        void Visit(DataTypeNode node,Scope scope);
        void Visit(StringNode node,Scope scope);
        void Visit(BlockNode node,Scope scope);
        void Visit(PropertyAccessNode node,Scope scope);
        void Visit(CollectionIndexingNode node,Scope scope);
        void Visit(CompoundAssignmentNode node,Scope scope);
        void Visit(MethodCallNode node,Scope scope);
        void Visit(ConcatExpresion node,Scope scope);
        void Visit(ErrorBlockNode node,Scope scope);
    }

}