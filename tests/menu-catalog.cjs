const assert=require('node:assert/strict');
const fs=require('node:fs');
const path=require('node:path');
const vm=require('node:vm');
const filename=path.join(__dirname,'../RestaurantWebsite/Scripts/menu-catalog.js');
assert.ok(fs.existsSync(filename),'Home and Menu must share an in-place catalog filter instead of category navigation');
function element(dataset={}){
 const handlers={};const classes=new Set();
 return{dataset,value:'',hidden:false,textContent:'',attributes:{},classList:{toggle(name,active){if(active)classes.add(name);else classes.delete(name);}},
 addEventListener(name,fn){(handlers[name]||=[]).push(fn);},
 dispatch(name,target){const event={target:target||this,defaultPrevented:false,preventDefault(){this.defaultPrevented=true;}};for(const fn of handlers[name]||[])fn(event);return event;},
 setAttribute(name,value){this.attributes[name]=value;},
 querySelector(){return null;},querySelectorAll(){return [];}};
}
function catalog(selected='all'){
 const root=element({selectedCategory:selected});
 const filters=['all','1','2','combo'].map(category=>element({menuFilter:category}));
 const cards=Array.from({length:9},(_,i)=>element({category:i<4?'1':'2',kind:'dish',search:i===8?'Phở gà thơm ngon':'Món '+i}));
 cards.push(element({category:'',kind:'combo',search:'Suất gia đình'}));
 const input=element(),form=element(),count=element(),empty=element(),modal=element(),body=element(),title=element();
 const details=[element({menuDetail:"/dish/1",detailTitle:"Chi tiết món ăn"}),element({menuDetail:"/dish/2",detailTitle:"Chi tiết món ăn"})];
 root.querySelector=selector=>({'[data-menu-search]':input,'[data-menu-form]':form,'[data-menu-count]':count,'[data-menu-empty]':empty,'[data-menu-modal]':modal,'[data-menu-modal-body]':body,'[data-menu-modal-title]':title}[selector]||null);
 root.querySelectorAll=selector=>selector==='[data-menu-filter]'?filters:selector==='[data-menu-item]'?cards:selector==='[data-menu-detail]'?details:[];
 return{root,filters,cards,input,form,count,empty,details,body,title};
}
const home=catalog(),menu=catalog('2');
const requests=[];
vm.runInNewContext(fs.readFileSync(filename,'utf8'),{document:{querySelectorAll:()=>[home.root,menu.root]},window:{jQuery:()=>({modal(){}})},fetch(url,options){return new Promise(resolve=>requests.push({url,options,resolve}));}});
const visible=c=>c.cards.filter(item=>!item.hidden).length;
assert.equal(visible(home),10,'home shows more than the former six-item subset');
assert.equal(visible(menu),5,'category URL initializes filter without removing other cards');
home.filters[1].dispatch('click');assert.equal(visible(home),4);
assert.equal(visible(menu),5,'filter events must stay within their catalog instance');
home.filters[0].dispatch('click');assert.equal(visible(home),10,'All restores the full catalog');
menu.filters[0].dispatch('click');assert.equal(visible(menu),10,'All after a category URL still has the complete catalog');
home.filters[3].dispatch('click');assert.equal(visible(home),1);
home.filters[0].dispatch('click');home.input.value='PHỞ';home.input.dispatch('input');assert.equal(visible(home),1);
home.input.value='không tồn tại';home.input.dispatch('input');assert.equal(home.empty.hidden,false);assert.equal(visible(home),0);
home.input.value='';home.input.dispatch('input');assert.equal(visible(home),10);assert.equal(home.empty.hidden,true);
assert.equal(home.form.dispatch('submit').defaultPrevented,true,'search submits in place without leaving Home');
assert.equal(home.filters[0].attributes['aria-pressed'],'true');
assert.equal(requests.length,0,'filtering must never fetch a partial catalog');
async function detailRace(){
 home.details[0].dispatch('click');home.details[1].dispatch('click');
 assert.equal(requests[0].options.cache,'no-store','details must not reuse guest/customer-specific cached HTML');
 requests[1].resolve({ok:true,text:async()=>'<h3>Món thứ hai</h3>'});await new Promise(setImmediate);
 requests[0].resolve({ok:true,text:async()=>'<h3>Món thứ nhất</h3>'});await new Promise(setImmediate);
 assert.equal(home.body.innerHTML,'<h3>Món thứ hai</h3>','late detail responses cannot replace the latest selected dish');
 console.log('PASS: shared Home/Menu filters restore all dishes, search in place, isolate components and guard stale detail responses');
}
detailRace().catch(error=>{console.error(error);process.exitCode=1;});
