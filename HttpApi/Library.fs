namespace HttpApi

open Suave
open Suave.Filters
open Suave.Operators

module Api = 
    let main = choose [ GET >=> Successful.OK "Hello" ]
