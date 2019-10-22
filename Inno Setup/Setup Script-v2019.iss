#define MyAppName "CRMRevitTools-v2019"
#define MyAppVersion "1.0.2"
#define MyVersionInfoVersion "1.0.2"
#define MyAppPublisher "Christopher Ryan Mackay"

[Setup]
AppId={{4EAC56CA-BB49-4251-9BB9-A01CF961A441}
AppName={#MyAppName}
AppCopyright=Copyright © 2019 Christopher Ryan Mackay
AppVersion={#MyAppVersion}
VersionInfoVersion={#MyVersionInfoVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={userdocs}\{#MyAppName}
DisableDirPage=yes
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=.
OutputBaseFilename=CRMRevitTools-v2019-v{#MyAppVersion} Setup
Compression=lzma
SolidCompression=yes
LicenseFile=LICENSE.txt
PrivilegesRequired=lowest

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "CRMRevitTools\Commands\*"; DestDir: "{userdocs}\CRMRevitTools\v2019\Commands"; Flags: ignoreversion
Source: "CRMRevitTools\MenuCreator\*"; DestDir: "{userdocs}\CRMRevitTools\v2019\MenuCreator"; Flags: ignoreversion
Source: "CRMRevitTools\RevitIcons\16x16\*"; DestDir: "{userdocs}\CRMRevitTools\v2019\RevitIcons\16x16\"; Flags: ignoreversion
Source: "CRMRevitTools\RevitIcons\32x32\*"; DestDir: "{userdocs}\CRMRevitTools\v2019\RevitIcons\32x32\"; Flags: ignoreversion
Source: "CRMRevitTools\Addin File\*"; DestDir: "C:\ProgramData\Autodesk\Revit\Addins\2019"; Flags: ignoreversion
Source: "CRMRevitToolsInit-v2019.exe"; DestDir: "{userdocs}\CRMRevitTools\v2019"; Flags: ignoreversion
Source: "LICENSE.txt"; DestDir: "{userdocs}\CRMRevitTools\v2019"; Flags: ignoreversion

;CRMRevitTools_Help
Source: "..\CRMRevitTools_Help\*"; DestDir: "{userdocs}\CRMRevitTools\v2019\CRMRevitTools_Help"; Flags: ignoreversion
Source: "..\CRMRevitTools_Help\css\*"; DestDir: "{userdocs}\CRMRevitTools\v2019\CRMRevitTools_Help\css"; Flags: ignoreversion

;Create Revit Sheets
Source: "..\CRMRevitTools_Help\images\create_revit_sheets\*"; DestDir: "{userdocs}\CRMRevitTools\v2019\CRMRevitTools_Help\images\create_revit_sheets"; Flags: ignoreversion

;Shared Parameter Creator
Source: "..\Parameter_Template-v2019.xlsx"; DestDir: "{userdocs}\CRMRevitTools\v2019\"; Flags: ignoreversion
Source: "..\CRMRevitTools_Help\images\shared_parameter_creator\*"; DestDir: "{userdocs}\CRMRevitTools\v2019\CRMRevitTools_Help\images\shared_parameter_creator"; Flags: ignoreversion

;Sheet Renamer
Source: "..\CRMRevitTools_Help\images\sheet_renamer\*"; DestDir: "{userdocs}\CRMRevitTools\v2019\CRMRevitTools_Help\images\sheet_renamer"; Flags: ignoreversion

[UninstallDelete] 
Type: dirifempty; Name: {userdocs}\CRMRevitTools;

[Run]
Filename: {userdocs}\CRMRevitTools\v2019\CRMRevitToolsInit-v2019.exe;

