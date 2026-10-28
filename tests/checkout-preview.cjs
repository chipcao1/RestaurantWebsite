const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const path = require('node:path');
const script = path.join(__dirname, '../RestaurantWebsite/Scripts/checkout.js');
assert.ok(fs.existsSync(script), 'checkout must validate promotion and update its summary before placing an order');
function node(value = '') {
  const handlers = {};
  return { value, textContent: '', hidden: false, disabled: false, className: '', dataset: {},
    addEventListener(name, handler) { (handlers[name] ||= []).push(handler); },
    dispatch(name, extra = {}) { const event = { preventDefault() { this.defaultPrevented = true; }, ...extra }; (handlers[name] || []).forEach(h => h(event)); return event; },
    setCustomValidity(message) { this.validationMessage = message; },
  };
}
async function main() {
  const ids = Object.fromEntries(['checkoutForm','promotionCode','promotionMessage','promotionSummary','promotionDescription','discountPreview','checkoutTotal','summarySubtotal','placeOrderButton','OrderType','deliveryAddressGroup','DeliveryAddress','dineInGroup','ReservationId','TableId'].map(id => [id, node()]));
  const token = node('csrf-token');
  ids.checkoutForm.dataset = { previewUrl: '/customer/Order/PreviewPromotion', subtotal: '200000' };
  ids.checkoutForm.querySelector = () => token;
  ids.OrderType.value = 'Delivery';
  const timers = new Map(); let timerId = 0;
  const requests = [];
  const window = { setTimeout(fn) { timers.set(++timerId, fn); return timerId; }, clearTimeout(id) { timers.delete(id); } };
  vm.runInNewContext(fs.readFileSync(script, 'utf8'), {
    document: { getElementById: id => ids[id] || null }, window, URLSearchParams, Intl,
    fetch(url, options) { return new Promise((resolve, reject) => requests.push({ url, options, resolve, reject })); },
  });
  const flush = async () => { const jobs = [...timers.values()]; timers.clear(); jobs.forEach(fn => fn()); await new Promise(setImmediate); };
  const settle = async (request, result) => { request.resolve({ ok: true, redirected: false, json: async () => result }); await new Promise(setImmediate); };
  assert.equal(ids.DeliveryAddress.required, true, 'delivery requires address');
  ids.OrderType.value='Takeaway';ids.OrderType.dispatch('change');
  assert.equal(ids.DeliveryAddress.required, false, 'takeaway has no hidden required address');
  ids.promotionCode.value = 'YM10'; ids.promotionCode.dispatch('input'); await flush();
  assert.equal(requests.length, 1, 'typing a code triggers a server validation');
  assert.equal(requests[0].options.method, 'POST');
  const body = new URLSearchParams(requests[0].options.body);
  assert.equal(body.get('__RequestVerificationToken'), 'csrf-token');
  assert.equal(body.get('promotionCode'), 'YM10');
  assert.equal(body.has('subtotal'), false, 'preview never trusts a browser subtotal');
  await settle(requests[0], { success:true, code:'YM10', description:'Giảm 10% (tối đa 50.000 ₫)', subtotal:200000, discount:20000, total:180000 });
  assert.equal(ids.promotionSummary.hidden, false);
  assert.match(ids.promotionDescription.textContent, /YM10.*10%/);
  assert.match(ids.discountPreview.textContent, /20[.,]000/);
  assert.match(ids.checkoutTotal.textContent, /180[.,]000/);
  assert.equal(ids.placeOrderButton.disabled, false);
  ids.promotionCode.dispatch('change');await flush();
  assert.equal(requests.length, 1, 'leaving a validated code must not disable the order button or send a duplicate request');
  ids.promotionCode.value = 'BAD'; ids.promotionCode.dispatch('input'); await flush();
  await settle(requests[1], { success:false, message:'Mã khuyến mãi không hợp lệ.' });
  assert.equal(ids.promotionSummary.hidden, true, 'invalid code clears previous discount');
  assert.match(ids.checkoutTotal.textContent, /200[.,]000/);
  assert.equal(ids.placeOrderButton.disabled, true);
  ids.promotionCode.value = 'OLD'; ids.promotionCode.dispatch('input'); await flush();
  ids.promotionCode.value = 'NEW'; ids.promotionCode.dispatch('input'); await flush();
  await settle(requests[3], { success:true, code:'NEW', description:'Giảm 30.000 ₫', subtotal:200000, discount:30000, total:170000 });
  await settle(requests[2], { success:true, code:'OLD', description:'Giảm 10%', subtotal:200000, discount:20000, total:180000 });
  assert.match(ids.promotionDescription.textContent, /NEW/, 'late response must not overwrite newer code');
  assert.match(ids.checkoutTotal.textContent, /170[.,]000/);
  ids.promotionCode.value=''; ids.promotionCode.dispatch('input'); await flush();
  assert.equal(ids.promotionSummary.hidden, true);
  assert.equal(ids.placeOrderButton.disabled, false, 'empty code permits checkout without a discount');
  assert.match(ids.checkoutTotal.textContent, /200[.,]000/);
  ids.promotionCode.value='NETWORK';ids.promotionCode.dispatch('input');await flush();
  requests[4].reject(new Error('network offline'));await new Promise(setImmediate);
  assert.equal(ids.placeOrderButton.disabled, true);
  assert.match(ids.promotionMessage.textContent, /Không thể kiểm tra mã/);
  ids.promotionCode.value='SLOW';ids.promotionCode.dispatch('input');await flush();
  ids.promotionCode.value='';ids.promotionCode.dispatch('input');await flush();
  await settle(requests[5],{ success:true,code:'SLOW',description:'Giảm 10%',subtotal:200000,discount:20000,total:180000 });
  assert.equal(ids.promotionSummary.hidden,true,'clearing a pending code invalidates its eventual response');
  assert.match(ids.checkoutTotal.textContent,/200[.,]000/);
  console.log('PASS: immediate promotion preview, totals, CSRF, invalid/cleared codes and out-of-order responses');
}
main().catch(error => { console.error(error); process.exitCode = 1; });
