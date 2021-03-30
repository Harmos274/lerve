module lerve.Services.CommonService.List

let rec splitOn (f: 'a -> bool) (s:'a list) : ('a list * 'a list) =
    let rec splitOn' (s: 'a list, rest: 'a list) : ('a list * 'a list) =
        match rest with
        | []      -> (s, [])
        | l :: xs -> if not <| f l then splitOn' (l :: s, xs) else (s, rest)
    
    let (fst, scd) = splitOn' ([], s) in (List.rev fst, scd)

let append (e: 'a) (l: 'a list): 'a list = e::l