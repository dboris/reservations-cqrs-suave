open System
open Suave
open HttpApi

[<EntryPoint>]
let main argv = 
    startWebServer defaultConfig Api.main
    0
