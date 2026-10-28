"""Generate an external net48 project to compile the C# output of RazorCheck."""
from pathlib import Path
import sys,xml.etree.ElementTree as E
out=Path(sys.argv[1]).resolve();app=Path(__file__).resolve().parents[1]/'RestaurantWebsite'
ns='http://schemas.microsoft.com/developer/msbuild/2003';E.register_namespace('',ns)
r=E.Element('{%s}Project'%ns,ToolsVersion='Current',DefaultTargets='Build');E.SubElement(r,'{%s}Import'%ns,Project='$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props')
p=E.SubElement(r,'{%s}PropertyGroup'%ns)
for k,v in {'TargetFrameworkVersion':'v4.8','OutputType':'Library','OutputPath':'razor-bin','LangVersion':'latest'}.items():E.SubElement(p,'{%s}%s'%(ns,k)).text=v
g=E.SubElement(r,'{%s}ItemGroup'%ns)
for x in E.parse(app/'RestaurantWebsite.csproj').findall('.//{%s}Reference'%ns):
 for hint in x.findall('{%s}HintPath'%ns):hint.text=str((app/hint.text.replace('\\','/')).resolve())
 g.append(x)
x=E.SubElement(g,'{%s}Reference'%ns,Include='RestaurantWebsite');E.SubElement(x,'{%s}HintPath'%ns).text=str(app/'bin/RestaurantWebsite.dll')
E.SubElement(g,'{%s}Compile'%ns,Include=str(out/'razor-generated/*.cs'))
E.SubElement(r,'{%s}Import'%ns,Project='$(MSBuildBinPath)\\Microsoft.CSharp.targets');E.indent(r)
E.ElementTree(r).write(out/'razor-validation.csproj',encoding='utf-8',xml_declaration=True)
