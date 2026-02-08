grammar QuantDsl;

// ===== PARSER RULES =====

file        : productDecl statement* EOF ;

productDecl : PRODUCT IDENT ;

statement
    : NOTIONAL NUMBER
    | FIXED_RATE NUMBER
    | MATURITY TENOR
    | PAYOFF '=' expr
    ;

// expression with precedence
expr    : term (('+'|'-') term)* ;
term    : factor (('*'|'/') factor)* ;
factor  : (IDENT | NOTIONAL | FIXED_RATE) | NUMBER | '(' expr ')' ;

// ===== LEXER RULES =====

// keywords (explicit)
PRODUCT    : 'product' ;
NOTIONAL   : 'notional' ;
FIXED_RATE : 'fixed_rate' ;
MATURITY   : 'maturity' ;
PAYOFF     : 'payoff' ;

// other tokens
TENOR  : [0-9]+ [DWMY] ;        // 5Y, 10D, 3M, 2W
IDENT  : [a-zA-Z_][a-zA-Z_0-9]* ;
NUMBER : [0-9]+ ('.' [0-9]+)? ;

WS     : [ \t\r\n]+ -> skip ;
