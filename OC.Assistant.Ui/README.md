# OC.Assistant.Ui

The UI package of the [Open Commissioning Assistant](https://github.com/OpenCommissioning/OC_Assistant), based on [Avalonia](https://www.nuget.org/packages/Avalonia) 
and the [FluentTheme](https://www.nuget.org/packages/Avalonia.Themes.Fluent) style.

[![NuGet Status](https://img.shields.io/nuget/v/OC.Assistant.Ui.svg)](https://www.nuget.org/packages/OC.Assistant.Ui/)

### How to use:
1. Create an ```Avalonia Application``` project with target framework ```.NET10```
2. Add the ```OC.Assistant.Ui``` package via nuget
3. Add the Styles of the UI package to your `App.xaml`: 
```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:ui="https://github.com/opencommissioning"
             x:Class="OC.Assistant.App">
    <Application.Styles>
        <ui:Styles/>
    </Application.Styles>
</Application>
```

For more details visit the [GitHub Repository](https://github.com/OpenCommissioning/OC_Assistant) of this package.