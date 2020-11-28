﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Nvidia.NvAPI
{

	// FUNCTION NAME: NvAPI_Initialize
	//
	//! This function initializes the NvAPI library (if not already initialized) but always increments the ref-counter. 
	//! This must be called before calling other NvAPI_ functions.
	//! Note: It is now mandatory to call NvAPI_Initialize before calling any other NvAPI.
	//! NvAPI_Unload should be called to unload the NVAPI Library. 
	//!
	//! SUPPORTED OS:  Windows 7 and higher
	//!
	//!
	//! \since Release: 80
	//!
	//! \return      This API can return any of the error codes enumerated in #NvAPI_Status. If there are return error codes with 
	//!              specific meaning for this API, they are listed below.
	//! \retval      NVAPI_LIBRARY_NOT_FOUND  Failed to load the NVAPI support library
	//! \sa nvapistatus
	//! \ingroup nvapifunctions

	[FunctionId(FunctionId.NvAPI_Initialize)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate NvApiStatus NvAPI_Initialize();


	///////////////////////////////////////////////////////////////////////////////
	//
	// FUNCTION NAME: NvAPI_GetErrorMessage
	//
	//! This function converts an NvAPI error code into a null terminated string.
	//!
	//! SUPPORTED OS:  Windows 7 and higher
	//!
	//!
	//! \since Release: 80
	//!
	//! \param nr      The error code to convert
	//! \param szDesc  The string corresponding to the error code
	//!
	//! \return NULL terminated string (always, never NULL)
	//! \ingroup nvapifunctions
	///////////////////////////////////////////////////////////////////////////////
	//NVAPI_INTERFACE NvAPI_GetErrorMessage(NvAPI_Status nr, NvAPI_ShortString szDesc);
	[FunctionId(FunctionId.NvAPI_GetErrorMessage)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate NvApiStatus NvAPI_GetErrorMessage([In] NvApiStatus status,
		[Out, MarshalAs(UnmanagedType.LPStr, SizeConst = 64)] StringBuilder message);


	///////////////////////////////////////////////////////////////////////////////
	//
	// FUNCTION NAME: NvAPI_GetInterfaceVersionString
	//
	//! This function returns a string describing the version of the NvAPI library.
	//!               The contents of the string are human readable.  Do not assume a fixed
	//!                format.
	//!
	//!
	//! SUPPORTED OS:  Windows 7 and higher
	//!
	//!
	//! \since Release: 80
	//!
	//! \param  szDesc User readable string giving NvAPI version information
	//!
	//! \return See \ref nvapistatus for the list of possible return values.
	//! \ingroup nvapifunctions
	///////////////////////////////////////////////////////////////////////////////
	//NVAPI_INTERFACE NvAPI_GetInterfaceVersionString(NvAPI_ShortString szDesc);
	[FunctionId(FunctionId.NvAPI_GetInterfaceVersionString)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate NvApiStatus NvAPI_GetInterfaceVersionString(
		[Out, MarshalAs(UnmanagedType.LPStr, SizeConst = 64)] StringBuilder message);




	[FunctionId(FunctionId.NvAPI_SYS_GetDriverAndBranchVersion)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate NvApiStatus NvAPI_SYS_GetDriverAndBranchVersion(out uint driverVersion,
		[Out, MarshalAs(UnmanagedType.LPStr, SizeConst = 64)] StringBuilder buildBranchString);



	///////////////////////////////////////////////////////////////////////////////
	//
	// FUNCTION NAME: NvAPI_SYS_GetChipSetInfo
	//
	//!  This function returns information about the system's chipset.
	//!
	//! SUPPORTED OS:  Windows 7 and higher
	//!
	//!
	//! \since Release: 95
	//!
	//! \retval  NVAPI_INVALID_ARGUMENT              pChipSetInfo is NULL.
	//! \retval  NVAPI_OK                           *pChipSetInfo is now set.
	//! \retval  NVAPI_INCOMPATIBLE_STRUCT_VERSION   NV_CHIPSET_INFO version not compatible with driver.
	//! \ingroup sysgeneral
	///////////////////////////////////////////////////////////////////////////////
	//NVAPI_INTERFACE NvAPI_SYS_GetChipSetInfo(NV_CHIPSET_INFO* pChipSetInfo);
	[FunctionId(FunctionId.NvAPI_SYS_GetChipSetInfo)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate NvApiStatus NvAPI_SYS_GetChipSetInfo([In, Out] IntPtr chipsetInfo);

	// FUNCTION NAME: NvAPI_Unload
	//
	//!   DESCRIPTION: Decrements the ref-counter and when it reaches ZERO, unloads NVAPI library. 
	//!                This must be called in pairs with NvAPI_Initialize. 
	//!
	//! SUPPORTED OS:  Windows 7 and higher
	//!
	//!
	//!        If the client wants unload functionality, it is recommended to always call NvAPI_Initialize and NvAPI_Unload in pairs.
	//!
	//!  Unloading NvAPI library is not supported when the library is in a resource locked state.
	//!  Some functions in the NvAPI library initiates an operation or allocates certain resources
	//!  and there are corresponding functions available, to complete the operation or free the
	//!  allocated resources. All such function pairs are designed to prevent unloading NvAPI library.
	//!
	//!  For example, if NvAPI_Unload is called after NvAPI_XXX which locks a resource, it fails with
	//!  NVAPI_ERROR. Developers need to call the corresponding NvAPI_YYY to unlock the resources, 
	//!  before calling NvAPI_Unload again.
	//!
	//! \return      This API can return any of the error codes enumerated in #NvAPI_Status. If there are return error codes with 
	//!              specific meaning for this API, they are listed below.
	//! \retval      NVAPI_API_IN_USE       Atleast an API is still being called hence cannot unload requested driver.
	//!
	//! \ingroup nvapifunctions
	[FunctionId(FunctionId.NvAPI_Unload)]
	internal delegate NvApiStatus NvAPI_Unload();


	///////////////////////////////////////////////////////////////////////////////
	//
	// FUNCTION NAME: NvAPI_DRS_CreateSession
	//
	//!   DESCRIPTION: This API allocates memory and initializes the session.
	//!
	//! SUPPORTED OS:  Windows 7 and higher
	//!
	//!
	//! \param [out]  *phSession Return pointer to the session handle.
	//!                
	//! \retval ::NVAPI_OK SUCCESS
	//! \retval ::NVAPI_ERROR: For miscellaneous errors.
	//
	///////////////////////////////////////////////////////////////////////////////
	//NVAPI_INTERFACE NvAPI_DRS_CreateSession(NvDRSSessionHandle* phSession);
	[FunctionId(FunctionId.NvAPI_DRS_CreateSession)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate NvApiStatus NvAPI_DRS_CreateSession([Out] out DRSSessionHandle phSession);


	///////////////////////////////////////////////////////////////////////////////
	//
	// FUNCTION NAME: NvAPI_DRS_DestroySession
	//
	//!   DESCRIPTION: This API frees the allocation: cleanup of NvDrsSession.
	//!
	//! SUPPORTED OS:  Windows 7 and higher
	//!
	//!
	//! \param [in] hSession Input to the session handle.
	//!                
	//! \retval ::NVAPI_OK SUCCESS
	//! \retval ::NVAPI_ERROR For miscellaneous errors.
	//
	///////////////////////////////////////////////////////////////////////////////
	//NVAPI_INTERFACE NvAPI_DRS_DestroySession(NvDRSSessionHandle hSession);
	[FunctionId(FunctionId.NvAPI_DRS_DestroySession)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate NvApiStatus NvAPI_DRS_DestroySession(DRSSessionHandle phSession);


	///////////////////////////////////////////////////////////////////////////////
	//
	// FUNCTION NAME: NvAPI_DRS_LoadSettings
	//
	//!   DESCRIPTION: This API loads and parses the settings data.
	//!
	//! SUPPORTED OS:  Windows 7 and higher
	//!
	//!
	//! \param [in] hSession  Input to the session handle.
	//!                
	//! \retval ::NVAPI_OK     SUCCESS
	//! \retval ::NVAPI_ERROR  For miscellaneous errors.
	//
	///////////////////////////////////////////////////////////////////////////////
	///NVAPI_INTERFACE NvAPI_DRS_LoadSettings(NvDRSSessionHandle hSession);
	[FunctionId(FunctionId.NvAPI_DRS_LoadSettings)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate NvApiStatus NvAPI_DRS_LoadSettings([In] DRSSessionHandle sessionHandle);


	///////////////////////////////////////////////////////////////////////////////
	//
	// FUNCTION NAME: NvAPI_DRS_GetBaseProfile
	//
	//!   DESCRIPTION: Returns the handle to the current global profile.
	//!
	//! SUPPORTED OS:  Windows 7 and higher
	//!
	//!
	//! \param [in]  hSession    Input to the session handle.
	//! \param [in]  *phProfile   Returns Base profile handle.
	//!                
	//! \retval ::NVAPI_OK     SUCCESS if the profile is found
	//! \retval ::NVAPI_ERROR  For miscellaneous errors.
	//!
	//! \ingroup drsapi
	///////////////////////////////////////////////////////////////////////////////
	//NVAPI_INTERFACE NvAPI_DRS_GetBaseProfile(NvDRSSessionHandle hSession, NvDRSProfileHandle* phProfile);
	[FunctionId(FunctionId.NvAPI_DRS_GetBaseProfile)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate NvApiStatus NvAPI_DRS_GetBaseProfile([In] DRSSessionHandle sessionHandle, [Out] out DRSProfileHandle profileHandle);



	///////////////////////////////////////////////////////////////////////////////
	//
	// FUNCTION NAME: NvAPI_DRS_FindProfileByName
	//
	//!   DESCRIPTION: This API finds a profile in the current session.
	//!
	//! SUPPORTED OS:  Windows 7 and higher
	//!
	//!
	//! \param [in]   hSession      Input to the session handle.
	//! \param [in]   profileName   Input profileName.
	//! \param [out]  phProfile     Input profile handle.
	//!                
	//! \retval ::NVAPI_OK                SUCCESS if the profile is found
	//! \retval ::NVAPI_PROFILE_NOT_FOUND if profile is not found
	//! \retval ::NVAPI_ERROR             For miscellaneous errors.
	//!
	//! \ingroup drsapi
	///////////////////////////////////////////////////////////////////////////////
	//NVAPI_INTERFACE NvAPI_DRS_FindProfileByName(NvDRSSessionHandle hSession, NvAPI_UnicodeString profileName, NvDRSProfileHandle* phProfile);
	[FunctionId(FunctionId.NvAPI_DRS_FindProfileByName)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate NvApiStatus NvAPI_DRS_FindProfileByName(
		[In] DRSSessionHandle sessionHandle,
		[MarshalAs(UnmanagedType.LPWStr, SizeConst = (int)NvAPI.UnicodeMaxString)]
			StringBuilder profileName,
		[Out] out DRSProfileHandle profileHandle);

	//internal delegate NvApiStatus NvAPI_DRS_FindProfileByName([In] DRSSessionHandle sessionHandle, [In] string profileName, [Out] out DRSProfileHandle profileHandle);




	///////////////////////////////////////////////////////////////////////////////
	//
	// FUNCTION NAME: NvAPI_DRS_CreateProfile
	//
	//!   DESCRIPTION: This API creates an empty profile.
	//!
	//! SUPPORTED OS:  Windows 7 and higher
	//!
	//!
	//! \param [in]  hSession        Input to the session handle.
	//! \param [in]  *pProfileInfo   Input pointer to NVDRS_PROFILE.
	//! \param [in]  *phProfile      Returns pointer to profile handle.
	//!                
	//! \retval ::NVAPI_OK     SUCCESS
	//! \retval ::NVAPI_ERROR  For miscellaneous errors.
	//!
	//! \ingroup drsapi
	///////////////////////////////////////////////////////////////////////////////
	//NVAPI_INTERFACE NvAPI_DRS_CreateProfile(NvDRSSessionHandle hSession, NVDRS_PROFILE* pProfileInfo, NvDRSProfileHandle* phProfile);
	[FunctionId(FunctionId.NvAPI_DRS_CreateProfile)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate NvApiStatus NvAPI_DRS_CreateProfile([In] DRSSessionHandle sessionHandle, [In] ref DRSProfile profile, [Out] out DRSProfileHandle profileHandle);



	// FUNCTION NAME: NvAPI_DRS_GetApplicationInfo
	//
	//!   DESCRIPTION: This API gets information about the given application.  The input application name
	//!                must match exactly what the Profile has stored for the application. 
	//!                This function is better used to retrieve application information from a previous
	//!                enumeration.
	//!
	//! SUPPORTED OS:  Windows 7 and higher
	//!
	//!
	//! \param [in]   hSession       Input to the session handle.
	//! \param [in]   hProfile       Input profile handle.
	//! \param [in]   appName        Input application name.
	//! \param [out]  *pApplication  Returns NVDRS_APPLICATION struct with all the attributes.
	//!                
	//! \return  This API can return any of the error codes enumerated in #NvAPI_Status. 
	//!          If there are return error codes with specific meaning for this API, 
	//!          they are listed below.
	//! \retval ::NVAPI_EXECUTABLE_PATH_IS_AMBIGUOUS   The application name could not 
	//                                                single out only one executable.
	//! \retval ::NVAPI_EXECUTABLE_NOT_FOUND           No application with that name is found on the profile.
	//!
	//! \ingroup drsapi
	///////////////////////////////////////////////////////////////////////////////
	//NVAPI_INTERFACE NvAPI_DRS_GetApplicationInfo(NvDRSSessionHandle hSession, NvDRSProfileHandle hProfile, NvAPI_UnicodeString appName, NVDRS_APPLICATION* pApplication);
	[FunctionId(FunctionId.NvAPI_DRS_GetApplicationInfo)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate NvApiStatus NvAPI_DRS_GetApplicationInfo(
		[In] DRSSessionHandle hSession,
		[In] DRSProfileHandle hProfile,
		[MarshalAs(UnmanagedType.LPWStr, SizeConst = (int)NvAPI.UnicodeMaxString)]
		StringBuilder appName,
		[In, Out] IntPtr hApp);


	///////////////////////////////////////////////////////////////////////////////
	//
	// FUNCTION NAME: NvAPI_DRS_CreateApplication
	//
	//!   DESCRIPTION: This API adds an executable name to a profile.
	//!
	//! SUPPORTED OS:  Windows 7 and higher
	//!
	//!
	//! \param [in]  hSession       Input to the session handle.
	//! \param [in]  hProfile       Input profile handle.
	//! \param [in]  *pApplication  Input NVDRS_APPLICATION struct with the executable name to be added.
	//!                
	//! \retval ::NVAPI_OK     SUCCESS
	//! \retval ::NVAPI_ERROR  For miscellaneous errors.
	//!
	//! \ingroup drsapi
	///////////////////////////////////////////////////////////////////////////////
	//NVAPI_INTERFACE NvAPI_DRS_CreateApplication(NvDRSSessionHandle hSession, NvDRSProfileHandle hProfile, NVDRS_APPLICATION* pApplication);

	[FunctionId(FunctionId.NvAPI_DRS_CreateApplication)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	//internal delegate NvApiStatus NvAPI_DRS_CreateApplication([In] DRSSessionHandle hSession, [In] DRSProfileHandle hProfile, [In] ref DRSApplicationV1 app);
	internal delegate NvApiStatus NvAPI_DRS_CreateApplication([In] DRSSessionHandle hSession, [In] DRSProfileHandle hProfile, [In, Out] IntPtr app);

	///////////////////////////////////////////////////////////////////////////////
	//
	// FUNCTION NAME: NvAPI_DRS_SetSetting
	//
	//!   DESCRIPTION: This API adds/modifies a setting to a profile.
	//!
	//! SUPPORTED OS:  Windows 7 and higher
	//!
	//!
	//! \param [in]  hSession     Input to the session handle.
	//! \param [in]  hProfile     Input profile handle.
	//! \param [in]   *pSetting   Input NVDRS_SETTING struct pointer.
	//!                
	//! \retval ::NVAPI_OK     SUCCESS
	//! \retval ::NVAPI_ERROR  For miscellaneous errors.
	//!
	//! \ingroup drsapi
	///////////////////////////////////////////////////////////////////////////////
	//NVAPI_INTERFACE NvAPI_DRS_SetSetting(NvDRSSessionHandle hSession, NvDRSProfileHandle hProfile, NVDRS_SETTING* pSetting);//
	[FunctionId(FunctionId.NvAPI_DRS_SetSetting)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate NvApiStatus NvAPI_RS_SetSettingDelegate(DRSSessionHandle phSessio, DRSProfileHandle hProfile, ref DRSSetting pSetting);


	///////////////////////////////////////////////////////////////////////////////
	//
	// FUNCTION NAME: NvAPI_DRS_SaveSettings
	//
	//!   DESCRIPTION: This API saves the settings data to the system.
	//!
	//! SUPPORTED OS:  Windows 7 and higher
	//!
	//!
	//! \param [in] hSession  Input to the session handle.
	//!                
	//! \retval ::NVAPI_OK    SUCCESS
	//! \retval ::NVAPI_ERROR For miscellaneous errors.
	//
	///////////////////////////////////////////////////////////////////////////////
	//NVAPI_INTERFACE NvAPI_DRS_SaveSettings(NvDRSSessionHandle hSession);
	[FunctionId(FunctionId.NvAPI_DRS_SaveSettings)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate NvApiStatus NvAPI_DRS_SaveSettings([In] DRSSessionHandle sessionHandle);


}
