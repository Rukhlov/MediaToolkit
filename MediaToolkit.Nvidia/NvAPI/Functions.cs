using System;
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
    internal delegate NvApiStatus NvAPI_Initialize();


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
        [MarshalAs(UnmanagedType.LPWStr, SizeConst = (int)NvAPI.NVAPI_UNICODE_STRING_MAX)]
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


    [FunctionId(FunctionId.NvAPI_DRS_GetApplicationInfo)]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    //internal delegate NvApiStatus NvAPI_DRS_GetApplicationInfo([In] DRSSessionHandle hSession, [In] DRSProfileHandle hProfile, [In] string appName, [In] ref NVDRS_APPLICATION_V3 app);
    internal delegate NvApiStatus NvAPI_DRS_GetApplicationInfo(
        [In] DRSSessionHandle hSession,
        [In] DRSProfileHandle hProfile,
        //[In] string appName,
        [MarshalAs(UnmanagedType.LPWStr, SizeConst = (int)NvAPI.NVAPI_UNICODE_STRING_MAX)]
            StringBuilder appName,
        [In] ref DRSApplicationV1 app);//where  T : NVDRS_APPLICATION_V1;



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
    internal delegate NvApiStatus NvAPI_DRS_CreateApplication([In] DRSSessionHandle hSession, [In] DRSProfileHandle hProfile, [In] ref DRSApplicationV1 app);


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



    [FunctionId(FunctionId.NvAPI_DRS_SaveSettings)]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate NvApiStatus NvAPI_DRS_SaveSettings([In] DRSSessionHandle sessionHandle);


}
