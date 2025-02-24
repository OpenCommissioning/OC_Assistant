# Change Log

All notable changes to this project will be documented in this file. See [versionize](https://github.com/versionize/versionize) for commit guidelines.

<a name="1.9.1"></a>
## [1.9.1](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.9.1) (2025-02-24)

### Bug Fixes

* upgrade SDK and Theme package references ([ebc0316](https://www.github.com/OpenCommissioning/OC_Assistant/commit/ebc0316629161ae9de1e292a83b06ae3eda26f61))

<a name="1.9.0"></a>
## [1.9.0](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.9.0) (2025-02-14)

### Features

* new feature to create OC TwinCAT devices (standardized function block and structs) ([29cdf06](https://www.github.com/OpenCommissioning/OC_Assistant/commit/29cdf067bf3f40d4f49e49c405c8073598879d61))

### Bug Fixes

* exception handling for DteSingleThread ([387dcb5](https://www.github.com/OpenCommissioning/OC_Assistant/commit/387dcb502b90da918c4f819015358e661c70eef6))
* improved algorithm for generating the TwinCAT project ([123cc30](https://www.github.com/OpenCommissioning/OC_Assistant/commit/123cc30f8ff255123eac58a1d8fc85a0b1700aa9))
* rename the target folder for device templates ([5210281](https://www.github.com/OpenCommissioning/OC_Assistant/commit/521028138027d211d2997a2509e7c716287d642c))
* selected plugin highlighting when recreating plugins via menu ([d5f252f](https://www.github.com/OpenCommissioning/OC_Assistant/commit/d5f252fdfa3feb853178d909275bcd92d215225f))
* the Assistant won't restart anymore when selecting a new project. ([5b86b82](https://www.github.com/OpenCommissioning/OC_Assistant/commit/5b86b828b1789c1905f210b46cd22c7c268398cb))
* visibility of the plugin editor scrollbar when recreating plugins via project menu. ([7f5f636](https://www.github.com/OpenCommissioning/OC_Assistant/commit/7f5f636c3ecfe39b824ecc69590042ebdb62c6ab))

<a name="1.8.3"></a>
## [1.8.3](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.8.3) (2025-02-10)

### Bug Fixes

* DTE optimization when selecting and closing the TwinCAT solution. ([d8381d9](https://www.github.com/OpenCommissioning/OC_Assistant/commit/d8381d9a0679eafd86e7fe91d789a8440072f885))

<a name="1.8.2"></a>
## [1.8.2](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.8.2) (2025-02-10)

### Bug Fixes

* upgrade visual studio shell interop package reference ([7c098d9](https://www.github.com/OpenCommissioning/OC_Assistant/commit/7c098d939abb40a51ed446f1819bbc343abdc5f4))

<a name="1.8.1"></a>
## [1.8.1](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.8.1) (2025-02-08)

### Bug Fixes

* adjust TwinCAT template project to match new generated project structure ([fdd8f2c](https://www.github.com/OpenCommissioning/OC_Assistant/commit/fdd8f2c88256875ac3ccbe2154714bfcc53283b9))
* don't create not supported VAR_INST variables in program methods ([3611a3d](https://www.github.com/OpenCommissioning/OC_Assistant/commit/3611a3d49d5457a081d8f5b6fe1257f3f02f46b3))

<a name="1.8.0"></a>
## [1.8.0](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.8.0) (2025-02-07)

### Features

* rework of project generator with new mapping feature ([b7ce809](https://www.github.com/OpenCommissioning/OC_Assistant/commit/b7ce809b6a8540e91dbd3ee2f50377b29aa883c2))
* the project generator will no longer create 'Cycle' actions. Cyclic text is now called directly in the FB implementation section. Caution: already generated TwinCAT projects will not be compatible with this version. ([633bded](https://www.github.com/OpenCommissioning/OC_Assistant/commit/633bdedb414d1680117ed3b4f5da000be30275d3))

### Bug Fixes

* don't query NedId while Assistant is busy ([820fc7d](https://www.github.com/OpenCommissioning/OC_Assistant/commit/820fc7db153edcda1a5c91b328526c78c91ba885))
* minor improvements for DTE stability ([f99972f](https://www.github.com/OpenCommissioning/OC_Assistant/commit/f99972fb98107877f863600949ced5d4ade4d0bc))
* performance and stability improvements for the TwinCAT project generator ([b401708](https://www.github.com/OpenCommissioning/OC_Assistant/commit/b401708367c2a36109d12ada070436fbc35eb827))

<a name="1.7.0"></a>
## [1.7.0](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.7.0) (2025-01-28)

### Features

* add a scan duration setting for the PnGenerator ([40f074e](https://www.github.com/OpenCommissioning/OC_Assistant/commit/40f074ea96292d7ed5dba6405b3bf9d37769456e))
* add support for Visual Studio 2017, 2019 and 2022 ([eada6e1](https://www.github.com/OpenCommissioning/OC_Assistant/commit/eada6e1c49e001b5328ee916ec7bb192bf3a760d))
* TIA aml export files can now be used to extend generated Profinet devices with IO addresses, network peers and failsafe identification ([f5d55df](https://www.github.com/OpenCommissioning/OC_Assistant/commit/f5d55df119fe97bf33a1208df3fb4b18c0fad5d8))

### Bug Fixes

* module and submodule position when parsing TIA aml files ([c7dab4e](https://www.github.com/OpenCommissioning/OC_Assistant/commit/c7dab4ec077c3630f8a6df9ba267a7af0775b2fb))
* prevent xml exception for the XtiUpdater when trying to create an existing attribute ([dc5aa8c](https://www.github.com/OpenCommissioning/OC_Assistant/commit/dc5aa8c42571023ff8fe9b49f92c00af1a63058c))
* remove usage of win api functions timeBeginPeriod and timeEndPeriod due to reported errors. high precision timing is now achieved via new StopwatchEx class in SDK version 1.6.0 or later. ([22aeaee](https://www.github.com/OpenCommissioning/OC_Assistant/commit/22aeaee2d596cbf0e847329a33245e95c5918b0a))
* replace the settings Window with the Theme MessageBox ([6c9c29f](https://www.github.com/OpenCommissioning/OC_Assistant/commit/6c9c29f854f7272246ae63634a9e7b6a75ec092f))
* show the exit code when the dsian.TcPnScanner.CLI fails. minor improvements for the PnScanner ([fcd8a9e](https://www.github.com/OpenCommissioning/OC_Assistant/commit/fcd8a9ecac6fa2e7cd6604cd09f59f28b1648b90))
* update Profisafe generator to match imported aml info ([3438694](https://www.github.com/OpenCommissioning/OC_Assistant/commit/343869471f2c44b65c58ec06f0ed9237c22c526c))
* update Profisafe generator to match new TwinCAT ProfisafeDevice ([18eeef8](https://www.github.com/OpenCommissioning/OC_Assistant/commit/18eeef86fb7c50782ea0edc1333bc2b06d7739b9))
* upgrade to SDK 1.6.0 ([d735b14](https://www.github.com/OpenCommissioning/OC_Assistant/commit/d735b14ff338c3508a99a1598cae89961d33d1b4))
* use TwinCAT automation interface instead of zip import to generate GVLs, DUTs and PRGs ([b20348a](https://www.github.com/OpenCommissioning/OC_Assistant/commit/b20348a752e3fdb84d838d721c936dc6ad4d7995))

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

