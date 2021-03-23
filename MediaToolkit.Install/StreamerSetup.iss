#include "ISPPBuiltins.iss"

#define ApplicationName "Polywall Streamer"
#define ApplicationPublisher "Polywall"
#define ApplicationURL "visiology.com"
;#define ApplicationExeName "MediaToolkit.dll"
#define ApplicationExeName "ScreenStreamer.Wpf.App.exe"

#define DEBUG

#define TrunkPath "..\"

#define ReleasePath "bin\Release"
#define DebugPath "bin\Debug"

;Путь к исходным файлам 
#define CurrentSourcePath TrunkPath + ReleasePath
#define CurrentTestPath TrunkPath + "Tests\bin\Release"

#ifdef DEBUG 
  #define CurrentSourcePath TrunkPath + DebugPath
  #define CurrentTestPath TrunkPath + "Tests\bin\Debug"
#endif

#define WpfSourcePath CurrentSourcePath + "\ScreenStreamer.Wpf.App\"

;Получаем версию из атрибутов исполняемого файла
;#define ApplicationVersion GetFileVersion(CurrentSourcePath + "\MediaToolkit.dll")

#define ApplicationVersion GetVersionNumbersString(WpfSourcePath + ApplicationExeName)
#define ProductVersion GetStringFileInfo(WpfSourcePath + ApplicationExeName, PRODUCT_VERSION)


#define AVF1 Copy(ApplicationVersion, 1, Pos(".", ApplicationVersion) - 1) + "_" + Copy(ApplicationVersion, Pos(".", ApplicationVersion) + 1)
#define AVF2 Copy(AVF1,1, Pos(".", AVF1) - 1) + "_" + Copy(AVF1, Pos(".", AVF1) + 1)
#define AppicationVersionFile Copy(AVF2,1, Pos(".", AVF2) - 1) ;+ "b" + Copy(AVF2, Pos(".",AVF2) + 1)

;Название выходного файла инсталлятора
#define OutputBaseFilenameVervion "PolywallStreamer_v" + ApplicationVersion
#ifdef DEBUG 
  #define OutputBaseFilenameVervion "PolywallStreamer_v" + ApplicationVersion + "_Debug"
#endif

[Setup]
AppId={{2AD1EF0A-6962-466B-86B8-85C3BDE65049}
AppMutex= "ScreenStreamer.Wpf.App_E7B28EAF-A330-4467-98F8-F3BCA7613268"
AppName={#ApplicationName}

OutputBaseFilename={#OutputBaseFilenameVervion}

AppVersion= {#ApplicationVersion}
AppVerName={#ApplicationName} {#ApplicationVersion}
VersionInfoVersion = {#ApplicationVersion}

AppPublisher={#ApplicationPublisher}
AppPublisherURL={#ApplicationURL}
AppSupportURL={#ApplicationURL}
AppUpdatesURL={#ApplicationURL}

;DefaultDirName={sd}\Mitsar\{#ApplicationName}
DefaultDirName={commonpf}\Polywall\Polywall Streamer
DefaultGroupName=Polywall\{#ApplicationName}

AllowNoIcons=yes

Compression=lzma
SolidCompression=yes
PrivilegesRequired = poweruser

DisableProgramGroupPage=no
DisableDirPage=no
AllowCancelDuringInstall=no

WizardSmallImageFile=Resources\logo_48x48.bmp
SetupIconFile=Resources\logo.ico
UninstallDisplayIcon = {app}\MediaToolkit.Resources.dll

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl, CustomMessage\Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl, CustomMessage\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Dirs]
;Name: "{commonappdata}\"
;Name: "{commonappdata}\..."


[Files]

Source: "{#CurrentSourcePath}\avcodec-58.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\avdevice-58.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\avfilter-7.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\avformat-58.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\avutil-56.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\postproc-55.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\swresample-3.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\swscale-5.dll"; DestDir: "{app}"; Flags: replacesameversion

Source: "{#CurrentSourcePath}\MediaToolkit.Resources.dll"; DestDir: "{app}"; Flags: replacesameversion

Source: "{#WpfSourcePath}\MediaToolkit.Core.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#WpfSourcePath}\MediaToolkit.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#WpfSourcePath}\MediaToolkit.FFmpeg.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#WpfSourcePath}\MediaToolkit.NativeAPIs.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#WpfSourcePath}\MediaToolkit.UI.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#WpfSourcePath}\MediaToolkit.SharpDX.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#WpfSourcePath}\MediaToolkit.DeckLink.dll"; DestDir: "{app}"; Flags: replacesameversion

Source: "{#WpfSourcePath}\Microsoft.Xaml.Behaviors.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#WpfSourcePath}\NAudio.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#WpfSourcePath}\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#WpfSourcePath}\NLog.dll"; DestDir: "{app}"; Flags: replacesameversion

Source: "{#WpfSourcePath}\ScreenStreamer.Common.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#WpfSourcePath}\ScreenStreamer.Wpf.App.exe"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#WpfSourcePath}\ScreenStreamer.Wpf.App.exe.config"; DestDir: "{app}"; Flags: replacesameversion

#ifdef DEBUG 
Source: "..\Resources\CRT\Debug_NoRedist\ucrtbased.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\Resources\CRT\Debug_NoRedist\vcruntime140d.dll"; DestDir: "{app}"; Flags: replacesameversion

#endif

;Source: "{#WpfSourcePath}\SharpDX.Direct2D1.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\SharpDX.Direct3D11.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\SharpDX.Direct3D9.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\SharpDX.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\SharpDX.DXGI.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\SharpDX.Mathematics.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\SharpDX.MediaFoundation.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\SharpDX.D3DCompiler.dll"; DestDir: "{app}"; Flags: replacesameversion

;Source: "{#WpfSourcePath}\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: replacesameversion

;Source: "{#WpfSourcePath}\Prism.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\Prism.pdb"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\Prism.Wpf.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\Prism.Wpf.pdb"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\Prism.Wpf.xml"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\Prism.xml"; DestDir: "{app}"; Flags: replacesameversion

;Source: "{#WpfSourcePath}\CommonServiceLocator.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\GalaSoft.MvvmLight.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\GalaSoft.MvvmLight.Extras.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\GalaSoft.MvvmLight.Platform.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\Hardcodet.Wpf.TaskbarNotification.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\MaterialDesignColors.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\MaterialDesignThemes.Wpf.dll"; DestDir: "{app}"; Flags: replacesameversion

;Source: "{#WpfSourcePath}\System.Threading.Tasks.Extensions.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\System.Threading.Tasks.Extensions.xml"; DestDir: "{app}"; Flags: replacesameversion

;Source: "{#WpfSourcePath}\System.ValueTuple.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\System.Windows.Interactivity.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\Unity.Abstractions.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\Unity.Container.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\Unity.Container.pdb"; DestDir: "{app}"; Flags: replacesameversion

;Source: "{#WpfSourcePath}\WindowsInput.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#WpfSourcePath}\WindowsInput.xml"; DestDir: "{app}"; Flags: replacesameversion


Source: "{#CurrentTestPath}\Test.PolywallClient.exe"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentTestPath}\Test.PolywallClient.exe.config"; DestDir: "{app}"; Flags: replacesameversion       




// Microsoft Visual C++ 2015-2019 Redistributable (x86) - 14.24.28127
// https://support.microsoft.com/ru-ru/help/2977003/the-latest-supported-visual-c-downloads
//HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\X86
//HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\X86
Source: "{#CurrentSourcePath}\vs_redist\vc_redist.x86.exe"; DestDir: "{tmp}"; Check: VCRedistNeedsInstall; Flags: deleteafterinstall


;Source: "{#CurrentSourcePath}\ucrtbase.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\ucrtbase_clr0400.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\ucrtbased.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\vcamp140d.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\vcruntime140.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\vcruntime140_clr0400.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\vcruntime140d.dll"; DestDir: "{app}"; Flags: replacesameversion


[InstallDelete]
;Type: filesandordirs; Name: "{app}\ru"
;Type: filesandordirs; Name: "{app}\en"
;Type: filesandordirs; Name: "{app}\ja"
;Type: filesandordirs; Name: "{app}\pl"
;Type: files; Name: "{app}\unins000.dat"

[Registry]
Root: HKLM; Subkey: "Software\Visiology\Polywall\Path"; ValueType: string; ValueName: "PolywallStreamerPath"; ValueData: "{app}"; Flags: uninsdeletekey

[Icons]
;Name: "{group}\{#ApplicationName}"; Filename: "{app}\{#ApplicationExeName}"
Name: "{group}\Polywall Streamer"; Filename: "{app}\ScreenStreamer.Wpf.App.exe"
Name: "{group}\uninstall.exe"; Filename: "{uninstallexe}"

Name: "{commondesktop}\Polywall Streamer"; Filename: "{app}\ScreenStreamer.Wpf.App.exe"; Tasks: desktopicon
;Name: "{commondesktop}\{#ApplicationName}"; Filename: "{app}\{#ApplicationExeName}"; Tasks: desktopicon
;Name: "{commondesktop}\Test.PolywallClient.exe"; Filename: "{app}\Test.PolywallClient.exe"; Tasks: desktopicon

;Name: "{commondesktop}\{#ApplicationName}"; Filename: "{app}\{#ApplicationExeName}"; Tasks: desktopicon
;Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#ApplicationName}"; Filename: "{app}\{#ApplicationExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{tmp}\vc_redist.x86.exe"; Parameters: "/install /quiet /norestart"; Check: VCRedistNeedsInstall; StatusMsg: "Installing VC++ Redistributable...";

[Code]


function VCinstalled(const regKey: String): Boolean;
 { Function for Inno Setup Compiler }
 { Returns True if same or later  MSVC 14.24.28127 is installed, otherwise False. }
 var
  major: Cardinal;
  minor: Cardinal;
  bld: Cardinal;
  rbld: Cardinal;

 begin
  Result := False;

  if RegQueryDWordValue(HKEY_LOCAL_MACHINE, regKey, 'Major', major) then begin
    if RegQueryDWordValue(HKEY_LOCAL_MACHINE, regKey, 'Minor', minor) then begin
      if RegQueryDWordValue(HKEY_LOCAL_MACHINE, regKey, 'Bld', bld) then begin
        if RegQueryDWordValue(HKEY_LOCAL_MACHINE, regKey, 'RBld', rbld) then begin
            Log('VC 2015-2019 Redist Major is: ' + IntToStr(major) + ' Minor is: ' + IntToStr(minor) + ' Bld is: ' + IntToStr(bld) + ' Rbld is: ' + IntToStr(rbld));
            { Version info was found. Return true if later or equal to our 14.24.28127.0 redistributable }
            { Note brackets required because of weird operator precendence }
            Result := (major >= 14) and (minor >= 24) and (bld >= 28127) and (rbld >= 0)
        end;
      end;
    end;
  end;
 end;

function VCRedistNeedsInstall: Boolean;
begin
 if NOT IsWin64 then 
  Result := not (VCinstalled('SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\X86'))
 else if Is64BitInstallMode then
  Result := not (VCinstalled('SOFTWARE\WOW6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x64'))
 else
  Result := not (VCinstalled('SOFTWARE\WOW6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x86'));  
end;


(*
#IFDEF UNICODE
  #DEFINE AW "W"
#ELSE
  #DEFINE AW "A"
#ENDIF

type
  INSTALLSTATE = Longint;

  const
  INSTALLSTATE_INVALIDARG = -2;  { An invalid parameter was passed to the function. }
  INSTALLSTATE_UNKNOWN = -1;     { The product is neither advertised or installed. }
  INSTALLSTATE_ADVERTISED = 1;   { The product is advertised but not installed. }
  INSTALLSTATE_ABSENT = 2;       { The product is installed for a different user. }
  INSTALLSTATE_DEFAULT = 5;      { The product is installed for the current user. }

  { Visual C++ 2017 Redistributable 14.16.27024 }
  VC_2017_REDIST_X84_ADD = '{7258184A-EC44-4B1A-A7D3-68D85A35BFD0}';
  VC_2017_REDIST_X84_MIN = '{5EEFCEFB-E5F7-4C82-99A5-813F04AA4FBD}';

  VC_2017_REDIST_X64_ADD = '{9D29FC96-9EEE-4253-943F-96B3BBFDD0B6}';
  VC_2017_REDIST_X64_MIN = '{F1B0FB3A-E0EA-47A6-9383-3650655403B0}';

function MsiQueryProductState(szProduct: string): INSTALLSTATE; 
  external 'MsiQueryProductState{#AW}@msi.dll stdcall';

function VCVersionInstalled(const ProductID: string): Boolean;
begin
  Result := MsiQueryProductState(ProductID) = INSTALLSTATE_DEFAULT;
end;

function _VCRedistNeedsInstall: Boolean;
begin
  Result := not VCVersionInstalled(VC_2017_REDIST_X64_MIN);
end;

    *)



function GetAppCurrentVersion(param: String): String;
begin
  Result:='{#ApplicationVersion}';
end;

function GetAppID(param: String): String;
begin
  Result := '{#emit SetupSetting("AppId")}';
end;




(*
function GetPathInstalled(AppID: String): String;
var
  PrevPath: String;
begin
  PrevPath := '';
  if not RegQueryStringValue(HKLM, 'Software\Microsoft\Windows\CurrentVersion\Uninstall\'+AppID+'_is1', 'Inno Setup: App Path', PrevPath) then 
  begin
    RegQueryStringValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Uninstall\'+AppID+'_is1', 'Inno Setup: App Path', PrevPath);
  end;
  Result := PrevPath;
end;

*)

function GetUninstallString(RegPath: string): string;
var
  UninstallExecFile: String;
begin
  Result := '';
  UninstallExecFile := '';
  if not RegQueryStringValue(HKLM, RegPath, 'UninstallString', UninstallExecFile) then
    RegQueryStringValue(HKCU, RegPath, 'UninstallString', UninstallExecFile);
  Result := UninstallExecFile;
end;

function GetRegValue(RegPath: string; RegValue: string): string;
var
  ResultStr: String;
begin
  Result := '';
  ResultStr := '';
  if not RegQueryStringValue(HKLM, RegPath, RegValue, ResultStr) then
    RegQueryStringValue(HKCU, RegPath, RegValue, ResultStr);
  Result := ResultStr;
end;

function UnInstallOldVersion(sUnInstallString: String): Integer;
var
  iResultCode: Integer;
begin
// Return Values:
// 1 - uninstall string is empty
// 2 - error executing the UnInstallString
// 3 - successfully executed the UnInstallString

  // default return value
  Result := 0;

  if sUnInstallString <> '' then begin
    sUnInstallString := RemoveQuotes(sUnInstallString);
    {Exec(ExpandConstant(UninstallExecFile), '/VERYSILENT ', '', SW_SHOW, ewWaitUntilTerminated, iResultCode);}
    { if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then}
    if Exec(sUnInstallString, '/VERYSILENT ','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
      Result := 3
    else
      Result := 2;
  end else
    Result := 1;
end;

var
  UpdatePage: TWizardPage;
  DescriptionLabel: TLabel;
  UpdateButton: TNewRadioButton;
  CustomButton: TNewRadioButton;

  UpdateMode: Boolean;
  UninstallRegPath: string;
  UninstallExecFile: string;
  UninstallExecFileVersion: string;

function InitializeSetup: Boolean;

begin

   UninstallExecFile :='';
   UninstallRegPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
   if RegValueExists(HKEY_LOCAL_MACHINE,UninstallRegPath, 'UninstallString') then
   begin
      UpdateMode := true;
      UninstallExecFile := GetUninstallString(UninstallRegPath);
      UninstallExecFile :=  RemoveQuotes(UninstallExecFile);

      UninstallExecFileVersion := GetRegValue(UninstallRegPath, 'DisplayVersion');
      UninstallExecFileVersion :=  RemoveQuotes(UninstallExecFileVersion)

   end
   else 
   begin
      UpdateMode := false;
   end;

   Result := True;

  
end;



procedure InitializeWizard();
begin

  UpdatePage := CreateCustomPage( wpWelcome, 
                                  CustomMessage('UpdatePageCaption'),//'Select Installation Options', 
                                  FmtMessage(CustomMessage('UpdatePageDescription'), ['{#ApplicationName}'])); // 'Choose how you want to install {#ApplicationName}.');

  DescriptionLabel := TLabel.Create(UpdatePage);
  DescriptionLabel.Parent := UpdatePage.Surface;

  DescriptionLabel.AutoSize := True;
  DescriptionLabel.Wordwrap := True;
  DescriptionLabel.Width := UpdatePage.SurfaceWidth; 
  //DescriptionLabel.Height := 40;      

  //DescriptionLabel.Caption := FmtMessage(CustomMessage('DescriptionLabelCaption'), ['{#ApplicationName}', '{#ApplicationVersion}']); //'{#ApplicationName} version {#ApplicationVersion} already installed on your system. Select the operation you want to perform and click Next to contunue.';
  DescriptionLabel.Caption := FmtMessage(CustomMessage('DescriptionLabelCaption'), ['{#ApplicationName}', UninstallExecFileVersion]); //'{#ApplicationName} version {#ApplicationVersion} already installed on your system. Select the operation you want to perform and click Next to contunue.';

  UpdateButton := TNewRadioButton.Create(UpdatePage);
  UpdateButton.Parent := UpdatePage.Surface;
  UpdateButton.Left := 8;  
  UpdateButton.Top := DescriptionLabel.Top + DescriptionLabel.Height + ScaleY(13);  
  UpdateButton.Width := UpdatePage.SurfaceWidth;
  UpdateButton.Height := ScaleY(UpdateButton.Height);
  UpdateButton.Caption := CustomMessage('UpdateButtonCaption');//'Upgrade using previous settings (recommended).';
  UpdateButton.Checked := (UpdateMode = True); { default, unless other type is selected below }

  CustomButton := TNewRadioButton.Create(UpdatePage);
  CustomButton.Parent := UpdatePage.Surface;  
  CustomButton.Width := UpdatePage.SurfaceWidth;
  CustomButton.Height := ScaleY(CustomButton.Height);
  CustomButton.Left := 8;  
  CustomButton.Top := UpdateButton.Top + UpdateButton.Height + ScaleY(8);  
  CustomButton.Checked := (UpdateMode = False); {(WizardForm.TypesCombo.ItemIndex = 1); }
  CustomButton.Caption := CustomMessage('CustomButtonCaption');//'Change settings (advanced mode).';

end;

function NextButtonClick(CurPageID: Integer): Boolean;
//var 
//  iResultCode: Integer;
begin

  if CurPageID = UpdatePage.ID then
  begin

   // удаляем предыдущюу программу
    if UpdateMode And UpdateButton.Checked then 
       UnInstallOldVersion(UninstallExecFile);
    
  end
  else if (CurPageID = wpReady) then
  begin
      UnInstallOldVersion(UninstallExecFile);
  end; 

  Result := True;
end;



function ShouldSkipPage(PageID: Integer): Boolean;
begin
   (*
     wpWelcome,
     wpLicense,
     wpPassword, 
     wpInfoBefore, 
     wpUserInfo, 
     wpSelectDir, 
     wpSelectComponents,
     wpSelectProgramGroup, 
     wpSelectTasks, 
     wpReady, 
     wpPreparing, 
     wpInstalling, 
     wpInfoAfter, 
     wpFinished
   *)
  if UpdateMode then
  begin
    if (PageID = wpSelectTasks) then
    begin 
        Result := UpdateButton.Checked;
    end 
    else if (PageID = wpSelectDir) then
    begin
        Result := UpdateButton.Checked;
    end
    else if (PageID = wpReady) then
    begin
        Result := UpdateButton.Checked;
    end
    else if (PageID = wpSelectProgramGroup) then
    begin
        Result := UpdateButton.Checked; 
    end

    else if (PageID = UpdatePage.ID) then
    begin
        Result := False;   
    end;
  end
  else 
  begin

    if (PageID = UpdatePage.ID) then
    begin
        Result := True;
    end;

  end;
      

  (*
  Result := (PageID = wpSelectTasks) and (UpdateButton.Checked);
  Result := (PageID = wpSelectDir) and (UpdateButton.Checked);
  *)

end;





(*
// Проверяем установку дотнета версии 4.0, взято из стандартных примеров innosetup
function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1.4322'     .NET Framework 1.1
//    'v2.0.50727'    .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//    'v4.5'          .NET Framework 4.5
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key: string;
    install, release, serviceCount: cardinal;
    check45, success: boolean;
begin
    // .NET 4.5 installs as update to .NET 4.0 Full
    if version = 'v4.5' then begin
        version := 'v4\Full';
        check45 := true;
    end else
        check45 := false;

    // installation key group for all .NET versions
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + version;

    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;

    // .NET 4.0/4.5 uses value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;

    // .NET 4.5 uses additional value Release
    if check45 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
        success := success and (release >= 378389);
    end;

    result := success and (install = 1) and (serviceCount >= service);
end;

//Удаляем установленные файлы
procedure DeleteNewFiles(ADirName: string);
begin
  DelTree(ADirName, True, True, True);
end;


// Получаем путь к деинсталлятору
function GetUninstallString(RegPath: string): string;
var
  UninstallExecFile: String;
begin
  Result := '';
  UninstallExecFile := '';
  if not RegQueryStringValue(HKLM, RegPath, 'UninstallString', UninstallExecFile) then
    RegQueryStringValue(HKCU, RegPath, 'UninstallString', UninstallExecFile);
  Result := UninstallExecFile;
end;

/////////////////////////////////////////
// ЗДЕСЬ НАЧИНАЕТСЯ ВЫПОЛНЕНИЕ SETUP-а //
/////////////////////////////////////////
function InitializeSetup: Boolean;
var
  MsgBoxResult: Integer;
  iResultCode: Integer;

  //UninstallRegPath64: string;
  //UninstallRegPath32: string;
  UninstallRegPath: string;

  Message: string;

  UninstallExecFile: string;
begin
  Result := True;
  // Проверяем установку .NET_v4.0
  if not IsDotNetDetected('v4\Full', 0) then begin
   Message:=FmtMessage(CustomMessage('DotNetVer40Requires'), ['{#ApplicationName}']);
   MsgBox(Message, mbInformation, MB_OK);
    Result := False;
   Exit; // Если дотнета 4.0 нету - закрываем инсталлятор
  end;

  //UninstallRegPath64 := ExpandConstant('Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  //UninstallRegPath32 := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');

  UninstallRegPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  // проверяем уже установленную версию
  if RegValueExists(HKEY_LOCAL_MACHINE,UninstallRegPath, 'UninstallString') then
  begin
    Message:=FmtMessage(CustomMessage('PreviousVersionFound'), ['{#ApplicationName}']);

    MsgBoxResult := MsgBox(Message , mbInformation, MB_YESNO);
    if MsgBoxResult = IDYES then
    begin
      UninstallExecFile := GetUninstallString(UninstallRegPath);
      UninstallExecFile :=  RemoveQuotes(UninstallExecFile);
      Exec(ExpandConstant(UninstallExecFile), '/VERYSILENT ', '', SW_SHOW, ewWaitUntilTerminated, iResultCode);   // Uninstall делаем без лишних вопросов
      if iResultCode=0 then
      begin //Выполнение uninstll-а установленной программы было успешно завершено
        Result := True; //MsgBox(IntToStr(iResultCode), mbInformation, MB_OK);
      end else // Uninstall был отменен
        Result := False;
    end
   else  // Пользователь не хочет удалять установленную версию
     Result := False; // Закрываем инсталлятор
  end;
end;

// ДЕИНСТАЛЛЯЦИЯ
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin 
  if CurUninstallStep = usUninstall then begin      
      DeleteNewFiles(ExpandConstant('{app}'));// удаляем все что лежит в папке приложения
  end;
end;


*)


