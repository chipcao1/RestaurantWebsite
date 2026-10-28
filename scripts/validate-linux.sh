#!/usr/bin/env bash
# Compile classic MVC5 and Razor on Linux; this does not replace Windows runtime tests.
set -euo pipefail
YAMY_REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
YAMY_DOTNET="${YAMY_DOTNET:-dotnet}"
YAMY_VALIDATION_ROOT="${YAMY_VALIDATION_ROOT:-/tmp/restaurant-yamy-validation}"
mkdir -p "$YAMY_VALIDATION_ROOT"
cd "$YAMY_REPO_ROOT"
python3 scripts/check-resx.py
python3 - "$YAMY_VALIDATION_ROOT" <<'PY'
from pathlib import Path
import sys,xml.etree.ElementTree as E
out=Path(sys.argv[1]);r=E.Element('Project',Sdk='Microsoft.NET.Sdk');p=E.SubElement(r,'PropertyGroup');E.SubElement(p,'TargetFramework').text='net48';E.SubElement(p,'EnableDefaultCompileItems').text='false';g=E.SubElement(r,'ItemGroup')
items=[(x.get('id'),x.get('version')) for x in E.parse('RestaurantWebsite/packages.config').getroot()]
items += [('Microsoft.NETFramework.ReferenceAssemblies.net48','1.0.3'),('MSBuild.Microsoft.VisualStudio.Web.targets','14.0.0.3')]
for name,version in items:E.SubElement(g,'PackageReference',Include=name,Version=version)
E.ElementTree(r).write(out/'restore.csproj',encoding='utf-8',xml_declaration=True)
PY
"$YAMY_DOTNET" restore "$YAMY_VALIDATION_ROOT/restore.csproj" --packages "$YAMY_VALIDATION_ROOT/nuget" --source https://api.nuget.org/v3/index.json
python3 - "$YAMY_VALIDATION_ROOT" <<'PY'
from pathlib import Path
import sys,xml.etree.ElementTree as E
out=Path(sys.argv[1]);repo=Path.cwd();packages=repo/'packages';packages.mkdir(exist_ok=True)
for x in E.parse('RestaurantWebsite/packages.config').getroot():
 link=packages/(x.get('id')+'.'+x.get('version'))
 if not link.exists():link.symlink_to(out/'nuget'/x.get('id').lower()/x.get('version'),target_is_directory=True)
(out/'linux-compile.targets').write_text('<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003"><Target Name="KillVBCSCompilerBeforeCopy"/><Target Name="KillVBCSCompilerBeforeClean"/></Project>')
PY
for YAMY_CONFIGURATION in Debug Release; do
  "$YAMY_DOTNET" msbuild RestaurantWebsite/RestaurantWebsite.csproj /t:Build "/p:Configuration=$YAMY_CONFIGURATION" "/p:FrameworkPathOverride=$YAMY_VALIDATION_ROOT/nuget/microsoft.netframework.referenceassemblies.net48/1.0.3/build/.NETFramework/v4.8" "/p:VSToolsPath=$YAMY_VALIDATION_ROOT/nuget/msbuild.microsoft.visualstudio.web.targets/14.0.0.3/tools/VSToolsPath" "/p:CustomAfterMicrosoftCommonTargets=$YAMY_VALIDATION_ROOT/linux-compile.targets" /p:AutomaticallyUseReferenceAssemblyPackages=false
done
"$YAMY_DOTNET" run --project tests/RestaurantWebsite.Tests
node tests/frontend-smoke.cjs
"$YAMY_DOTNET" run --project scripts/RazorCheck -- "$YAMY_REPO_ROOT/RestaurantWebsite" "$YAMY_VALIDATION_ROOT/razor-generated"
python3 scripts/razor-project.py "$YAMY_VALIDATION_ROOT"
"$YAMY_DOTNET" msbuild "$YAMY_VALIDATION_ROOT/razor-validation.csproj" "/p:FrameworkPathOverride=$YAMY_VALIDATION_ROOT/nuget/microsoft.netframework.referenceassemblies.net48/1.0.3/build/.NETFramework/v4.8" /p:AutomaticallyUseReferenceAssemblyPackages=false
"$YAMY_DOTNET" run --project scripts/ModelSnapshot -- "$YAMY_VALIDATION_ROOT/model.txt"
python3 scripts/check-model-snapshot.py "$YAMY_VALIDATION_ROOT/model.txt"
printf '%s\n' 'Linux compilation, Razor and domain checks passed. Windows runtime checks remain separate.'
