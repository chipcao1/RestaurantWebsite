"""Keep explicit classic csproj entries synchronized after moving MVC files."""
from pathlib import Path
import xml.etree.ElementTree as E
root=Path(__file__).resolve().parents[1]/'RestaurantWebsite'
f=root/'RestaurantWebsite.csproj'
ns='http://schemas.microsoft.com/developer/msbuild/2003'; E.register_namespace('',ns)
t=E.parse(f);r=t.getroot()
for group in r.findall('{%s}ItemGroup'%ns):
 for x in list(group):
  if x.tag=='{%s}Compile'%ns or (x.tag=='{%s}Content'%ns and x.get('Include','').endswith('.cshtml')): group.remove(x)
g=E.SubElement(r,'{%s}ItemGroup'%ns)
for p in sorted(root.rglob('*')):
 if any(x in ('bin','obj','packages') for x in p.relative_to(root).parts):continue
 if p.suffix in ('.cs','.cshtml'):
  E.SubElement(g,'{%s}%s'%(ns,'Compile' if p.suffix=='.cs' else 'Content'),Include=str(p.relative_to(root)).replace('/','\\'))
E.indent(t,space='  ')
t.write(f,encoding='utf-8',xml_declaration=True)
