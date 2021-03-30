namespace lerve.Services.LispService.EvaluatorDomain

open lerve.Services.LispService.ParserDomain
open lerve.Services.LispService.EvaluatorDomain.ExpressionEvaluator

module Builtins = 
    let private fromBool (b: bool): Value = if b then String "#t" else String "#f"
    
    let public asAtom (expr: Expression): string =
        match expr with
        | Atom a -> a
        | _      -> raise <| EvaluatorException "Invalid atom"
        
    let private asNumber (expr: Value): int =
        match expr with
        | Number n -> n
        | _        -> raise <| EvaluatorException "Invalid atom"
    
    let private add (values: Value list): Value =
        List.map int values |> List.sum |> Number 
    
    let private sub (values: Value list): Value =
        match values with
        | [Number n]     -> Number -n
        | Number n :: xs -> List.map int xs |> List.fold (-) n |> Number
        | _              -> raise <| EvaluatorException "(-): Invalid arguments"
    
    let private mult (values: Value list): Value =
        List.map int values |> List.fold (*) 1 |> Number 
    
    let private division (values: Value list): Value =
        match values with
        | [Number _   ; Number 0  ] -> raise  <| EvaluatorException "div: Floating point exception"
        | [Number lhs ; Number rhs] -> Number <| lhs / rhs
        | [_          ; _         ] -> raise  <| EvaluatorException "div: Invalid arguments"
        | _                         -> raise  <| EvaluatorException "div: Invalid number of arguments"
    
    let private modulo (values: Value list): Value =
        match values with
        | [Number _   ; Number 0  ] -> raise  <| EvaluatorException "mod: Floating point exception"
        | [Number lhs ; Number rhs] -> Number <| lhs % rhs
        | [_          ; _         ] -> raise  <| EvaluatorException "mod: Invalid arguments"
        | _                         -> raise  <| EvaluatorException "mod: Invalid number of arguments"

    let private inferior (values: Value list): Value =
        match values with
        | [Number lhs ; Number rhs] -> lhs < rhs |> fromBool
        | [_          ; _         ] -> raise  <| EvaluatorException "(<): Invalid arguments"
        | _                         -> raise  <| EvaluatorException "(<): Invalid number of arguments"

    let private cons (values: Value list): Value =
        match values with
        | [List l; Nil   ] -> List l
        | [lhs   ; List l] -> List <| lhs :: l
        | [lhs   ; rhs   ] -> List [lhs ; rhs]
        | _                -> raise  <| EvaluatorException "cons: Invalid number of arguments"
   
    let private car (values: Value list): Value =
        match values with
        | [List (l :: _)] -> l
        | _               -> raise <| EvaluatorException "car: invalid argument"
        
    let private cdr (values: Value list): Value =
        match values with
        | [List [_ ; v]]   -> v
        | [List (_ :: xs)] -> List xs
        | _                -> raise <| EvaluatorException "cdr: invalid argument"
        
    let rec private cond (ctx: Context) (values: Expression list): Value =
        let cond' (ctx: Context) (value: Value) (expr: Expression) (xs: Expression list): Value =
            match value with
            | String "#t" -> cond ctx xs
            | _           -> evaluateExpr ctx expr
            
        match values with
        | Seq [expr ; ret] :: xs -> cond' ctx (evaluateExpr ctx expr) ret xs
        | _                      -> raise <| EvaluatorException "cond: invalid branch"
    
    let private eq (values: Value list): Value =
        match values with
        | [Number lhs ; Number rhs] -> lhs = rhs |> fromBool
        | [String lhs ; String rhs] -> lhs = rhs |> fromBool
        | [Nil        ; Nil       ] -> fromBool true
        | [_          ; _         ] -> fromBool false
        | _                         -> raise <| EvaluatorException "eq?: Invalid number of arguments"
    
    let private atom (values: Value list): Value =
        match values with
        | []         -> raise <| EvaluatorException "atom?: No argument"
        | [ List _ ] -> fromBool false
        | _          -> fromBool true
        
    let private lambda (_: Context) (exprs: Expression list): Value =
        let lambda' (seq: Expression) (f: Expression): Value =
            match seq with
            | Seq args -> Defined (List.map asAtom args, f) |> Function
            | _        -> raise <| EvaluatorException "lambda: Invalid argument"
            
        match exprs with
        | [args ; func] -> lambda' args func
        | _             -> raise <| EvaluatorException "lambda: Invalid number of argument"
    
    let private slet (ctx: Context) (exprs: Expression list): Value =
        let rec letContext (ctx: Context) (exprs: Expression list): Context =
            match exprs with
            | Seq [ Atom key ; value ] :: xs -> letContext (ctx.Add (key,evaluateExpr ctx value)) xs
            | []                             -> ctx
            | _                              -> raise <| EvaluatorException "let: Invalid variable declaration"
        
        match exprs with
        | [Seq defs ; expr] -> evaluateExpr (letContext ctx defs) expr
        | _                 -> raise <| EvaluatorException "let: Invalid number of arguments"
    
    let private quote (ctx: Context) (exprs: Expression list): Value =
        match exprs with
        | [expr] -> evaluateQuoted expr
        | _      -> raise <| EvaluatorException "quote: Invalid arguments"
    
    let private builtins = [
        "+",      Function <| Builtin add
        "-",      Function <| Builtin sub
        "*",      Function <| Builtin mult
        "div",    Function <| Builtin division
        "<",      Function <| Builtin inferior
        "eq?",    Function <| Builtin eq
        "atom?",  Function <| Builtin atom
        "cons",   Function <| Builtin cons
        "car",    Function <| Builtin car
        "cdr",    Function <| Builtin cdr
        "cond",   Function <| Spe cond
        "lambda", Function <| Spe lambda
        "let",    Function <| Spe slet
        "quote",  Function <| Spe quote
        "#t",     String "#t"
        "#f",     String "#f"
    ]
    
    let public baseContext = Map.ofList builtins