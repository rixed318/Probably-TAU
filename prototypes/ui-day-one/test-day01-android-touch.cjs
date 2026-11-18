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
  assert.match(legacy,/env\(safe-area-inset-top\)/);
  assert.match(legacy,/min-height:48px/);
  assert.doesNotMatch(legacy,/<(?:script|link)\b[^>]*\b(?:src|href)\s*=/i);
  assert.doesNotMatch(legacy,/@import\b|url\s*\(\s*['"]?https?:/i);
  assert.doesNotMatch(legacy,/<(?:iframe|object|embed|audio|video|img)\b[^>]*(?:src|data)\s*=\s*['"]?https?:/i);
  assert.doesNotMatch(legacy,/\b(?:fetch\s*\(|XMLHttpRequest\b|WebSocket\s*\(|EventSource\s*\()/);
  const scripts=[...legacy.matchAll(/<script(?:\s[^>]*)?>([\s\S]*?)<\/script>/g)].map(m=>m[1]);
  assert.ok(scripts.length>0,'expected embedded executable JavaScript');
  for(const script of scripts)new vm.Script(script);
});
