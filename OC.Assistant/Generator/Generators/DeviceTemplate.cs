﻿using OC.Assistant.Core;
using OC.Assistant.Sdk;
using TCatSysManagerLib;

namespace OC.Assistant.Generator.Generators;

/// <summary>
/// Generator for device templates.
/// </summary>
public static class DeviceTemplate
{
	/// <summary>
	/// Creates a device template.
	/// </summary>
	/// <param name="parent">The parent <see cref="ITcSmTreeItem"/>. Usually the plc project or a plc folder.</param>
	/// <param name="name">The name of the device.</param>
	public static void Create(ITcSmTreeItem parent, string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			Logger.LogWarning(typeof(DeviceTemplate), "Name must not be empty");
			return;
		}
		
		if (!name.IsPlcCompatible())
		{
			Logger.LogWarning(typeof(DeviceTemplate), $"{name} is not a valid name. Allowed characters are a-z A-Z 0-9 and underscore");
			return;
		}
		
		if (parent.GetChild(name, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER) is not null)
		{
			Logger.LogWarning(typeof(DeviceTemplate), $"{name} already exists");
			return;
		}
		
		//Create folder
		if (parent.GetOrCreateChild(name, 
			    TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER) is not {} folder) return;
		
		//Create function block
		if (folder.GetOrCreateChild($"FB_{name}", 
			    TREEITEMTYPES.TREEITEMTYPE_PLCPOUFB) is not {} fb) return;
		
		//Create structs
		if (folder.GetOrCreateChild($"ST_{name}_Control", 
			    TREEITEMTYPES.TREEITEMTYPE_PLCDUTSTRUCT) is not {} control) return;
		if (folder.GetOrCreateChild($"ST_{name}_Status", 
			    TREEITEMTYPES.TREEITEMTYPE_PLCDUTSTRUCT) is not {} status) return;
		if (folder.GetOrCreateChild($"ST_{name}_Config", 
			    TREEITEMTYPES.TREEITEMTYPE_PLCDUTSTRUCT) is not {} config) return;
		
		//Create methods
		if (fb.GetOrCreateChild("InitRun", 
			    TREEITEMTYPES.TREEITEMTYPE_PLCMETHOD) is not {} initRun) return;
		if (fb.GetOrCreateChild("Cycle", 
			    TREEITEMTYPES.TREEITEMTYPE_PLCMETHOD) is not {} cycle) return;
		if (fb.GetOrCreateChild("GetControlData", 
			    TREEITEMTYPES.TREEITEMTYPE_PLCMETHOD) is not {} getControl) return;
		if (fb.GetOrCreateChild("SetStatusData", 
			    TREEITEMTYPES.TREEITEMTYPE_PLCMETHOD) is not {} setStatus) return;
		
		//Fill with content
		fb.SetContent(DECLARATION.Replace(Tags.NAME, name), IMPLEMENTATION);
		control.SetContent(CONTROL_STRUCT.Replace(Tags.NAME, name));
		status.SetContent(STATUS_STRUCT.Replace(Tags.NAME, name));
		config.SetContent(CONFIG_STRUCT.Replace(Tags.NAME, name));
		initRun.SetContent(INIT_RUN_DECLARATION, INIT_RUN_IMPLEMENTATION);
		cycle.SetContent(CYCLE_DECLARATION, CYCLE_IMPLEMENTATION);
		getControl.SetContent(GET_CONTROL_DATA_DECLARATION, GET_CONTROL_DATA_IMPLEMENTATION);
		setStatus.SetContent(SET_STATUS_DATA_DECLARATION, SET_STATUS_DATA_IMPLEMENTATION);
		
		Logger.LogInfo(typeof(DeviceTemplate), 
			$"Device template {name} created. See folder {parent.Name} in TwinCAT project");
	}
	
	private static void SetContent(this ITcSmTreeItem parent, string declText, string? implText = null)
	{
		if (parent.CastTo<ITcPlcDeclaration>() is not {} decl) return;
		decl.DeclarationText = declText;
		if (implText is null) return;
		
		if (parent.CastTo<ITcPlcImplementation>() is not {} impl) return;
		impl.ImplementationText = implText;
	}

	private const string CONTROL_STRUCT =
		"""
		{attribute 'pack_mode' := '0'}
		TYPE ST_$NAME$_Control:
		STRUCT
			//Custom device control structure (from fieldbus)...
		END_STRUCT
		END_TYPE
		""";
	
	private const string STATUS_STRUCT =
		"""
		{attribute 'pack_mode' := '0'}
		TYPE ST_$NAME$_Status:
		STRUCT
			//Custom device status structure (to fieldbus)...
		END_STRUCT
		END_TYPE
		""";
	
	private const string CONFIG_STRUCT =
		"""
		{attribute 'pack_mode' := '0'}
		TYPE ST_$NAME$_Config:
		STRUCT
			bForceMode			: BOOL; //Ignore process data from fieldbus.
			bSwapProcessData	: BOOL; //Reverse process data byte order from/to fieldbus.
			//Custom device config structure (parameters and settings)...
		END_STRUCT
		END_TYPE
		""";
	
	private const string DECLARATION = 
		"""
		(*
			For Unity communication, extend a link from the OC_Core library.
			A link contains control and status variables:
			Control		TwinCAT => Unity
			Status		TwinCAT <= Unity
			
			Basic link:
			FB_LinkDevice		Control and Status of type BYTE
		
			Extended links with additional data, all extending the FB_LinkDevice:
			FB_LinkDataByte		ControlData and StatusData of type BYTE
			FB_LinkDataWord		ControlData and StatusData of type WORD
			FB_LinkDataDWord	ControlData and StatusData of type DWORD
			FB_LinkDataLWord	ControlData and StatusData of type LWORD
			FB_LinkDataReal		ControlData and StatusData of type REAL
			
			Example:
			The device is simulating a drive and writes a calculated position (fPosition : REAL) to Unity.
			Declaration		FUNCTION_BLOCK FB_$NAME$ EXTENDS OC_Core.FB_LinkDataReal
			Implementation	THIS^.ControlData := fPosition;
		*)
		{attribute 'reflection'}
		FUNCTION_BLOCK FB_$NAME$ EXTENDS OC_Core.FB_LinkDevice
		VAR_INPUT
			pControl 			: PVOID; //Pocess data from fieldbus. Size must be >= stControl.
			pStatus 			: PVOID; //Pocess data to fieldbus. Size must be >= stStatus.
			stControl 			: ST_$NAME$_Control; //Control structure. Is read from the fieldbus.
			stConfig 			: ST_$NAME$_Config; //Config structure. Contains parameters of the device.
		END_VAR
		VAR_OUTPUT
			stStatus 			: ST_$NAME$_Status; //Status structure. Is written to the fieldbus.
		END_VAR
		VAR
			//Is used in conjunction with attribute 'reflection' to indicate the function block's name at runtime. 
			{attribute 'instance-path'}
		    {attribute 'noinit'}
		    _sPath 				: STRING;
		    
		    //Custom device variables...
		END_VAR
		""";

	private const string IMPLEMENTATION =
		"""
		InitRun();

		GetControlData();
		Cycle();
		SetStatusData();
		""";

	private const string INIT_RUN_DECLARATION =
		"""
		//Initializes device parameters. Is only called once.
		METHOD InitRun
		VAR_INST
			bInitRun : BOOL := TRUE;
		END_VAR
		""";

	private const string INIT_RUN_IMPLEMENTATION =
		"""
		IF NOT bInitRun THEN RETURN; END_IF
		bInitRun := FALSE;

		IF pControl = 0 OR pStatus = 0 THEN
			ADSLOGSTR(ADSLOG_MSGTYPE_WARN, CONCAT(_sPath, ': %s'), 'Plc address not set.');
		END_IF
		
		//Custom device initialization...
		
		""";
	
	private const string CYCLE_DECLARATION =
		"""
		//Processes cyclic device logic.
		METHOD Cycle
		""";
	
	private const string CYCLE_IMPLEMENTATION =
		"""
		//Custom device logic...
		
		""";
	
	private const string GET_CONTROL_DATA_DECLARATION =
		"""
		//Reads fieldbus process data and writes to the stControl structure.
		METHOD GetControlData
		""";
	
	private const string GET_CONTROL_DATA_IMPLEMENTATION =
		"""
		IF pControl <= 0 OR stConfig.bForceMode THEN RETURN; END_IF
		
		//This is just an example how to copy the fieldbus data to the stControl structure via memcpy
		F_Memcpy(ADR(stControl), pControl, SIZEOF(stControl), stConfig.bSwapProcessData);
		""";
	
	private const string SET_STATUS_DATA_DECLARATION =
		"""
		//Writes the stStatus structure to fieldbus process data.
		METHOD SetStatusData
		""";
	
	private const string SET_STATUS_DATA_IMPLEMENTATION =
		"""
		IF pStatus <= 0 THEN RETURN; END_IF
		
		//This is just an example how to copy the stStatus structure to the fieldbus data via memcpy
		F_Memcpy(pStatus, ADR(stStatus), SIZEOF(stStatus), stConfig.bSwapProcessData);
		""";
}