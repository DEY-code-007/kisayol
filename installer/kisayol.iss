; Kisayol installer (Inno Setup 6). Build: ISCC.exe installer\kisayol.iss
; Per-user install, no admin prompt, .NET runtime bundled (self-contained publish).

#define AppName "Kısayol"
#define AppVersion RemoveFileExt(GetVersionNumbersString("..\publish\win-x64\Kisayol.exe"))
#define AppExe "Kisayol.exe"

[Setup]
AppId={{B6E2D4A1-7C3F-4E8B-9A15-3D2F6C8E1A47}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=Deniz Egemen Yılmaz
VersionInfoVersion={#AppVersion}
VersionInfoDescription=Kisayol Setup
DefaultDirName={localappdata}\Programs\Kisayol
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
DisableDirPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.19041
OutputDir=..\release
OutputBaseFilename=Kisayol-Setup-{#AppVersion}
SetupIconFile=..\src\Kisayol\app.ico
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName}
WizardStyle=modern dynamic
Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes
CloseApplications=yes
ShowLanguageDialog=no
LanguageDetectionMethod=none

[Languages]
Name: "tr"; MessagesFile: "compiler:Languages\Turkish.isl"

[CustomMessages]
tr.StartWithWindows=Kısayol'u Windows ile başlat
tr.Startup=Başlangıç:

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "startup"; Description: "{cm:StartWithWindows}"; GroupDescription: "{cm:Startup}"; Flags: unchecked

[Files]
Source: "..\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "Kisayol"; ValueData: """{app}\{#AppExe}"""; Tasks: startup; Flags: uninsdeletevalue

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\Kisayol"
