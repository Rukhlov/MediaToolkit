#include "ISPPBuiltins.iss"

#define ApplicationName "Polywall Media Streamer"
#define ApplicationPublisher "Polywall"
#define ApplicationURL "visiology.com"
;#define ApplicationExeName "MediaToolkit.dll"
#define ApplicationExeName "Test.Streamer.exe"

;#define DEBUG

#define TrunkPath "..\"

#define ReleasePath "bin\Release"
#define DebugPath "bin\Debug"

;Путь к исходным файлам 
#define CurrentSourcePath TrunkPath + ReleasePath

#ifdef DEBUG 
  #define CurrentSourcePath TrunkPath + DebugPath
#endif

;Получаем версию базы из атрибутов исполняемого файла
#define ApplicationVersion GetFileVersion(CurrentSourcePath + "\MediaToolkit.dll")

;Форматируем строку версии БД
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
AppMutex= "MediaToolkit"
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
DefaultDirName={commonpf}\Polywall\MediaStreamer
DefaultGroupName=Polywall\MediaStreamer\{#ApplicationName}

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
;Name: "{commonappdata}\Mitsar\Data Studio"
;Name: "{commonappdata}\Mitsar\Data Studio\FileData"


[Files]
Source: "{#CurrentSourcePath}\avcodec-58.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\avdevice-58.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\avfilter-7.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\avformat-58.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\avutil-56.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\CommandLine.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\CommandLine.xml"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\MediaToolkit.Resources.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\MediaToolkit.Core.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\MediaToolkit.Core.pdb"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\MediaToolkit.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\MediaToolkit.dll.config"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\MediaToolkit.FFmpeg.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\MediaToolkit.FFmpeg.pdb"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\MediaToolkit.NativeAPIs.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\MediaToolkit.NativeAPIs.pdb"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\MediaToolkit.pdb"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\MediaToolkit.UI.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\MediaToolkit.UI.dll.config"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\MediaToolkit.UI.pdb"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\mscoree.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\NAudio.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\NAudio.xml"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\NLog.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\NLog.xml"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\postproc-55.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\SharpDX.Direct2D1.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\SharpDX.Direct2D1.pdb"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\SharpDX.Direct3D11.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\SharpDX.Direct3D11.pdb"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\SharpDX.Direct3D9.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\SharpDX.Direct3D9.pdb"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\SharpDX.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\SharpDX.DXGI.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\SharpDX.DXGI.pdb"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\SharpDX.Mathematics.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\SharpDX.Mathematics.pdb"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\SharpDX.MediaFoundation.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\SharpDX.MediaFoundation.pdb"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\SharpDX.pdb"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\swresample-3.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\swscale-5.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\Test.Client.exe"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\Test.Client.exe.config"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\Test.Client.pdb"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\Test.Streamer.exe"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\Test.Streamer.exe.config"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\Test.Streamer.pdb"; DestDir: "{app}"; Flags: replacesameversion

;Source: "{#CurrentSourcePath}\Test.WebCam.exe"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\Test.WebCam.exe.config"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\Test.WebCam.pdb"; DestDir: "{app}"; Flags: replacesameversion

;Source: "{#CurrentSourcePath}\ucrtbase.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\ucrtbase_clr0400.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\ucrtbased.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\vcamp140d.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\vcruntime140.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\vcruntime140_clr0400.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\vcruntime140d.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\WindowsInput.dll"; DestDir: "{app}"; Flags: replacesameversion
;Source: "{#CurrentSourcePath}\WindowsInput.xml"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#CurrentSourcePath}\MediaToolkit.DeckLink.dll"; DestDir: "{app}"; Flags: replacesameversion

[InstallDelete]
;Type: filesandordirs; Name: "{app}\ru"
;Type: filesandordirs; Name: "{app}\en"
;Type: filesandordirs; Name: "{app}\ja"
;Type: filesandordirs; Name: "{app}\pl"
;Type: files; Name: "{app}\unins000.dat"

[Registry]
Root: HKLM; Subkey: "Software\Visiology\Polywall\Path"; ValueType: string; ValueName: "MediaToolkitPath"; ValueData: "{app}"; Flags: uninsdeletekey

[Icons]
Name: "{group}\{#ApplicationName}"; Filename: "{app}\{#ApplicationExeName}"
Name: "{group}\uninstall.exe"; Filename: "{uninstallexe}"

Name: "{commondesktop}\{#ApplicationName}"; Filename: "{app}\{#ApplicationExeName}"; Tasks: desktopicon
;Name: "{commondesktop}\{#ApplicationName}"; Filename: "{app}\{#ApplicationExeName}"; Tasks: desktopicon
;Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#ApplicationName}"; Filename: "{app}\{#ApplicationExeName}"; Tasks: quicklaunchicon

[Run]

[Code]

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
  //  if UpdateMode And UpdateButton.Checked then 
  //     UnInstallOldVersion(UninstallExecFile);
    
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


