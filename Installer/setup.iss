#define AppName      "Aura"
#define AppPublisher "Sealion, SAS"
#define AppURL       "https://sealion.fr"
#define AppExeName   "AuraInstaller.exe"

[Setup]
AppId={{2da0860b-95d6-4f12-bfc5-4279d8d5dd54}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}

; Installation dans Program Files (requiert UAC)
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
PrivilegesRequired=admin

; Apparence
WizardStyle=modern
WizardImageFile=cover.bmp
WizardSmallImageFile=LogoDL.bmp
SetupIconFile=logo.ico
UninstallDisplayIcon={app}\logo.ico
UninstallDisplayName={#AppName}

; Sortie
OutputDir=.\Output
OutputBaseFilename=Aura_Setup
Compression=lzma
SolidCompression=yes
ShowLanguageDialog=no

[Languages]
Name: "french";    MessagesFile: "compiler:Languages\French.isl"
Name: "english";   MessagesFile: "compiler:Default.isl"
Name: "german";    MessagesFile: "compiler:Languages\German.isl"
Name: "spanish";   MessagesFile: "compiler:Languages\Spanish.isl"
Name: "italian";   MessagesFile: "compiler:Languages\Italian.isl"
Name: "portuguese"; MessagesFile: "compiler:Languages\Portuguese.isl"

[Tasks]
Name: "desktopicon"; \
  Description: "{cm:CreateDesktopIcon}"; \
  GroupDescription: "{cm:AdditionalIcons}"; \
  Flags: unchecked

[Files]
; Launcher (binaire self-contained)
Source: "{#MyBuildDir}\Updater\AuraInstaller.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "logo.ico"; DestDir: "{app}"; Flags: ignoreversion

; Jeu Unity → installé dans %LocalAppData%\Aura\ (DataDir du launcher)
Source: "{#MyBuildDir}\Game\*"; DestDir: "{localappdata}\Aura"; \
  Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; Menu Démarrer → recherche Windows
Name: "{group}\{#AppName}"; \
      Filename: "{app}\{#AppExeName}"; \
      IconFilename: "{app}\logo.ico"; \
      WorkingDir: "{app}"

; Bureau (optionnel — coché par l'utilisateur)
Name: "{userdesktop}\{#AppName}"; \
      Filename: "{app}\{#AppExeName}"; \
      IconFilename: "{app}\logo.ico"; \
      WorkingDir: "{app}"; \
      Tasks: desktopicon

[Registry]
; Permet de trouver l'appli via "Rechercher" Windows
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\AuraInstaller.exe"; \
  ValueType: string; ValueName: ""; ValueData: "{app}\{#AppExeName}"; Flags: uninsdeletekey

[Run]
Filename: "{app}\{#AppExeName}"; \
  Description: "Lancer {#AppName}"; \
  Flags: nowait postinstall skipifsilent

[Code]
// Écrit version.txt dans DataDir pour que le launcher sache que le jeu est déjà installé
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
    SaveStringToFile(ExpandConstant('{localappdata}\Aura\version.txt'),
      '{#AppVersion}', False);
end;
