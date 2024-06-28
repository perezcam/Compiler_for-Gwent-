<Program>          ::= <StatementList>

<StatementList>    ::= <Statement> <StatementList>
                    |  ε

<Statement>        ::= <EffectDeclaration>
                    |  <CardDeclaration>

<EffectDeclaration> ::= 'effect' 'name' ':' <Cadena> 'action' ':' <ActionBlock>

<CardDeclaration>  ::= 'card' 'name' ':' <Cadena> 'type' ':' <Cadena> 'effect_keyword' ':' <Cadena>

<ActionBlock>      ::= '{' <StatementList> '}'

<Statement>        ::= <ForStatement>
                    |  <WhileStatement>
                    |  <Assignment>

<ForStatement>     ::= 'for' '(' <Identificador> 'in' <Identificador> ')' <Block>

<WhileStatement>   ::= 'while' '(' <Expression> ')' <Block>

<Assignment>       ::= <Identificador> '=' <Expression>

<Block>            ::= '{' <StatementList> '}'

<Expression>       ::= <Identificador>
                    |  <Número>
                    |  <Cadena>
                    |  '(' <Expression> ')'
