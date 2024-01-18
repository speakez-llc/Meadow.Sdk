namespace MeadowApp

open Meadow.Devices
open Meadow

type MeadowApp() =
    // Change F7CoreComputeV2 to F7FeatherV2 (or F7FeatherV1) for Feather boards
    inherit App<F7CoreComputeV2>()

    override this.Initialize() =
        do Resolver.Log.Info "Initialize... (F#)"

        base.Initialize()
        
    override this.Run () =
        do Resolver.Log.Info "Run... (F#)"

        do Resolver.Log.Info "Hello, Meadow Core-Compute!"

        base.Run()