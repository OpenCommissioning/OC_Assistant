# Change Log

All notable changes to this project will be documented in this file. See [versionize](https://github.com/versionize/versionize) for commit guidelines.

<a name="1.17.4"></a>
## [1.17.4](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.17.4) (2025-08-13)

### Bug Fixes

* ignore reveived config via API when busy or running ([f074516](https://www.github.com/OpenCommissioning/OC_Assistant/commit/f0745163c126ad1a1ed5848e3e329984daed38ce))
* move named pipe api to Assistant Core ([44bd710](https://www.github.com/OpenCommissioning/OC_Assistant/commit/44bd710f3bd2febc26b460106c8b72feaef462c1))
* upgrade sdk package reference for new API implementation ([7db5c7b](https://www.github.com/OpenCommissioning/OC_Assistant/commit/7db5c7bdff9aed503eb42957d979ceddbcc39b5b))

<a name="1.17.3"></a>
## [1.17.3](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.17.3) (2025-08-07)

### Bug Fixes

* hotfix for wrong behavior when saving plugins to the project XML file ([1a6f479](https://www.github.com/OpenCommissioning/OC_Assistant/commit/1a6f479472444a653439efd4a1c9a1c20fe46b5b))

<a name="1.17.2"></a>
## [1.17.2](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.17.2) (2025-08-07)

### Bug Fixes

* exclude empty string as valid plc variable name ([9dfbb5c](https://www.github.com/OpenCommissioning/OC_Assistant/commit/9dfbb5cc4ebf096b236614f7bf95fdc2897bc4f4))
* fix an issue where the DeviceTemplate MessageBox rejects valid inputs ([87bd1cc](https://www.github.com/OpenCommissioning/OC_Assistant/commit/87bd1cce9edf1fe73a90a54561aa992796d7e1b1))
* modify the github link name on the About page ([5178931](https://www.github.com/OpenCommissioning/OC_Assistant/commit/517893146e8e75760a9d1a8de6f411b02d001327))
* **PluginEditor:** the name of an existing plugin instance can now be changed ([eaa768b](https://www.github.com/OpenCommissioning/OC_Assistant/commit/eaa768b35a26776a47bfe1f02152aa5c5514231e))
* **PluginManager:** UI and behaviour rework ([fc92bd6](https://www.github.com/OpenCommissioning/OC_Assistant/commit/fc92bd66113910a5f6484d10fdb68016ffd5036d))

<a name="1.17.1"></a>
## [1.17.1](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.17.1) (2025-07-29)

### Bug Fixes

* update Theme package reference ([430f6b4](https://www.github.com/OpenCommissioning/OC_Assistant/commit/430f6b48eb2b7fd4ecfd2118e2159b08191fbf60))
* **DeviceTemplate:** don't close MessageBox with invalid input ([6aad0be](https://www.github.com/OpenCommissioning/OC_Assistant/commit/6aad0bece7f1fe675a49771aa950c43764486573))
* **PnGenerator:** add a feature to show the output files ([522ec43](https://www.github.com/OpenCommissioning/OC_Assistant/commit/522ec43691314e0d855c2655162f059f4335d573))
* **PnGenerator:** don't close the Scan Profinet Window with missing settings ([495dd2b](https://www.github.com/OpenCommissioning/OC_Assistant/commit/495dd2bcbd7a1cde45168c35edf9a772f3114535))

<a name="1.17.0"></a>
## [1.17.0](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.17.0) (2025-07-23)

### Features

* OC.Assistant.exe is now codesigned ([a91b3af](https://www.github.com/OpenCommissioning/OC_Assistant/commit/a91b3af52fa76d818cec8a536cd4f7041c79f2a2))
* update theme package reference to v2.1.0 ([e22297c](https://www.github.com/OpenCommissioning/OC_Assistant/commit/e22297cd3c818c17370adf014865da0595c0be7d))

### Bug Fixes

* remove Microsoft-WindowsAPICodePack-Core dependency ([ffdb2cd](https://www.github.com/OpenCommissioning/OC_Assistant/commit/ffdb2cdb9ae8e95ea5d81677ee301e8f31169b7c))
* wrong ProjectStateView when closing TwinCAT ([8191fb2](https://www.github.com/OpenCommissioning/OC_Assistant/commit/8191fb24adeaba67d18d0d691e8c1518768f12a5))
* wrong ProjectStateView when closing TwinCAT ([443abb0](https://www.github.com/OpenCommissioning/OC_Assistant/commit/443abb0361fe81c2533354051d49c946c756f91f))
* **MainWindow:** GridSplitter behaviour ([30f6c1c](https://www.github.com/OpenCommissioning/OC_Assistant/commit/30f6c1caf64309587458496945dfcda205a29669))

<a name="1.16.2"></a>
## [1.16.2](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.16.2) (2025-07-15)

### Bug Fixes

* upgrade package theme reference ([6f85992](https://www.github.com/OpenCommissioning/OC_Assistant/commit/6f85992e4fbbbb9fc5f96971449a5da175aec237))

<a name="1.16.1"></a>
## [1.16.1](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.16.1) (2025-07-12)

### Bug Fixes

* adjust the publish command to include the application icon ([4f01cf9](https://www.github.com/OpenCommissioning/OC_Assistant/commit/4f01cf9a6e30dfea23a344b6f4e70ffd1f86a51f))

<a name="1.16.0"></a>
## [1.16.0](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.16.0) (2025-07-12)

### Features

* implement new Theme package ([171c752](https://www.github.com/OpenCommissioning/OC_Assistant/commit/171c7529ca4aa50577d4274cc4f1f4490a223694))

<a name="1.15.1"></a>
## [1.15.1](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.15.1) (2025-07-09)

### Bug Fixes

* **Generator:** missing hierarchical underscore for instance call ([e3df652](https://www.github.com/OpenCommissioning/OC_Assistant/commit/e3df6528e1ee1913ecc02ca6ad3bc3efbda119f7))

<a name="1.15.0"></a>
## [1.15.0](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.15.0) (2025-07-09)

### Features

* use new async modal instead of a blocking MessageBox ([e50179d](https://www.github.com/OpenCommissioning/OC_Assistant/commit/e50179dac4e59e3e9609dd5d26873fc591903bed))

### Bug Fixes

* **Generator:** use an underscore for hierarchical separation ([323b373](https://www.github.com/OpenCommissioning/OC_Assistant/commit/323b37307dc9f0dbb55e4157bd0f00e6e2a22970))

<a name="1.14.1"></a>
## [1.14.1](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.14.1) (2025-07-04)

### Bug Fixes

* add a blur effect when showing a MessageBox ([11443e3](https://www.github.com/OpenCommissioning/OC_Assistant/commit/11443e33809bbd4fab780df644ad21812d703b85))
* all COM objects from the DTE interface will get tracked and released after usage ([ed90d1e](https://www.github.com/OpenCommissioning/OC_Assistant/commit/ed90d1eaaf7bc7463d64463673986cce3f435154))
* code optimization, no bug fixes or features ([7270627](https://www.github.com/OpenCommissioning/OC_Assistant/commit/7270627929c56599202afd7857beabd58cbc5222))
* upgrade Theme package reference ([e80a435](https://www.github.com/OpenCommissioning/OC_Assistant/commit/e80a4352070f27d6174b6b135f690a7bccf6d158))

<a name="1.14.0"></a>
## [1.14.0](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.14.0) (2025-06-20)

### Features

* add a feature to show additional info about a loaded plugin ([cb16318](https://www.github.com/OpenCommissioning/OC_Assistant/commit/cb16318155bd3af8655a1630670045184d9f4331))

### Bug Fixes

* add missing entries in case of a faulty xml file ([e4cb87b](https://www.github.com/OpenCommissioning/OC_Assistant/commit/e4cb87b3913d3febca9356deb291737c285f4693))
* show plugin assemblies on the 'About' page ([7dea304](https://www.github.com/OpenCommissioning/OC_Assistant/commit/7dea3045e8246d567ce30d687d70d5037694280b))
* typo when using the DeviceTemplate '_sPath' variable ([e7a57ef](https://www.github.com/OpenCommissioning/OC_Assistant/commit/e7a57ef677da22f71293c8e024a262bfd538b2d7))
* upgrade Theme package reference ([15eb989](https://www.github.com/OpenCommissioning/OC_Assistant/commit/15eb989deb786a8132b85a2427d6b2f06c90fbf2))
* **HelpMenu:** show link to github for sdk and theme instead of nuget ([ae676f3](https://www.github.com/OpenCommissioning/OC_Assistant/commit/ae676f353a888f3718a1b88463f03067f70a99a3))

<a name="1.13.4"></a>
## [1.13.4](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.13.4) (2025-05-28)

### Bug Fixes

* upgrade theme package reference ([c9489d9](https://www.github.com/OpenCommissioning/OC_Assistant/commit/c9489d9247863dd2906c43e353d950b252a0436c))

<a name="1.13.3"></a>
## [1.13.3](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.13.3) (2025-05-27)

### Bug Fixes

* **Generator:** allow any character for PLC variable names within backticks (`) ([a09d74d](https://www.github.com/OpenCommissioning/OC_Assistant/commit/a09d74d2cdcb891bd167de55a96a9cd1eb8c39f9))
* **Generator:** HiL modifications for backtick names ([9be21f5](https://www.github.com/OpenCommissioning/OC_Assistant/commit/9be21f5d08485ed5faa912f5dd155b1448cc9eb6))
* **PnGenerator:** optimize the structure for the failsafe program and variables ([108dd9a](https://www.github.com/OpenCommissioning/OC_Assistant/commit/108dd9af0f703c31381dffcf5e9421530b75d66b))

<a name="1.13.2"></a>
## [1.13.2](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.13.2) (2025-05-09)

### Bug Fixes

* create HiL structures automatically after profinet scan ([58cf2c3](https://www.github.com/OpenCommissioning/OC_Assistant/commit/58cf2c3c70fd6dd16b211a39676096ee7f22b2e2))
* **PnGenerator:** add missing gsd-path argument ([5bc3e58](https://www.github.com/OpenCommissioning/OC_Assistant/commit/5bc3e58132d74640cba6d813bdf4358aa547b5be))

<a name="1.13.1"></a>
## [1.13.1](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.13.1) (2025-04-30)

### Bug Fixes

* collect new DeviceIDs when specifying a GSDML file path ([5dc2408](https://www.github.com/OpenCommissioning/OC_Assistant/commit/5dc240869d3861be69c5d26b4c693e3706fef99c))
* remove duplicated finalizer ([9baa647](https://www.github.com/OpenCommissioning/OC_Assistant/commit/9baa647bdad301b08c7b074ab4572e63adb458ca))
* **PnGenerator:** update usage of new scanner tool 'OC.TcPnScanner' ([4a9f179](https://www.github.com/OpenCommissioning/OC_Assistant/commit/4a9f179c92f542254f1b5aef01f96753a2826bf9))
* **PnScanner:** remove duration property ([b2b486c](https://www.github.com/OpenCommissioning/OC_Assistant/commit/b2b486c088654678d31c1029e8e1f66d08f59211))

<a name="1.13.0"></a>
## [1.13.0](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.13.0) (2025-04-25)

### Features

* **PnGenerator:** add a setting to specify a folder with GSDML files ([9056f99](https://www.github.com/OpenCommissioning/OC_Assistant/commit/9056f99c25412235bc582d69d445389077846382))
* **PnGenerator:** implement a feature to get Vendor- and Device ID information ([6b73f94](https://www.github.com/OpenCommissioning/OC_Assistant/commit/6b73f9474ecfe80f349586d866959bbb01f0fde3))

### Bug Fixes

* open folder dialog behaviour ([e4f0280](https://www.github.com/OpenCommissioning/OC_Assistant/commit/e4f028063769aaca4e65e3d2800572942f226dff))
* show missing dependencies on third party list ([4b8d0e7](https://www.github.com/OpenCommissioning/OC_Assistant/commit/4b8d0e77b0222b69d87acd2fb437821531a9d39d))
* upgrade SDK package reference ([f11004a](https://www.github.com/OpenCommissioning/OC_Assistant/commit/f11004a94737a91e3fc17e3f18c8b1c7a1e05093))
* **PnGenerator:** log missing devices when converting an TIA aml file ([baf96a7](https://www.github.com/OpenCommissioning/OC_Assistant/commit/baf96a7a29976a6e7e80cb69146b3348307560e5))
* **PnGenerator:** use custom tcpnscanner ([f3dd2f2](https://www.github.com/OpenCommissioning/OC_Assistant/commit/f3dd2f2a61c6606f12a33366ea7dfaf94b0ec1fb))
* **PnScanner:** TIA aml file is now required ([1ad1fdd](https://www.github.com/OpenCommissioning/OC_Assistant/commit/1ad1fdd7b54113e821966f09eb0103b253edd634))

<a name="1.12.0"></a>
## [1.12.0](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.12.0) (2025-04-15)

### Features

* read the PLC port dynamically instead of using a fixed value 851 ([e9d63cf](https://www.github.com/OpenCommissioning/OC_Assistant/commit/e9d63cfd558d013b704f174aecb72dd8752623b9))
* upgrade SDK package reference ([9389b63](https://www.github.com/OpenCommissioning/OC_Assistant/commit/9389b6382a04f07b4d04966f87819107fb2150c1))

### Bug Fixes

* update theme resource keys ([0f3b80e](https://www.github.com/OpenCommissioning/OC_Assistant/commit/0f3b80e801ad4886feb03cfd2bab8f6bd6542617))
* upgrade theme package reference ([ac12a0d](https://www.github.com/OpenCommissioning/OC_Assistant/commit/ac12a0d293416f719bc6a4092487f1195142d86e))
* **Core:** optimize TryGetItem method ([139461d](https://www.github.com/OpenCommissioning/OC_Assistant/commit/139461d52ae010034af201999b679d6ae0e7e9a9))

<a name="1.11.4"></a>
## [1.11.4](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.11.4) (2025-03-25)

### Bug Fixes

* EtherCAT simulation box filtering for generic variables ([e7ff9b1](https://www.github.com/OpenCommissioning/OC_Assistant/commit/e7ff9b1e4f1d3320c49542c08855dddb0ff6d263))

<a name="1.11.3"></a>
## [1.11.3](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.11.3) (2025-03-20)

### Bug Fixes

* indentation for HiL FB calls ([2d5e3aa](https://www.github.com/OpenCommissioning/OC_Assistant/commit/2d5e3aa64d36de8b9de6a37ca2816f36e3f4df75))
* upgrade Theme package reference ([4e3abf1](https://www.github.com/OpenCommissioning/OC_Assistant/commit/4e3abf136008725a607dbcf40af1c5a7535c28b1))

<a name="1.11.2"></a>
## [1.11.2](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.11.2) (2025-03-19)

### Bug Fixes

* connection failure after opening a solution in rare occasions ([9da5fe0](https://www.github.com/OpenCommissioning/OC_Assistant/commit/9da5fe0edb6626e358428af97634c1f07634bac9))
* present available plc projects and tasks in settings ([a3191e6](https://www.github.com/OpenCommissioning/OC_Assistant/commit/a3191e6f8b29bc12438765f760d0bc3dd7475999))
* remove default.ethml file from template project ([646a556](https://www.github.com/OpenCommissioning/OC_Assistant/commit/646a55616660987150106541721609622fa1377a))

<a name="1.11.1"></a>
## [1.11.1](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.11.1) (2025-03-13)

### Bug Fixes

* change the root directory to search plugins to 'Plugins' folder ([ab81941](https://www.github.com/OpenCommissioning/OC_Assistant/commit/ab81941355218689fdff198f6af666d7e1ab7137))

<a name="1.11.0"></a>
## [1.11.0](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.11.0) (2025-03-13)

### Features

* version check to get informed about new releases ([ed2a229](https://www.github.com/OpenCommissioning/OC_Assistant/commit/ed2a229c923bdf538ae1a85e0ce284adfcd4f70e))

<a name="1.10.0"></a>
## [1.10.0](https://www.github.com/OpenCommissioning/OC_Assistant/releases/tag/v1.10.0) (2025-03-06)

### Features

* add support to set the initial solution path via command-line argument ([bf030cc](https://www.github.com/OpenCommissioning/OC_Assistant/commit/bf030cc2ee6a7cc818008aec0168c9ac0c8ed13b))

### Bug Fixes

* indicate changes on the plugins apply button ([c58f721](https://www.github.com/OpenCommissioning/OC_Assistant/commit/c58f7213d08d2ab2e464a3827c965f6b4173c496))
* update the device FB generator to match new standard ([5eed6ff](https://www.github.com/OpenCommissioning/OC_Assistant/commit/5eed6ff9e1d64238e8db5957dfd447dfac07f11d))

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

