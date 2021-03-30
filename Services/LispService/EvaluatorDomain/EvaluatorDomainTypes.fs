namespace lerve.Services.LispService.EvaluatorDomain

open lerve.Services.LispService.ParserDomain

exception private EvaluatorException of string

type Context = Map<string, Value> 

and Value =
    | Function of Function
    | Number of int
    | String of string
    | List of Value list
    | Nil
    
    static member op_Explicit(src: Value) : int =
        match src with
        | Number n -> n
        | _        -> raise <| EvaluatorException $"{src.ToString()} is not a number"
        
and Function =
    | Defined of string list * Expression
    | Builtin of (Value list -> Value)
    | Spe of (Context -> Expression list -> Value)
