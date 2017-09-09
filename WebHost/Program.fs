// Learn more about F# at http://fsharp.org
open System

[<EntryPoint>]
let main argv = 
    HttpApi.Say.hello "bob"
    0
