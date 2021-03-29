namespace lerve.Services.LispService

open System
open lerve.Services.CommonService.List

type public Token =
        | OpenContext
        | CloseContext
        | Quote
        | Word of string
        | Comment of string
        
exception private LexerException of string

module Lexer =
    let rec public lexer (s: string) =
        let rec lex (s: Char list): Token list =            
            let collectComment (s: Char list): Token list =
                let collectComment' (com: Char list, xs: Char list): Token list =
                    (Seq.toArray com |> String |> Comment) :: lex xs

                let isNewLine c =
                    c = '\n'

                collectComment' <| List.partition isNewLine s

            let collectStringLiteral (s: Char list): Token list =
                let rec collectStringLiteral' (ret: Char list, s: Char list): Char list * Char list =
                    match s with
                    | []                 -> raise <| LexerException "Unmatched quote"
                    | '"'  :: xs         -> (List.rev ret, xs)
                    | '\\' :: char :: xs -> collectStringLiteral' (char :: '\\' :: ret, xs)
                    | char :: xs         -> collectStringLiteral' (char :: ret, xs)

                let collectStringLiteral'' (lhs: Char list, rhs: Char list): Token list =
                    Quote :: (Seq.toArray lhs |> String |> Word) :: lex rhs
                    
                collectStringLiteral'' <| collectStringLiteral' ([], s)
              
            let collectWord (s: Char list): Token list =
                let isDelimiter (c: Char): bool =
                    match c with
                    | '"'  -> true
                    | '('  -> true
                    | ')'  -> true
                    | ';'  -> true
                    | '''  -> true
                    | char -> Char.IsWhiteSpace char

                let collectWord' (w: Char list, xs: Char list): Token list =
                    (Seq.toArray w |> String |> Word) :: lex xs

                collectWord' <| splitOn isDelimiter s

            match s with
            | []        -> []
            | '(' :: xs -> OpenContext  :: lex xs
            | ')' :: xs -> CloseContext :: lex xs
            | '"' :: xs -> collectStringLiteral xs
            | ';' :: xs -> collectComment xs
            | ''' :: xs -> Quote :: lex xs
            | '`' :: xs -> Quote :: lex xs
            | l   :: xs -> if Char.IsWhiteSpace l then lex xs else collectWord s
            
        try
            Ok <| (lex << Seq.toList) s
        with
        | LexerException cause -> Error <| "Lexer Exception: " + cause
        | _                    -> Error "Internal Error"
