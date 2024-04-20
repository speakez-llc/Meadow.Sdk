module Program

open System
open System.Threading.Tasks

let executePlatformDisplayRunner deviceDisplay =
#if WINDOWS
    match deviceDisplay with
    | :? System.Windows.Forms.Form as display -> System.Windows.Forms.Application.Run(display)
    | _ -> ()
#else
    match deviceDisplay with
    | :? GtkDisplay as display -> display.Run()
    | _ -> ()
#endif