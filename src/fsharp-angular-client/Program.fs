﻿// ---
// header: Tutorial
// tagline: Introducing FunScript
// ---

(**

This example shows basic features of FunScript. It demonstrates how to use the TypeScript
type provider to access standard DOM library functions and to call JavaScript libraries 
(such as JQuery). 

## Project structure

FunScript is a library that can be called in a number of ways. Most of the samples in the
`Examples` directory follow the same pattern - they are executable programs that, when
started, generate JavaScript source code (as `*.js` file) and run a simple HTTP web 
server that can be used to view the generated web.

In this mode, a typical project needs to reference `FunScript.dll` (containing the core
JavaScript translator), `FunScript.TypeScript.dll` (a type provider that generates nice
types for calling JavaScript libraries) and `FunScript.TypeScript.Interop.dll` (which 
is required by the TypeScript type provider).

All code that is translated to JavaScript needs to be marked with the `FunScript.JS`
attribute (an alias for `ReflectedDefinition`):
*)
[<FunScript.JS>]
module Page

open FunScript
open FunScript.TypeScript

(**
The `FunScript.TypeScript` namespace contains the TypeScript provider, which is 
discussed in the next section.

### Referencing the TypeScript provider

[TypeScript](http://www.typescriptlang.org/) is a language that extends JavaScript 
with types. It uses _definition files_ (`*.d.ts`) to specify the types of existing 
JavaScript libraries. FunScript can import information from TypeScript definition files 
and generate F# types for accessing the DOM and calling external JavaScript libraries.

In this file we have imported definitions for jQuery and the JavaScript DOM.
*)

(**
## Hello World

Let's now write a typical Hello world sample.
The global `window` object available in the browser can be accessed 
using `Globals.window`. When you type `Globals.window.` in F#, you will get an autocomplete
suggestion including the `alert` function:
*)

let hello () =
  Globals.window.alert("Hello world!")

(**
This snippet defines a function `hello` that can be translated to JavaScript 
(because the module is marked with the `FunScript.JS` attribute). It will be mapped
to a simple JavaScript function that calls `window.alert`.

Next, we look how to use jQuery to access elements on the page and work with them.
You can use any JavaScript library as long as there is a TypeScript definition
for it (the [Definitely Typed](https://github.com/borisyankov/DefinitelyTyped) 
contains a number of them).

The `$("...")` command known from jQuery can be invoked by typing `Globals.Dollar.Invoke("...")`.
To simplify the access to elements by ID, we can define the F# _dynamic operator_
and write just `jq?helloWorld` instead of `Globals.Dollar.Invoke("#helloWorld")`. The
following snippet gives the necessary definitions:
*)

// Allows writing jq?name for element access
let jq(selector : string) = Globals.Dollar.Invoke selector
let (?) jq name = jq("#" + name)

[<JS; JSEmit("log.info({0})")>]
let log_info (a) = failwith "never" |> ignore

[<JS; JSEmit("log.enableAll()")>]
let log_enable (b) = failwith "never" |> ignore


(**
The `?` operator takes a parameter of type `j.JQueryStatic` which is a (generated)
type that represents the type of `jQuery`. The details of the snippet are not
important - the important fact is that we can now easily access DOM elements:
*)

let main() = 
    log_enable ()
    log_info ( "application started" )
    jq?helloWorld.click(
        fun _ -> 
            log_info ( "button clicked" )
            hello() :> obj
    )

(** 
The `mainHello` function will be later called when the web page is loaded. It
gets the element with id `helloWorld` and registers the `hello` function as 
a handler for the `click` event.

## Hosting the web page

As discussed earlier, many of the sample FunScript projects use a simple launcher.
When the project is started, it generates JavaScript and hosts it via a simple
web server. This launcher automatically looks for the `main` function (and so it
finds the function in the above section). To start the launcher, the `Page.fs` 
file ends with the following line:
*)

printfn "$(document).ready(function () {\n%s\n});" <| FunScript.Compiler.compileWithoutReturn <@ main() @>
