module lerve.Services.CommonService.Result

let (<*>) (left: Result<'a, 'c>) (right: ('a -> Result<'b, 'c>)): Result<'b, 'c> =
    match left with
    | Ok(a)    -> right a
    | Error(c) -> Error(c)