"""Require the actual EF6 model to match main's unchanged InitialCreate snapshot."""
from pathlib import Path
import base64,gzip,sys,xml.etree.ElementTree as E
root=Path(__file__).resolve().parents[1]
def read(text):return E.fromstring(gzip.decompress(base64.b64decode(text)))
def canonical(e):
 # Linked model sources compile in the tooling assembly; CLR assembly identity
 # is not database schema. Keep every schema/mapping attribute in the comparison.
 attributes=tuple(sorted((k,v) for k,v in e.attrib.items() if k!='{http://schemas.microsoft.com/ado/2013/11/edm/customannotation}ClrType'))
 return (e.tag,attributes,(e.text or '').strip(),tuple(sorted((canonical(c) for c in e),key=repr)))
actual=Path(sys.argv[1]).read_text()
actual_xml=gzip.decompress(base64.b64decode(actual)).decode().replace('CodeFirstNamespace','RestaurantWebsite.Models').replace('CodeFirstContainer','AppDbContext')
res=E.parse(root/'RestaurantWebsite/Migrations/202609281435011_InitialCreate.resx').getroot()
saved=next(d.find('value').text for d in res.findall('data') if d.get('name')=='Target')
assert canonical(E.fromstring(actual_xml))==canonical(read(saved)), 'Migration snapshot differs from the actual EF6 model'
assert sorted(p.name for p in (root/'RestaurantWebsite/Migrations').glob('*.resx')) == ['202609281435011_InitialCreate.resx'], 'Yamy must not add database migrations'
print('EF6 model matches main InitialCreate; no additional migrations')
