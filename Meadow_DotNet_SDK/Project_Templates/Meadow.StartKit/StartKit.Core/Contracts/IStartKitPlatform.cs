﻿using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Buttons;

namespace StartKit.Core.Contracts;

public interface IStartKitPlatform
{
    // basic hardware
    IButton? GetUpButton();
    IButton? GetDownButton();
    IButton? GetLeftButton();
    IButton? GetRightButton();

    // complex hardware
    ITemperatureSensor? GetTemperatureSensor();
    IHumiditySensor? GetHumiditySensor();
    IGraphicsDisplay? GetDisplay();

    // platform-dependent services
    IOutputService GetOutputService(); // required service
    IBluetoothService? GetBluetoothService(); // optional service
}
