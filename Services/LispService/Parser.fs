namespace lerve.Services.LispService

open lerve.Services.CommonService.Tuple

type public Expression =
            | Atom of string
            | Quoted of Expression
            | Seq of Expression list

exception private ParserException of string

module Parser =
    let public parser (tokens: Token list): Result<Expression list, string> =
        let rec parse (tokens: Token list): Expression list =
            let parse' (expr: Expression, tokens: Token list): Expression list =
                expr :: parse tokens
            
            let rec parseExpression (tokens: Token list): (Expression * Token list) =
                let rec collectSeq (tokens: Token list): (Expression * Token list) =
                    let rec collectSeq' (ret: Expression list, tokens: Token list): (Expression * Token list) =
                        let collectSeq'' (list: Expression list) (expr: Expression, tokens: Token list): (Expression list * Token list) =
                            (expr :: list, tokens)
                            
                        match tokens with
                        | CloseContext :: xs -> (List.rev ret |> Seq, xs)
                        | Comment _    :: xs -> collectSeq' (ret, xs)
                        | Word word    :: xs -> collectSeq' (Atom word :: ret, xs)
                        | OpenContext  :: xs -> collectSeq xs |> collectSeq'' ret |> collectSeq'
                        | Quote        :: xs -> mapFst Quoted (parseExpression xs) |> collectSeq'' ret |> collectSeq'
                        | []                 -> raise <| ParserException "Unmatched opening parenthesis"

                    collectSeq' ([], tokens)
                    
                match tokens with
                | OpenContext  :: xs -> collectSeq xs
                | Word word    :: xs -> (Atom word, xs)
                | Quote        :: xs -> mapFst Quoted (parseExpression xs)
                | Comment _    :: xs -> parseExpression xs
                | CloseContext :: _  -> raise <| ParserException "Unmatched opening parenthesis"
                | []                 -> raise <| ParserException "Empty expression"
            
            match tokens with
            | []              -> []
            | Comment _ :: xs -> parse xs
            | tokens          -> (parseExpression >> parse') tokens
        
        try
            Ok <| parse tokens
        with
        | ParserException cause -> Error <| "Parser Exception: " + cause
        | _                     -> Error "Internal Error"
        
