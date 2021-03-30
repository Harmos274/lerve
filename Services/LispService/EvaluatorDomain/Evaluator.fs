namespace lerve.Services.LispService.EvaluatorDomain

open lerve.Services.LispService.ParserDomain
open lerve.Services.LispService.EvaluatorDomain.Builtins
open lerve.Services.LispService.EvaluatorDomain.ExpressionEvaluator

module Evaluator =
    let public evaluator (exprs: Expression list): Result<Value list, string> =
        let evaluateDefine (ctx: Context) (exprs: Expression list): Context * string =
            let createFunction (args: Expression list) (f: Expression): Value =
                Function <| Defined (List.map asAtom args, f)
                
            match exprs with
            | [Atom symbol ; expr]               -> (ctx.Add (symbol, evaluateExpr ctx expr), symbol)
            | [Seq (Atom symbol :: args) ; func] -> (ctx.Add (symbol, createFunction args func), symbol)
            | _                                  -> raise <| EvaluatorException "define: Invalid arguments"
        
        let rec evaluate (ctx: Context) (exprs: Expression list): Value list =
            match exprs with
            | [] -> []
            | Seq (Atom "define" :: define) :: xs -> evaluate (evaluateDefine ctx define |> fst) xs
            | expr :: xs                          -> evaluateExpr ctx expr :: evaluate ctx xs 
        
        try
            Ok <| evaluate baseContext exprs
        with
        | EvaluatorException cause -> Error $"Evaluator Exception: {cause}"
        | _                        -> Error "Internal Error"
