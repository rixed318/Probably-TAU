const {test}=require('node:test');const assert=require('node:assert/strict');const fs=require('node:fs');const vm=require('node:vm');const path=require('node:path');const html=fs.readFileSync(path.join(__dirname,'day01-android-touch.html'),'utf8');test('Android single-file offline bundle has three ordered parseable inline scripts and no remote resources',()=>{assert.match(html,/viewport-fit=cover/);assert.match(html,/env\(safe-area-inset-top\)/);assert.match(html,/min-height:48px/);assert.match(html,/min-width:48px/);assert.match(html,/touch-action:manipulation/);assert.match(html,/@media\s*\(orientation:landscape\)\s*and\s*\(max-height:520px\)/);assert.match(html,/@media\s*\(max-width:650px\)\s*and\s*\(orientation:portrait\)/);for(const side of ['top','right','bottom','left'])assert.match(html,new RegExp('env\\(safe-area-inset-'+side+'\\)'));assert.doesNotMatch(html,/<script[^>]+src=/i);assert.doesNotMatch(html,/<link[^>]+href=/i);const scripts=[...html.matchAll(/<script>([\s\S]*?)<\/script>/g)].map(x=>x[1]);assert.equal(scripts.length,3);for(const script of scripts)new vm.Script(script);assert.match(scripts[0],/TAU_DAY01_READ_MODEL/);assert.match(scripts[1],/TAU_DAY01_GAMEPLAY/);assert.match(scripts[2],/createGateway/);});
// Offline packaging regression: reject less obvious network dependencies as well as script src.
test('Android bundle has no CSS imports, remote media, frames or runtime network calls',()=>{
  assert.doesNotMatch(html,/@import\b/i);
  assert.doesNotMatch(html,/url\s*\(\s*['\"]?https?:/i);
  assert.doesNotMatch(html,/<(?:iframe|object|embed|audio|video|img)\b[^>]*(?:src|data)\s*=\s*['\"]?https?:/i);
  assert.doesNotMatch(html,/\b(?:fetch\s*\(|XMLHttpRequest\b|WebSocket\s*\(|EventSource\s*\()/);
  assert.doesNotMatch(html,/<(?:script|link)\b[^>]*\b(?:src|href)\s*=/i);
});

// Both published files must remain self-contained; the legacy offline demo has a different script layout.
test('second packaged Android offline HTML remains standalone and parseable',()=>{
  const legacy=fs.readFileSync(path.join(__dirname,'day01-android-offline.html'),'utf8');
  assert.match(legacy,/viewport-fit=cover/);
  assert.match(legacy,/env\(safe-area-inset-top(?:\s*,\s*[^)]*)?\)/);
  assert.match(legacy,/min-height:48px/);
  assert.doesNotMatch(legacy,/<(?:script|link)\b[^>]*\b(?:src|href)\s*=/i);
  assert.doesNotMatch(legacy,/@import\b|url\s*\(\s*['"]?https?:/i);
  assert.doesNotMatch(legacy,/<(?:iframe|object|embed|audio|video|img)\b[^>]*(?:src|data)\s*=\s*['"]?https?:/i);
  assert.doesNotMatch(legacy,/\b(?:fetch\s*\(|XMLHttpRequest\b|WebSocket\s*\(|EventSource\s*\()/);
  const scripts=[...legacy.matchAll(/<script(?:\s[^>]*)?>([\s\S]*?)<\/script>/g)].map(m=>m[1]);
  assert.ok(scripts.length>0,'expected embedded executable JavaScript');
  for(const script of scripts)new vm.Script(script);
});

// Regression for the loading-screen failure: scripts are in <head>, so DOM nodes
// do not exist until DOMContentLoaded. Keep event wiring and first render deferred.
test('Android Day01 defers DOM wiring and first render until DOMContentLoaded',()=>{
  const scripts=[...html.matchAll(/<script>([\s\S]*?)<\/script>/g)].map(m=>m[1]);
  const ui=scripts[2];
  assert.match(html,/<\/script><\/head>\s*<body>/i,'UI script must remain before body for this regression');
  assert.match(ui,/d\.addEventListener\(["']DOMContentLoaded["'],\s*\(\)\s*=>\s*\{/);
  const handler=ui.match(/d\.addEventListener\(["']DOMContentLoaded["'],\s*\(\)\s*=>\s*\{([\s\S]*?)\n\}\);/);
  assert.ok(handler,'expected explicit DOM-ready handler');
  assert.match(handler[1],/el\(["']inspect["']\)\.addEventListener/);
  assert.match(handler[1],/el\(["']end["']\)\.addEventListener/);
  assert.match(handler[1],/render\(\)/);
  assert.doesNotMatch(ui.slice(0,ui.indexOf('d.addEventListener("DOMContentLoaded"')),/\nrender\(\);/);
});

// Regression for #20: a failed action may leave #error populated; a subsequent
// successful inspect/end-day must clear it before repainting the view.
test('Android successful inspect and end-day clear stale validation errors',()=>{
  const scripts=[...html.matchAll(/<script>([\s\S]*?)<\/script>/g)].map(m=>m[1]);
  const ui=scripts[2];
  for(const [id,operation] of [['inspect','g.inspect(g.view().selected)'],['end','g.endDay()']]){
    const handler=ui.match(new RegExp('el\\("'+id+'"\\)\\.addEventListener\\("click",\\(\\)=>\\{try\\{([^}]+)\\}catch\\(e\\)'));
    assert.ok(handler,id+' click handler must retain success/error branches');
    const success=handler[1];
    const op=success.indexOf(operation);
    const clear=success.indexOf('el("error").textContent=""');
    const paint=success.indexOf('render()');
    assert.ok(op>=0 && clear>op && paint>clear,id+' must clear stale error after success, before render');
  }
});


// Accessibility regression for #20: the progress count is announced as a single
// polite status update, without moving keyboard focus after each decision.
test('Android decision progress is a polite atomic live status',()=>{
  assert.match(html,/<p\s+id="progress"\s+role="status"\s+aria-live="polite"\s+aria-atomic="true"\s*>\s*<\/p>/i);
  const scripts=[...html.matchAll(/<script>([\s\S]*?)<\/script>/g)].map(m=>m[1]);
  const ui=scripts[2];
  assert.match(ui,/el\("progress"\)\.textContent="Решений: "\+done\.size\+" из 3"/);
  assert.match(html,/<p\s+id="error"\s+role="alert"\s*>/i);
});

// Offline distribution must not silently navigate away or submit visitor decisions
// to a remote endpoint. These are static guards, not browser/device tests.
const forbiddenNavigation = [
  /<form\b/i,
  /<meta\b[^>]*http-equiv\s*=\s*["']?refresh/i,
  /\b(?:location\.(?:href|assign|replace)|window\.open)\s*(?:=|\()/i,
  /<a\b[^>]*href\s*=\s*["']?(?:https?:|\/\/)/i,
];
test('Android touch bundle has no remote forms, redirects or external navigation',()=>{
  for(const pattern of forbiddenNavigation) assert.doesNotMatch(html,pattern);
});
test('offline navigation guards reject representative forbidden snippets',()=>{
  const forbidden = [
    '<form action="https://example.invalid">',
    '<meta http-equiv="refresh" content="0;url=https://example.invalid">',
    'location.href="https://example.invalid"',
    '<a href="//example.invalid">external</a>',
  ];
  forbidden.forEach((snippet,i)=>assert.match(snippet,forbiddenNavigation[i]));
});
