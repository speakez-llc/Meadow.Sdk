namespace MeadowApplication.Template

open System
open System.Threading.Tasks
open Meadow
open Meadow.Foundation.Displays
open Program

type MeadowApp() =
    inherit App<Desktop>()

    let create deviceDisplay = new DisplayController(deviceDisplay)

    override this.Initialize() =
        Resolver.Log.Info(sprintf "Initializing %s" (this.GetType().Name))
        Resolver.Log.Info(sprintf " Platform OS is a %s" (this.Device.PlatformOS.GetType().Name))
        Resolver.Log.Info(sprintf " Platform: %s" (this.Device.Information.Platform))
        Resolver.Log.Info(sprintf " OS: %s" (this.Device.Information.OSVersion))
        Resolver.Log.Info(sprintf " Model: %s" (this.Device.Information.Model))
        Resolver.Log.Info(sprintf " Processor: %s" (this.Device.Information.ProcessorType))

        let displayController = new DisplayController(this.Device.Display.Value)

        base.Initialize()

    override this.Run() =
        this.ExecutePlatformDisplayRunner()
        Task.CompletedTask

    member private this.ExecutePlatformDisplayRunner() =
#if WINDOWS
        match this.Device.Display.Value with
        | :? System.Windows.Forms.Form as display -> System.Windows.Forms.Application.Run(display)
        | _ -> ()
#else
        match this.Device.Display.Value with
        | :? GtkDisplay as display -> display.Run()
        | _ -> ()
#endif
