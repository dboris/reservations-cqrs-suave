open System
open Suave
open HttpApi

let config = { defaultConfig with bindings = [ HttpBinding.createSimple HTTP "127.0.0.1" 8082 ] }

[<EntryPoint>]
let main argv = 
    startWebServer config Api.main
    0
