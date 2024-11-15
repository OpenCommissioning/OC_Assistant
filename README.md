# Open Commissioning Assistant
The Open Commissioning Assistant is an application designed to facilitate engineering processes based on the Open Commissioning Framework. It specializes in implementing various plugins for virtual commissioning and optimizing TwinCAT project workflows.

The goal of this application is to keep it as simple as possible with a clean design and let it only do what it needs to do.
It is currently optimized for a connection to a TwinCAT3 Shell. Supported Versions:
- ``TcXaeShell.DTE.15.0`` (based on VS2017, TwinCAT v3.1.4024.x)
- ``TcXaeShell.DTE.17.0`` (based on VS2022, TwinCAT v3.1.4026.x)


## Target framework
- ``.NET 8.0``


## Used packages:
- ``OC.Assistant.Sdk``, see [nuget package](https://www.nuget.org/packages/OC.Assistant.Sdk)
- ``OC.Assistant.Theme``, see [nuget package](https://www.nuget.org/packages/OC.Assistant.Theme)
- ``envdte``, see [nuget package](https://www.nuget.org/packages/envdte)
- ``Beckhoff.TwinCAT.Ads``, see [nuget package](https://www.nuget.org/packages/Beckhoff.TwinCAT.Ads)
- ``TCatSysManagerLib``, see [nuget package](https://www.nuget.org/packages/TCatSysManagerLib)


## Installation

Download the _OC.Assistant.exe_ from the Release page and start it.
``.NET 8.0`` is required to run the application.


## Core Functionality

The application's primary functions are:
1. Generating and updating connected TwinCAT projects
2. Connecting external systems via plugins and make this data available to the TwinCAT project.

### Handling TwinCAT Solutions

The `File` menu contains functions regarding TwinCAT Solutions:

![FileMenu.png](Documentation%2FImages%2FFileMenu.png)

- _Connect_: select and connect to a running TwinCAT Shell. The status of the connection and the mode of the connected TwinCAT System is diplayed in the bottom right corner.
- _Open Solution_: select an existing TwinCAT Solution which gets opened in a new TwinCAT Shell and connected to by the Assistant application
- _Create Solution_: create an empty TwinCAT solution preconfigured with the plc task and an empty configuration
- _Open Config (readonly)_: 
Opens an existing [configuration](#configuration-file) of a twincat project. 
This opens the plugin configurations in the Assistant and enables to start and stop them. it does 
not open a TwinCAT Shell, it connects to a running twincat system. 
therefore the configuration of the plugins cant be changed, making it readonly. 
this is commonly done when the engineering process of 
the twincat solution is finished so it is not required to have the solution explorer open. 

### Updating Projects
The `Project` menu contains functions to update the connected TwinCAT Solution. **Note** that this menu is only active when the Assistant is 
connected to a TwinCAT solution and this solution is in _Config Mode_.

![ProjectMenu.png](Documentation%2FImages%2FProjectMenu.png)


- _Recreate Project_: Updates the conected TwinCAT project based on the current [configuration](#configuration-file). When the configuration includes _device_ entries, 
this function automatically generates corresponding behavior models within the connected TwinCAT solution. __Note__ that this function is automatically called when
using the _Update TwinCAT Project_ function of the `Client` component in the [Open Commissioning Unity package](https://github.com/OpenCommissioning/OC_Unity_Core), so in most cases this funcion does not need to be called via this menu.

![DeviceGeneration.gif](Documentation%2FImages%2FDeviceGeneration.gif)


- _Recreate Plugins_: Reads the plugin configurations from the current [configuration](#configuration-file) and creates the correspondig [GVLs](#plugin-system) in the connected TwinCAT project and creates their configuration interface in the Assistant application. 

- _Update Task_: Generates Input and Output variables based on the _Devices_ of the _PLC Project_ in the _Task_ of the connected TwinCAT solution
and links those to the corresponding variables in the _PLC Project_.
*Note* that the _PLC Project_ needs to be build successfully before updating the task.

![TaskGeneration.gif](Documentation%2FImages%2FTaskGeneration.gif)


- _Settings_: Opens The Settings for the connected TwinCAT solution

![ProjetctSettings.png](Documentation%2FImages%2FProjectSettings.png)

  - _PlcName_: specifies the name of the PLC project in the connected solution in which the device models should be generated in when using _Update Project_

  - _PlcTaskName_: specifies the name of the Task in which the input and output variables should be generated in when using _Update Task_

  - _Task AutoUpdate_: if checked, the TwinCAT task gets updated automatically with the new input and ouptut variables when the _PLC Project_ is build.



### Profinet 

The `Profinet` menu provides the function to automatically scan for Profinet devices on a selected network interface and create corresponding
Profinet device nodes in the connected TwinCAT solution.
**Note**: When using it for the first time, the ``dsian.TcPnScanner`` tool has to be installed. This can be done via the Assistant application itself
by clicking on __Install dsian.TcPnScanner__ in the ``Profinet`` menu.
After successful installation, the function _Scan Profinet_ function becomes available.

![ProfinetMenu.png](Documentation%2FImages%2FProfinetMenu.png)

To scan for Profinet devices:

1. Click "Scan Profinet"
2. Enter a name for the Profinet Node in TwinCAT
3. Select the network adapter for scanning (must have TwinCAT Real-Time driver installed)
4. Optionally, specify a .hwml file
5. Click "Start" to begin the scan

The scan will create Profinet nodes in the connected TwinCAT solution for each detected device.

![ScanProfinet.png](Documentation%2FImages%2FScanProfinet.png)

## Plugin System
Plugins enable the Assistant to connect to external systems such as PLCs, robot controllers, and other control data sources, making this data available to the TwinCAT solution via Global Variable Lists (_GVLs_). 
**Note**: at the moment there are no plugins released yet.

![OS_System](Documentation%2FImages%2FAssistant_Plugins_dark.png#gh-dark-mode-only)
![OS_System](Documentation%2FImages%2FAssistant_Plugins_light.png#gh-light-mode-only)

There are plugins for:
  - Cyclic communication with various virtual PLCs
  - Cyclic communication with various virtual Robot controllers
  - Acyclic data handling (RecordData)
  - Modbus Server and Client communication

### Installation
1. Locate the Plugins folder in the assistant's installation directory
2. Move plugin files into this folder
3. The plugin becomes available for use within the application

### Usage
1. Create a new instance of a plugin using the `+` button
2. Configure the plugin instance with required data
3. Upon successful loading, the assistant generates a _GVL_ for the plugin's interface
4. The generated variables become available for use in TwinCAT

![PluginUsage.gif](Documentation%2FImages%2FPluginUsage.gif)

### Configuration File

The `OC.Assistant.xml` file in the project folder of the TwinCAT solution serves as the base for generating objects in the TwinCAT solution.
It gets created the first time the Assistant connects to the solution.
This configuration includes all plugin connections and _devices_ of the project for which behavior models should be generated

The plugin connection section is populated by adding plugins via the Assistant application itself.
The device section is populated by using the "Update TwinCAT Project" function of the Client component in the Unity package.

This is an example configuration file containing a Plugin configuration and _Devices_:
```xml
<?xml version="1.0" encoding="utf-8"?>
<Config>
  <TaskAutoUpdate />
  <Plugins>
    <Plugin Name="PLC1" Type="PlcSimAdvanced" IoType="Address">
      <Parameter>
        <AutoStart>true</AutoStart>
        <PlcName>PLC_1</PlcName>
        <Identifier>1</Identifier>
        <CycleTime>10</CycleTime>
        <InputAddress>0-1023</InputAddress>
        <OutputAddress>0-1023</OutputAddress>
      </Parameter>
      <InputStructure />
      <OutputStructure />
    </Plugin>
  </Plugins>
  <Project>
    <Hil />
    <Main>
      <Group Name="Devices">
        <Group Name="Cylinders">
          <Device Name="Cylinder_1" Type="FB_Cylinder" />
          <Device Name="Cylinder_2" Type="FB_Cylinder" />
          <Device Name="Cylinder_3" Type="FB_Cylinder" />
          <Device Name="Cylinder_4" Type="FB_Cylinder" />
        </Group>
        <Group Name="Drives">
          <Device Name="Drive_Position" Type="FB_Drive" />
          <Device Name="Drive_Simple" Type="FB_Drive" />
          <Device Name="Drive_Speed" Type="FB_Drive" />
        </Group>
        <Group Name="Interactions">
          <Device Name="Button" Type="FB_Button" />
          <Device Name="Lamp" Type="FB_Lamp" />
          <Device Name="Lock" Type="FB_Lock" />
          <Device Name="Panel_1" Type="FB_Panel" />
          <Device Name="Toggle" Type="FB_Button" />
        </Group>
        <Group Name="SensorsAnalog">
          <Device Name="SensorAnalog_1" Type="FB_SensorAnalog" />
          <Device Name="SensorAnalog_2" Type="FB_SensorAnalog" />
          <Device Name="SensorAnalog_3" Type="FB_SensorAnalog" />
        </Group>
        <Group Name="SensorsBinary">
          <Device Name="SensorBinary_1" Type="FB_SensorBinary" />
          <Device Name="SensorBinary_2" Type="FB_SensorBinary" />
          <Device Name="SensorBinary_3" Type="FB_SensorBinary" />
        </Group>
      </Group>
    </Main>
  </Project>
</Config>
```

# Contributing

We welcome contributions from everyone and appreciate your effort to improve this project.
We have some basic rules and guidelines that make the contributing process easier for everyone involved.

## Submitting Pull Requests

1. For non-trivial changes, please open an issue first to discuss your proposed changes.
2. Fork the repo and create your feature branch.
3. Follow the code style conventions and guidelines throughout working on your contribution.
4. Create a pull request with a clear title and description.

After your pull request is reviewed and merged.

**Note**: All contributions will be licensed under the project's license.

## Code Style Convention

Please follow these naming conventions in your code:

| Type           | Rule             |
|----------------|------------------|
| Private field  | _lowerCamelCase  |
| Public field   | UpperCamelCase   |
| Protected field | UpperCamelCase   |
| Internal field | UpperCamelCase   |
| Property       | UpperCamelCase   |
| Method         | UpperCamelCase   |
| Class          | UpperCamelCase   |
| Interface      | IUpperCamelCase  |
| Local variable | lowerCamelCase   |
| Parameter      | lowerCamelCase   |
| Constant       | UPPER_SNAKE_CASE |

## Guidelines for Contributions

- **Keep changes focused:** Submit one pull request per bug fix or feature. This makes it easier to review and merge your contributions.
- **Discuss major changes:** For large or complex changes, please open an issue to discuss with maintainers before starting work.
- **Commit message format**: Use the [semantic-release](https://semantic-release.gitbook.io/semantic-release#commit-message-format) commit message format.
- **Write clear code:** Prioritize readability and maintainability.
- **Be consistent:** Follow existing coding styles and patterns in the project.
- **Include tests:** It is recommended to add or update tests to cover your changes.
- **Document your work:** Update relevant documentation, including code comments and user guides.

We appreciate your contributions and look forward to collaborating with you to improve this project!
