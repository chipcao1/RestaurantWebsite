const assert=require('node:assert/strict'),fs=require('node:fs'),vm=require('node:vm'),path=require('node:path');
const file=path.join(__dirname,'../RestaurantWebsite/Scripts/food-images.js');
assert.ok(fs.existsSync(file),'remote food images must fall back safely when unavailable');
function image(complete=false,width=0){return{tagName:'IMG',complete,naturalWidth:width,dataset:{},src:'https://example.test/photo.jpg',hasAttribute:name=>name==='data-food-image'};}
const broken=image(true,0),working=image(true,500),events={};
vm.runInNewContext(fs.readFileSync(file,'utf8'),{document:{getElementById:()=>({dataset:{placeholder:'/Content/images/food-placeholder.svg'}}),addEventListener:(name,fn)=>events[name]=fn,querySelectorAll:()=>[broken,working]}});
assert.equal(broken.src,'/Content/images/food-placeholder.svg');assert.equal(working.src,'https://example.test/photo.jpg');
const dynamic=image();events.error({target:dynamic});assert.equal(dynamic.src,'/Content/images/food-placeholder.svg','modal image failures also use fallback');
events.error({target:dynamic});assert.equal(dynamic.src,'/Content/images/food-placeholder.svg','failed fallback must not cause an infinite error loop');
dynamic.src='https://example.test/new-broken-photo.jpg';events.error({target:dynamic});assert.equal(dynamic.src,'/Content/images/food-placeholder.svg','changing a preview image can fall back again for a new failed source');
console.log('PASS: failed initial and dynamic food images fall back once while valid images remain unchanged');
