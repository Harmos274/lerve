module lerve.Services.CommonService.Tuple

let public mapFst (f: 'a -> 'b) (a: 'a, c: 'c): ('b * 'c) = (f a, c)
