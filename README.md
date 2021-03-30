# lerve
Simple [Scheme-like Lisp](https://fr.wikipedia.org/wiki/Scheme) interpreter web API, written in F# .NET Core.

## Endpoint

### `POST /code`

Request:
```json
{
	"source": "(define (toto a b) (+ a b)) (toto 1 2) (toto 2 3)"
}
```

Response `200 OK`:
```json
{
	"type": "Success",
	"values": [
		{
			"type": "Number",
			"values": 3
		},
		{
			"type": "Number",
			"values": 5
		}
	]
}
```
---
Response `400 Bad Request`:
```json
{
	"type": "Failure",
	"values": {
		"source": "https://localhost:5001/Code",
		"message": "Evaluator Exception: toto is not a variable"
	}
}
```

## Syntax
**Lerve**'s syntax is build with `Sequences` and `Atoms`. A *Sequence* is written with `(` and `)`. And every other elements are considered *Atoms*.

For example, a call to `+` with the arguments `1` and `2` will be `(+ 1 2)`. Indeed, a call to a function is essentially a *Sequence* with the **name** of the function as **first element** and the **rest** as its **arguments**. 

A *Sequence* can be returned with the help of the  `'` (quote) operator, for example, if you wan't a list of 1, 2 and 3  you can declare it as `'(1 2 3)`. If you don't use the quote, **lerve** will try to evaluate the sequence as a call. Moreover if you wan't to use a function as an argument, you can quote it and it will not be  evaluated. 

By design, `'toto` and  `"toto"` are the same but `'1` and `"1"` are'nt.

When a `;` is on a line it means all the characters at the right of the delimiter are considered as a comment.

Boolean type is represented as `#t` and `#f`.

## How to `define` a function ?
Adding a function to your program's context is actually very simple. 
`(define (NAME ARGS...) (IMPLEMENTATION...))` will add the function `NAME` which takes `ARGS...` as arguments to the global context.

## Builtins

| Builtin | Signature                          | Example                                 |
|:-------:|:---------------------------------- |:--------------------------------------- |
|    +    | int -> int -> ... -> int           | `(+ 1 2 ...)`                           |
|    -    | int -> int -> ... -> int           | `(- 1 2 ...)`                           |
|    *    | int -> int -> ... -> int           | `(* 1 2 ...)`                           |
|   div   | int -> int -> int                  | `(div 1 2) ; 0`                         |
|    <    | int -> int -> boolean              | `(< 1 2) ; #t`                          |
|   eq?   | a -> a -> boolean                  | `(eq? 1 2) ; #f`                        |
|  atom?  | List -> boolean                    | `(atom? '()) ; #t`                      |
|  cons   | a -> List/a -> List                | `(cons 1 (cons 2 3)) ; (1 2 3)`         |
|   car   | List -> a                          | `(car '(1 2 3)) ; 1`                    |
|   cdr   | List -> List                       | `(cdr '(1 2 3))) ; (2 3 Nil)`           |
|  cond   | List[boolean, a] -> a              | `(cond (#f 1) (#t (+ 1 1))) ; 2`        |
| lambda  | Name -> Proto -> Impl -> Procedure | `((lambda (a b) (+ a b)) 1 2) ; 3`      |
|   let   | List[string, a] -> Impl -> x       | `(let ((a 2) (b (+ 1 2))) (+ a b)) ; 5` |
|  quote  | a -> a                             | `(quote (+ 1 2)) ; (+ 1 2)`             |



