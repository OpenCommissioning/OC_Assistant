# Change Log

All notable changes to this project will be documented in this file. See [versionize](https://github.com/versionize/versionize) for commit guidelines.

<a name="1.6.0"></a>
## [1.6.0](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.6.0) (2025-01-08)

### Features

* Assistant now automatically connects to the target NetId of the connected solution ([4743470](https://www.github.com/OpenCommissioning/OC_Assistant/commit/4743470dbfd5ebb194dce83eb16b491e8e13928a))
* move the solution path label to the TcStateIndicator class (to the bottom right) ([e6166e1](https://www.github.com/OpenCommissioning/OC_Assistant/commit/e6166e169c5a68255cc2ade2e372304ff6a7a303))
* update TcState and TcStateIndicator to show the target AmsNetId ([982fa9b](https://www.github.com/OpenCommissioning/OC_Assistant/commit/982fa9b3c1a68978b11e19d38f6975601f49b3f9))
* upgrade Sdk and Theme nuget packages ([61cfcdf](https://www.github.com/OpenCommissioning/OC_Assistant/commit/61cfcdf98a559c7974578031c41c4313eb7b7ef2))

### Bug Fixes

* add missing exception handling in FileMenu methods ([c36a198](https://www.github.com/OpenCommissioning/OC_Assistant/commit/c36a198d31fa4f16f29cb38c8353e41e2cccbd2b))
* better implementation of the TcState class ([affc99d](https://www.github.com/OpenCommissioning/OC_Assistant/commit/affc99d95e214ad36e22402049900d7ae047180c))
* remove menu 'Open Config (readonly)' ([632e5f6](https://www.github.com/OpenCommissioning/OC_Assistant/commit/632e5f6a8d9bdd337b8c5ca51fd1659251b4b497))
* TcState class improvements ([a755177](https://www.github.com/OpenCommissioning/OC_Assistant/commit/a755177ecdc172ff837781fda33abdf20e4f8a5b))

<a name="1.5.2"></a>
## [1.5.2](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.5.2) (2024-11-18)

### Bug Fixes

* add support for multiple EtherCAT device files (new ethml file type) ([687393a](https://www.github.com/OpenCommissioning/OC_Assistant/commit/687393ab53b660590c2fa64d7f1abd2d3a96e343))
* update sdk package reference to v1.4.3 ([cd7aca9](https://www.github.com/OpenCommissioning/OC_Assistant/commit/cd7aca945dd80dc6a0e82fc77398364939d9e971))
* update twincat template project to match new *.ethml files ([fc233c5](https://www.github.com/OpenCommissioning/OC_Assistant/commit/fc233c5aaa5f6a24c5d4088ff21501d12ab0d275))

<a name="1.5.1"></a>
## [1.5.1](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.5.1) (2024-11-15)

### Bug Fixes

* add a filter for the 'simulation_interface' attribute to the task generator ([2c85862](https://www.github.com/OpenCommissioning/OC_Assistant/commit/2c858621278f2715099fe9d15298bb27daa26dce))
* add support for the new TwinCAT shell based on VS2022 ([ed43793](https://www.github.com/OpenCommissioning/OC_Assistant/commit/ed43793b51c3b5ae0a9558b4b01591b47b249221))
* filter by attribute only, not 'MAIN' anymore ([f7e951b](https://www.github.com/OpenCommissioning/OC_Assistant/commit/f7e951bbf7fca484d5b5072d1dcf89f975d48c38))

<a name="1.5.0"></a>
## [1.5.0](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.5.0) (2024-11-13)

### Features

* architectural changes, re-release ([c97327e](https://www.github.com/OpenCommissioning/OC_Assistant/commit/c97327eeb4e0da451518967877b08b9573a01734))

