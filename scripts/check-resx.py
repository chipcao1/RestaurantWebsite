"""Validate headers required by the Windows .NET Framework ResX reader."""
from pathlib import Path
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[1] / 'RestaurantWebsite'
expected = {
    'resmimetype': 'text/microsoft-resx',
    'version': '2.0',
    'reader': 'System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089',
    'writer': 'System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089',
}
files = list(root.rglob('*.resx'))
for path in files:
    headers = {x.get('name'): x.findtext('value') for x in ET.parse(path).getroot().findall('resheader')}
    for name, value in expected.items():
        assert headers.get(name) == value, f'{path.name}: missing or invalid resheader {name}'
print(f'ResX headers valid: {len(files)} files')
