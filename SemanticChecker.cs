using System;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.VisualBasic;
namespace Interpeter
{
    public class SemanticChecker : IVisitor
    {
        private Dictionary<ExpressionNode, string> typeTable = new Dictionary<ExpressionNode, string>();
        //Introduces "card" y recibes acceso a todos los posibles parametros de card
        private Dictionary<string, TypeInfo> typeInfo = new Dictionary<string, TypeInfo>();
        //methodParamsINOUT Key-NombredelMetodo Value-item1.Tipo de Entrada item2.Tipo de Salida
        private Dictionary<string, (string,string)> methodParamsINOUT = new Dictionary<string, (string,string)>();
        private Dictionary<string, List<(string,string)>> effectDebt = new Dictionary<string, List<(string,string)>>();
        private ProgramNode program; 
        public SemanticChecker(ProgramNode program)
        {
            //Diccionario de Efectos INOUT Types
            methodParamsINOUT["Find"] = ("predicate","List<card>");
            methodParamsINOUT["Push"] = ("card","void");
            methodParamsINOUT["SendBottom"] = ("card","void");
            methodParamsINOUT["Pop"] = ("void", "void");
            methodParamsINOUT["Remove"] = ("card","void");
            methodParamsINOUT["Shuffle"] = ("void", "void");
            this.program = program;
            CheckSemantic(program);
        }
         public void CheckSemantic(ProgramNode program)
        {
            Scope ProgramScope = new Scope();
            Visit(program,ProgramScope);
        }
        public void Visit(ProgramNode node, Scope scope)
        {
            foreach (var statement in node.Statements)
            {
                statement.Accept(this,scope);
            }
        }
        public void Visit(EffectNode node,Scope scope)
        {
            if(node.Name is null)
                throw new Exception("El nombre del efecto no puede ser nulo");
            if(node.Action is null)
                throw new Exception("El Bloque de Accion del efecto no debe ser null");
            
            List<(string,string)> param_type = new List<(string, string)>();
            foreach (var param in node.Params)
            {
                param.Accept(this,scope);
                param_type.Add(((param.Variable as IdentifierNode)!.Name!,EvaluateExpressionType(param.Value!,scope)));
            }
            effectDebt[node.Name] = param_type;
            Scope actionBlockScope = new Scope(scope);
            node.Action?.Accept(this,actionBlockScope);
        }
        public void Visit(CardNode node,Scope scope)
        {
            if (node.Name != null)
            {
               typeTable[node.Name] = "card";
            }
            node.Power?.Accept(this,scope);
            foreach (var activation in node.OnActivationBlock)
            {
                Scope activationScope = new Scope(scope);
                activation.Accept(this,activationScope);
            }
        }
        public void Visit(ActivationBlockNode node,Scope scope)
        {
            if(node.parent is null && node.selector is null)
                throw new Exception("El selector no puede ser nulo pq no es un postaction");
            else if(node.selector is null && node.parent is not null)
                node.selector = node.parent.selector;
            node.effect?.Accept(this,scope);
            node.selector?.Accept(this,scope);
            node.postAction?.Accept(this,scope);   
        }
        public void Visit(EffectBuilderNode node,Scope scope)
        {
            if(!effectDebt.ContainsKey(node.Name!))
                throw new Exception("La carta esta haciendo referencia a un objeto no declarado");
            List<(string,string)> paramsDeclaration = new List<(string, string)>();
            foreach (var statement in node.assingments)
            {
                statement.Accept(this,scope);
                AssignmentNode assignment = (AssignmentNode)statement;
                string assignmentType = EvaluateExpressionType(assignment.Value!,scope);
                paramsDeclaration.Add(((assignment.Variable as IdentifierNode)!.Name!,assignmentType));
            }
            List<(string,string)> paramsDiffer = new List<(string, string)>();
            foreach (var param in effectDebt[node.Name!])
            {
                if(!paramsDeclaration.Contains(param))
                    paramsDiffer.Add(param);
            }
            if(paramsDiffer.Count != 0)
                throw new Exception($"Los parametros {paramsDiffer} no coinciden con los que recibe el efecto {node.Name}");
        }
        public void Visit(SelectorNode node,Scope scope)
        {
            ContextInfo contextprop = new ContextInfo("properties");
            node.source?.Accept(this,scope);
            if(!contextprop.Properties!.ContainsKey(node.source!.Name!))
                throw new Exception($"El ambito {node.source.Name} no pertenece al contexto");
            node.single?.Accept(this,scope);
            if(EvaluateExpressionType(node.single!,scope) != "bool" )
                throw new Exception ($"La expresion {node.single} no es una expresion de tipo booleano");
            node.predicate?.Accept(this,scope);
        }
        public void Visit(PredicateFunction node,Scope scope)
        {
            if(scope.Vars.ContainsKey(node.target!.Name!))
                throw new Exception($"La varibale {node.target.Name} ya existe");
            scope.Declare(node.target.Name!,"card");
            typeInfo[node.target.Name!] = new CardInfo(node.target.Name!);
            node.filter?.Accept(this,scope);
        }
        public void Visit(ActionBlockNode node,Scope scope)
        {
            node.target.Accept(this,scope);
            node.context.Accept(this,scope);
            IdentifierNode context_id = (IdentifierNode) node.context.Variable!; 
            typeInfo[context_id.Name!] = new ContextInfo(context_id.Name!); 

            foreach (var statement in node.Statements)
            {
                Scope statementScope = new Scope();
                statement.Accept(this,statementScope);
            }
        }
        public void Visit(ForStatementNode node,Scope scope)
        {
            if (node.Variable != null && node.Variable is IdentifierNode identifierNode &&!typeTable.ContainsKey(node.Variable))
            {
                typeTable[node.Variable] = "var";
                scope.Declare(identifierNode.Name!,"var");
            }
            node.Body?.Accept(this,scope);
        }
        public void Visit(WhileStatementNode node,Scope scope)
        {
            if(node.Condition != null && node.Condition is BooleanBinaryExpressionNode || node.Condition is BooleanLiteralNode)
                typeTable[node.Condition] = "bool";
            else 
                throw new Exception("La condicion recibida en la condicion no es booleana");   
          
            node.Body?.Accept(this,scope);
           
        }
        public void Visit(BlockNode node,Scope scope)
        {
            foreach (var statement in node.Statements)
            {   
                Scope statementScope = new Scope(scope);
                statement.Accept(this,statementScope);
            }
        }
        public void Visit(AssignmentNode node,Scope scope)
        {
            node.Value?.Accept(this,scope);
            
            if (node.Variable is null)
                throw new Exception("Variable nula");
            
            if(node.Variable is IdentifierNode identifier)
            {
                string varName = identifier.Name!;   
                scope.Declare(varName,EvaluateExpressionType(node.Value!,scope));            
            }
            else if(node.Variable is PropertyAccessNode property)
            {
                property.Accept(this,scope);
                if(EvaluateExpressionType(property,scope) != EvaluateExpressionType(node.Value!,scope))
                    throw new Exception($"Se esta intentando modificar el valor de {property.PropertyName!.Name} con un tipo {EvaluateExpressionType(node.Value!,scope)} distinto al suyo");
            }
            else if(node.Variable is MethodCallNode method)
            {
                method.Accept(this,scope);
            }
            else
                throw new Exception("Tipo de Asignacion no esperada");
        }
        public void Visit(AccesExpressionNode node,Scope scope)
        {
            node.Expression!.Accept(this,scope);
        }
        public void Visit(IdentifierNode node,Scope scope)
        {
            if (node.Name != null && !scope.ContainsVar(node.Name))
            {
                throw new Exception($"Variable '{node.Name}' is not declared.");
            }
        }
        public void Visit(UnaryExpressionNode node,Scope scope)
        {
            node.Operand?.Accept(this,scope);
            string OperandID = (node.Operand as IdentifierNode)!.Name!;
            if(!scope.ContainsVar(OperandID))
                throw new Exception("Variable no declarada");
            if(scope.Vars[OperandID] != "int")
                throw new Exception("No se puede incrementar una variable que no sea de tipo numerico");
        }
        public void Visit(MathBinaryExpressionNode node,Scope scope)
        {
            node.Left?.Accept(this,scope);
            node.Right?.Accept(this,scope);
            EvaluateExpressionType(node,scope);
        }
        public void Visit(BooleanLiteralNode node,Scope scope)
        {
            EvaluateExpressionType(node,scope);
        }
        public void Visit(BooleanBinaryExpressionNode node,Scope scope)
        {
            node.Left?.Accept(this,scope);
            node.Right?.Accept(this,scope);
            EvaluateExpressionType(node,scope);
        }
        public void Visit(NumberNode node,Scope scope)
        {
            EvaluateExpressionType(node,scope);
        }
        public void Visit(DataTypeNode node,Scope scope)
        {
            EvaluateExpressionType(node,scope);
        }
        public void Visit(StringNode node,Scope scope)
        {
            EvaluateExpressionType(node,scope);
        }
        public void Visit(PropertyAccessNode node,Scope scope)
        {
            node.Target?.Accept(this,scope);
            node.PropertyName?.Accept(this,scope);
            if(node.Target is PropertyAccessNode prop && !ContainsProperty(prop.PropertyName!,node.PropertyName!))
            {
                throw new Exception($"El objeto {prop.PropertyName!.Name} no contiene la propiedad {node.PropertyName}");
            }
            else if (node.Target is IdentifierNode iden && (!ContainsProperty((node.Target as IdentifierNode)!, node.PropertyName!) || !scope.ContainsVar((node.Target as IdentifierNode)!.Name!)) )
            {
                throw new Exception($"El objeto {(node.Target as IdentifierNode)!.Name } no contiene la propiedad {node.PropertyName}");
            }    
        }
        public void Visit(CollectionIndexingNode node,Scope scope)
        {
            node.Collection?.Accept(this,scope);
            node.Index?.Accept(this,scope);
            if(EvaluateExpressionType(node.Index!, scope) != "int")
                throw new Exception($"El valor de indexado no es correcto pues en de tipo {EvaluateExpressionType(node.Index!, scope)} en lugar de int");
            if(EvaluateExpressionType(node.Collection!,scope) != "List<card>")
                throw new Exception($"El objeto {node.Collection} no es indexable");

        }
        public void Visit(CompoundAssignmentNode node,Scope scope)
        {
            node.Target?.Accept(this,scope);
            node.Value?.Accept(this,scope);
            if(EvaluateExpressionType(node.Target!,scope) != EvaluateExpressionType(node.value!,scope))
                throw new Exception("Los valores no son tipo entero ambos");
        }
        public void Visit(MethodCallNode node,Scope scope)
        {
            node.Target?.Accept(this,scope);
            node.MethodName?.Accept(this,scope);
            if (node.Arguments != null)
            {
                foreach (var argument in node.Arguments)
                {
                    argument.Accept(this,scope);
                }
            }
            if(methodParamsINOUT[node!.MethodName!.Name!].Item1 != EvaluateExpressionType(node.Arguments!.ElementAt(0),scope))
                throw new Exception($"El metodo recibe {methodParamsINOUT[node!.MethodName!.Name!].Item1} y recibio {EvaluateExpressionType(node.Arguments!.ElementAt(0),scope)}");

        }
        public void Visit(ConcatExpresion node,Scope scope)
        {
            node.right?.Accept(this,scope);
            node.left?.Accept(this,scope);
            EvaluateExpressionType(node,scope);
        }
        public void Visit(ErrorBlockNode node,Scope scope)
        {
            // Manejar nodos de error si es necesario
        }
        private string EvaluateExpressionType(ExpressionNode expr,Scope scope)
        {
            if (expr is IdentifierNode idNode)
            {
                if (scope.Vars.TryGetValue(idNode.Name!, out var type))
                {
                    return type;
                }
                else
                {
                    throw new Exception($"Variable '{idNode.Name}' is not declared.");
                }
            }
            else if (expr is NumberNode)
            {
                return "int";
            }
            else if (expr is StringNode)
            {
                return "string";
            }
            else if (expr is BooleanLiteralNode)
            {
                return "bool";
            }
            else if (expr is ConcatExpresion concExpr)
            {
                string leftType = EvaluateExpressionType(concExpr.left!, scope);
                string rightType = EvaluateExpressionType(concExpr.right!, scope);
                if (leftType == rightType && leftType == "string")
                {
                    return "string";
                }
                else if(leftType != "string")
                    throw new Exception($"Se esperaba tipo string y se recibio {leftType} en {concExpr.left}");
                else
                    throw new Exception($"Se esperaba tipo string y se recibio {rightType} en {concExpr.right}");
            }
            else if (expr is MathBinaryExpressionNode mathExpr)
            {
                string leftType = EvaluateExpressionType(mathExpr.Left!, scope);
                string rightType = EvaluateExpressionType(mathExpr.Right!, scope);
                if (leftType == rightType && leftType == "int")
                {
                    return "int";
                }
                else if(leftType != "int")
                    throw new Exception($"Se esperaba tipo int y se recibio {leftType} en {mathExpr.Left}");
                else
                    throw new Exception($"Se esperaba tipo int y se recibio {rightType} en {mathExpr.Right}");
            }
            else if (expr is BooleanBinaryExpressionNode boolExpr)
            {
                string leftType = EvaluateExpressionType(boolExpr.Left!,scope);
                string rightType = EvaluateExpressionType(boolExpr.Right!,scope);
                if (leftType == rightType && leftType == "bool")
                {
                    return "bool";
                }
                else if(leftType != "bool")
                    throw new Exception($"Se esperaba tipo bool y se recibio {leftType} en {boolExpr.Left}");
                else
                    throw new Exception($"Se esperaba tipo bool y se recibio {rightType} en {boolExpr.Right}");
               
            }
            else if (expr is PropertyAccessNode propertyAccess)
            {
                ContextInfo context = new ContextInfo("access");
                return context.Properties![propertyAccess.PropertyName!.Name!];
            }
            else if (expr is MethodCallNode method)
            {
                return methodParamsINOUT[method.MethodName!.Name!].Item2;
            }
            throw new Exception($"Tipo de Expresion no Esperada {expr.GetType()}");
        }
        public bool ContainsProperty(IdentifierNode target, IdentifierNode property)
        {
            TypeInfo type = typeInfo[target.Name!];
            if(!type.Properties!.ContainsKey(property.Name!))
                return false;
            return true;
        }
    }
}