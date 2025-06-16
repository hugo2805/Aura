#define AppName "Aura"
#define AppPublisher "Sealion, SAS"
#define MyAppURL "https://sealion.fr"
#define AppExeName "AuraInstaller.exe"
#define AppSourceDir "Aura"

[Setup]
AppId={{2da0860b-95d6-4f12-bfc5-4279d8d5dd54}}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
OutputDir=.\Output
OutputBaseFilename={#AppName}_Setup
SetupIconFile=logo.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
WizardImageFile=cover.bmp
WizardSmallImageFile=LogoDL.bmp
DisableDirPage=yes
DisableProgramGroupPage=yes
Uninstallable=yes
ShowLanguageDialog=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "armenian"; MessagesFile: "compiler:Languages\Armenian.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "bulgarian"; MessagesFile: "compiler:Languages\Bulgarian.isl"
Name: "catalan"; MessagesFile: "compiler:Languages\Catalan.isl"
Name: "corsican"; MessagesFile: "compiler:Languages\Corsican.isl"
Name: "czech"; MessagesFile: "compiler:Languages\Czech.isl"
Name: "danish"; MessagesFile: "compiler:Languages\Danish.isl"
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "finnish"; MessagesFile: "compiler:Languages\Finnish.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "hebrew"; MessagesFile: "compiler:Languages\Hebrew.isl"
Name: "hungarian"; MessagesFile: "compiler:Languages\Hungarian.isl"
Name: "icelandic"; MessagesFile: "compiler:Languages\Icelandic.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"
Name: "norwegian"; MessagesFile: "compiler:Languages\Norwegian.isl"
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "portuguese"; MessagesFile: "compiler:Languages\Portuguese.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "slovak"; MessagesFile: "compiler:Languages\Slovak.isl"
Name: "slovenian"; MessagesFile: "compiler:Languages\Slovenian.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"

[Files]
Source: "{#MyBuildDir}\Updater\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#MyBuildDir}\Updater\Assets\*"; DestDir: "{app}\Assets"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#MyBuildDir}\Game\*";    DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#MyBuildDir}\version.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{commondesktop}\Aura"; Filename: "{app}\AuraInstaller.exe"; IconFilename: "{app}\Assets\logo.ico"

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Lancer {#AppName}"; Flags: nowait postinstall
