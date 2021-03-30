namespace lerve.Services.LispService.EvaluatorDomain

open lerve.Services.LispService.ParserDomain
open lerve.Services.CommonService.Converter
open lerve.Services.CommonService.List

module rec ExpressionEvaluator =
    let invokeFunction (ctx: Context) (f: Function) (args: Value list): Value =
        let rec functionContext (ctx: Context) (symbols: string list) (values: Value list): Context =
            match (symbols, values) with
            | (symbol :: sxs, value :: vxs) -> functionContext (ctx.Add (symbol, value)) sxs vxs
            | ([]           , []          ) -> ctx
            | _                             -> raise <| EvaluatorException "Invalid number of argument"
            
        match f with
        | Builtin b -> b args
        | Defined (symbols, func) -> evaluateExpr (functionContext ctx symbols args) func
        | Spe _                   -> raise <| EvaluatorException "The impossible has happened"

    let rec public evaluateQuoted (expr: Expression): Value =
        let evaluateQuotedAtom (s: string): Value =
            match tryParseInt s with
            | Some(a) -> Number a
            | None    -> String s
        let evaluateQuotedSeq (exprs: Expression list): Value list =
            List.foldBack (append << evaluateQuoted) exprs [Nil]
                
        match expr with
        | Atom a   -> evaluateQuotedAtom a
        | Seq []   -> Nil
        | Seq s    -> List <| evaluateQuotedSeq s
        | Quoted q -> evaluateQuoted q 
    
    let public evaluateExpr (ctx: Context) (expr: Expression): Value =
        let evaluateAtom (ctx: Context) (s: string): Value =
            let evaluateInt (s: string): int =
                match tryParseInt s with
                | Some(a) -> a
                | None    -> raise (EvaluatorException $"{s} is not a variable")
                
            match ctx.TryFind s with
            | Some(a) -> a
            | None    -> evaluateInt s |> Number
        
        let evaluateSeq (ctx: Context) (exprs: Expression list): Value =
            let evaluateSeq' (ctx: Context) (value: Value) (exprs: Expression list): Value =
                let rec evaluateSeq'' (ctx: Context) (values: Value list): Value =
                    match values with
                    | Function f :: xs -> invokeFunction ctx f xs
                    | []               -> Nil
                    | _                -> raise <| EvaluatorException "Sequence is not a procedure"

                match value with
                | Function (Spe s) -> s ctx exprs
                | _                -> value :: List.map (evaluateExpr ctx) exprs |> evaluateSeq'' ctx
                
            match exprs with
            | []         -> Nil
            | expr :: xs -> evaluateSeq' ctx (evaluateExpr ctx expr) xs
        
        match expr with
        | Quoted ex  -> evaluateQuoted ex
        | Seq    exs -> evaluateSeq ctx exs
        | Atom   a   -> evaluateAtom ctx a
