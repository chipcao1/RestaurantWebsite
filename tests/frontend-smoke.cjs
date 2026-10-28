const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const path = require('node:path');
const source = fs.readFileSync(path.join(__dirname, '../RestaurantWebsite/Resources/User/js/custom.js'), 'utf8');
assert.doesNotThrow(() => vm.runInNewContext(source, {
  document: { querySelector: () => null, getElementById: () => null },
  window: {},
  Date,
}), 'shared frontend must work without the old year element or optional CDN plugins');
console.log('PASS: shared frontend tolerates absent optional plugins and footer year element');
