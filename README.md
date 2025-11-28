# Open Commissioning Assistant
The Open Commissioning Assistant is a portable Desktop Application
to connect external systems via plugins and make the data available to Unity.
The main reason for using a separate application besides Unity is the full .NET environment without any restrictions
regarding dependencies and nuget packages.

## Download Pages
* [Assistant](https://github.com/OpenCommissioning/OC_Assistant/releases/latest)
* Plugin: [TwinCAT](https://github.com/OpenCommissioning/OC_Assistant_Twincat/releases/latest)
* Plugin: [PlcSimAdvanced](https://github.com/OpenCommissioning/OC_Assistant_PlcSimAdvanced/releases/latest)
* Plugin: [OfficeLite](https://github.com/OpenCommissioning/OC_Assistant_OfficeLite/releases/latest)
* Plugin: [RobotStudio](https://github.com/OpenCommissioning/OC_Assistant_RobotStudio/releases/latest)

## Requirements
- ``Windows 10 / 11, Version 20H2`` or newer
- ``.NET Desktop Runtime 10.0.x``
- Plugins may require additional dependencies

## Plugin System
Plugins enable the Assistant to connect to external systems such as PLCs, Robot controllers and other control data sources, 
making this data available for Unity.

Typical use cases:
- Cyclic communication with PLCs
- Cyclic communication with Robot controllers
- Acyclic data handling

### Installation
1. Create a folder named `Plugins` in the directory of the `OC.Assistant.exe`.
2. Move the unpacked plugin folder into the `Plugins` folder.
3. Start the `OC.Assistant.exe` application.
4. The Assistant will search for `*.plugin` files and load compatible assemblies.

### Usage
1. Create a new instance of a plugin using the `+` button.
2. Configure the plugin instance with required data.
3. Upon successful loading, the plugin variables become available for use in Unity.

# Contributing

We welcome contributions from everyone and appreciate your effort to improve this project.
We have some basic rules and guidelines that make the contributing process easier for everyone involved.

## Submitting Pull Requests

1. For non-trivial changes, please open an issue first to discuss your proposed changes.
2. Fork the repo and create your feature branch.
3. Follow the code style conventions and guidelines throughout working on your contribution.
4. Create a pull request with a clear title and description.

After your pull request is reviewed and merged.

> [!NOTE]
> All contributions will be licensed under the project's license.

## Code Style Convention

Please follow these naming conventions in your code:

| Type           | Rule             |
|----------------|------------------|
| Private field  | _lowerCamelCase  |
| Public field   | UpperCamelCase   |
| Protected field | UpperCamelCase  |
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
