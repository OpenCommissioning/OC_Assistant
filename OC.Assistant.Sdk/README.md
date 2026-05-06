# OC.Assistant.Sdk

The official SDK for creating
[Open Commissioning Assistant](https://github.com/OpenCommissioning/OC_Assistant) plugins.

[![NuGet Status](https://img.shields.io/nuget/v/OC.Assistant.Sdk.svg)](https://www.nuget.org/packages/OC.Assistant.Sdk/)

### Quick Getting Started:
1. Create a ```Class Library``` project with target framework ```.NET 10```
2. Add the ```OC.Assistant.Sdk``` package via nuget
3. Create a public class and inherit the ```OC.Assistant.Sdk.PluginBase```
4. Create a xml file with file extension ```*.plugin``` and let it copy to the output directory:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Plugin>
    <!-- The name of the compiled dll file. -->
    <AssemblyFile>YourAssemblyName.dll</AssemblyFile>
    <!-- Optional directory to search dlls at runtime. Can be added multiple times. -->
    <AdditionalDirectory>path\to\directory1</AdditionalDirectory>
    <AdditionalDirectory>path\to\directory2</AdditionalDirectory>
    <!-- Optional url and type of the plugin repository. -->
    <RepositoryUrl>https://github.com/YourProfile/YourPluginRepo</RepositoryUrl>
    <RepositoryType>github</RepositoryType>
</Plugin>
```
To run your plugin, place your *.dll and *.plugin files in the ```Plugins``` folder of the ```OC.Assistant.exe```

Further details how to use ```Attributes```, ```Properties``` and ```Methods``` can be found within the SDK. \
You can also learn how to build your own plugin by reviewing existing ones on our [Open Commissioning GitHub page](https://github.com/OpenCommissioning). \
Also check out our [YouTube channel](https://www.youtube.com/@OpenCommissioning) for upcoming tutorials.
