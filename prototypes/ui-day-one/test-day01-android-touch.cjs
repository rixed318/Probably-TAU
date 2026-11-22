const {test}=require('node:test');const assert=require('node:assert/strict');const fs=require('node:fs');const vm=require('node:vm');const path=require('node:path');const html=fs.readFileSync(path.join(__dirname,'day01-android-touch.html'),'utf8');test('Android single-file offline bundle has three ordered parseable inline scripts and no remote resources',()=>{assert.match(html,/viewport-fit=cover/);assert.match(html,/env\(safe-area-inset-top\)/);assert.match(html,/min-height:48px/);assert.match(html,/min-width:48px/);assert.match(html,/touch-action:manipulation/);assert.match(html,/@media\s*\(orientation:landscape\)\s*and\s*\(max-height:520px\)/);assert.match(html,/@media\s*\(max-width:650px\)\s*and\s*\(orientation:portrait\)/);for(const side of ['top','right','bottom','left'])assert.match(html,new RegExp('env\\(safe-area-inset-'+side+'\\)'));assert.doesNotMatch(html,/<script[^>]+src=/i);assert.doesNotMatch(html,/<link[^>]+href=/i);const scripts=[...html.matchAll(/<script>([\s\S]*?)<\/script>/g)].map(x=>x[1]);assert.equal(scripts.length,3);for(const script of scripts)new vm.Script(script);assert.match(scripts[0],/TAU_DAY01_READ_MODEL/);assert.match(scripts[1],/TAU_DAY01_GAMEPLAY/);assert.match(scripts[2],/createGateway/);});
// Offline packaging regression: reject less obvious network dependencies as well as script src.
test('Android bundle has no CSS imports, remote media, frames or runtime network calls',()=>{
  assert.doesNotMatch(html,/@import\b/i);
  assert.doesNotMatch(html,/url\s*\(\s*['\"]?https?:/i);
  assert.doesNotMatch(html,/<(?:iframe|object|embed|audio|video|img)\b[^>]*(?:src|data)\s*=\s*['\"]?https?:/i);
  assert.doesNotMatch(html,/\b(?:fetch\s*\(|XMLHttpRequest\b|WebSocket\s*\(|EventSource\s*\()/);
  assert.doesNotMatch(html,/\b(?:navigator\s*\.\s*sendBeacon\s*\(|importScripts\s*\(|new\s+Worker\s*\()/i);
  assert.doesNotMatch(html,/url\s*\(\s*['\"]?\/\//i);
  assert.doesNotMatch(html,/<(?:script|link)\b[^>]*\b(?:src|href)\s*=/i);
});

// Both published files must remain self-contained; the legacy offline demo has a different script layout.
test('second packaged Android offline HTML remains standalone and parseable',()=>{
  const legacy=fs.readFileSync(path.join(__dirname,'day01-android-offline.html'),'utf8');
  assert.match(legacy,/viewport-fit=cover/);
  assert.match(legacy,/env\(safe-area-inset-top(?:\s*,\s*[^)]*)?\)/);
  assert.match(legacy,/min-height:48px/);
  // Mobile regression: the legacy desktop CSS has 35px small buttons; coarse-pointer
  // override must provide both dimensions and preserve all four Android cutout insets.
  assert.match(legacy,/@media\s*\(pointer\s*:\s*coarse\)/);
  assert.match(legacy,/min-width\s*:\s*48px/);
  for(const side of ['top','right','bottom','left']) {
    assert.ok(legacy.includes('env(safe-area-inset-'+side),'missing safe area '+side);
  }
  assert.doesNotMatch(legacy,/<(?:script|link)\b[^>]*\b(?:src|href)\s*=/i);
  assert.doesNotMatch(legacy,/@import\b|url\s*\(\s*['"]?https?:/i);
  assert.doesNotMatch(legacy,/<(?:iframe|object|embed|audio|video|img)\b[^>]*(?:src|data)\s*=\s*['"]?https?:/i);
  assert.doesNotMatch(legacy,/\b(?:fetch\s*\(|XMLHttpRequest\b|WebSocket\s*\(|EventSource\s*\()/);
  // Apply the same offline escape-route checks to the second distributed HTML.
  assert.doesNotMatch(legacy,/\b(?:navigator\s*\.\s*sendBeacon\s*\(|importScripts\s*\(|new\s+Worker\s*\()/i);
  assert.doesNotMatch(legacy,/url\s*\(\s*['\"]?\/\//i);
  const scripts=[...legacy.matchAll(/<script(?:\s[^>]*)?>([\s\S]*?)<\/script>/g)].map(m=>m[1]);
  assert.ok(scripts.length>0,'expected embedded executable JavaScript');
  for(const script of scripts)new vm.Script(script);
});

// Prevent protocol-relative external media from bypassing the offline http(s) guard.
// These assertions cover static HTML only, not computed DOM assignments.
test('both Android bundles reject protocol-relative media sources',()=>{
  const mediaSource = /<(?:iframe|object|embed|audio|video|img|source|track)\b[^>]*\b(?:src|data|poster)\s*=\s*['"]?\/\//i;
  for(const filename of ['day01-android-touch.html','day01-android-offline.html']){
    const source=fs.readFileSync(path.join(__dirname,filename),'utf8');
    assert.doesNotMatch(source,mediaSource,filename+' must not load protocol-relative media');
  }
  for(const snippet of ['<img src="//cdn.invalid/a.png">','<video poster="//cdn.invalid/a.png">','<object data="//cdn.invalid/a.svg">'])
    assert.match(snippet,mediaSource,'must detect protocol-relative media');
  assert.doesNotMatch('<img src="./local.png">',mediaSource);
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
  /\b(?:(?:window|document|top|self|parent|globalThis)\s*\.\s*)?location\s*(?:=|\.\s*(?:href\s*=|assign\s*\(|replace\s*\())|\bwindow\s*\.\s*open\s*\(/i,
  /<a\b[^>]*href\s*=\s*["']?(?:https?:|\/\/)/i,
];
test('Android touch bundle has no remote forms, redirects or external navigation',()=>{
  for(const pattern of forbiddenNavigation) assert.doesNotMatch(html,pattern);
});
// Keep both downloadable Android HTML variants under the same navigation policy.
test('legacy offline bundle has no forms or navigation escapes',()=>{
  const legacy=fs.readFileSync(path.join(__dirname,'day01-android-offline.html'),'utf8');
  for(const pattern of forbiddenNavigation) assert.doesNotMatch(legacy,pattern);
});
test('offline navigation guards reject representative forbidden snippets',()=>{
  const forbidden = [
    '<form action="https://example.invalid">',
    '<meta http-equiv="refresh" content="0;url=https://example.invalid">',
    'window.location="https://example.invalid"',
    '<a href="//example.invalid">external</a>',
  ];
  forbidden.forEach((snippet,i)=>assert.match(snippet,forbiddenNavigation[i]));
  for(const snippet of [
    'location.href="https://example.invalid"',
    'document.location = "https://example.invalid"',
    'top.location.replace("https://example.invalid")',
    'self.location.assign("https://example.invalid")',
    'window.open("https://example.invalid")',
    'parent.location.assign("https://example.invalid")',
    'globalThis.location.href="https://example.invalid"',
    'parent.location="https://example.invalid"',
  ]) assert.match(snippet,forbiddenNavigation[2]);
});

// Duplicate static IDs silently redirect getElementById bindings and can break touch
// controls or accessibility relationships. Check both distributable Android variants.
test('Android offline HTML variants have unique static element IDs',()=>{
  for(const filename of ['day01-android-touch.html','day01-android-offline.html']){
    const source=fs.readFileSync(path.join(__dirname,filename),'utf8');
    const ids=[...source.matchAll(/<[^>]+\sid\s*=\s*["']([^"']+)["']/gi)].map(m=>m[1]);
    assert.ok(ids.length>0,filename+' must contain ID-bound controls');
    const duplicates=ids.filter((id,i)=>ids.indexOf(id)!==i);
    assert.deepEqual([...new Set(duplicates)],[],filename+' has duplicate static IDs');
  }
});


// Static accessibility relationships must not point to missing elements.
// This covers HTML-authored references only, not IDs created by JavaScript.
test('Android offline HTML variants resolve static ARIA ID references',()=>{
  for(const filename of ['day01-android-touch.html','day01-android-offline.html']){
    const source=fs.readFileSync(path.join(__dirname,filename),'utf8');
    const ids=new Set([...source.matchAll(/<[^>]+\sid\s*=\s*["']([^"']+)["']/gi)].map(m=>m[1]));
    for(const match of source.matchAll(/\baria-(?:labelledby|describedby|controls|owns)\s*=\s*["']([^"']+)["']/gi)){
      for(const ref of match[1].trim().split(/\s+/)){
        assert.ok(ids.has(ref),filename+' has unresolved ARIA reference: '+ref);
      }
    }
  }
});


// Mobile offline regression: initial focus must remain under the player's control.
// Autofocus can summon the Android keyboard and obscure the landscape Day01 HUD.
test('Android offline variants do not force autofocus on load',()=>{
  for(const filename of ['day01-android-touch.html','day01-android-offline.html']){
    const source=fs.readFileSync(path.join(__dirname,filename),'utf8');
    assert.doesNotMatch(source,/<(?:input|button|textarea|select|a)\b[^>]*\sautofocus(?:\s|=|\/?>)/i,
      filename+' must not force initial focus');
  }
});

// A <base> element can silently redirect relative assets and links to a remote
// origin even when src/href themselves look local. Keep both offline exports safe.
test('Android offline variants reject base URL overrides',()=>{
  const baseTag=/<base\b[^>]*>/i;
  for(const filename of ['day01-android-touch.html','day01-android-offline.html']){
    const source=fs.readFileSync(path.join(__dirname,filename),'utf8');
    assert.doesNotMatch(source,baseTag,filename+' must not override the file base URL');
  }
  assert.match('<base href="https://example.invalid/">',baseTag);
  assert.match('<base target="_blank">',baseTag);
  assert.doesNotMatch('<baseboard>not a base tag</baseboard>',baseTag);
});

// Responsive images can fetch remote resources via srcset even when src is local.
// Check both offline distributions and preserve local/data-only candidates.
test('Android offline variants reject remote responsive image srcset',()=>{
  const remoteSrcset=/<(?:img|source)\b[^>]*\bsrcset\s*=\s*(["'])(?:(?!\1)[\s\S])*?(?:https?:\/\/|\/\/)[^"']*\1/i;
  // HTML permits unquoted srcset values; reject remote URLs in that form too.
  const unquotedRemoteSrcset=/<(?:img|source)\b[^>]*\bsrcset\s*=\s*(?!["'])[^\s>]*(?:https?:\/\/|\/\/)/i;
  for(const filename of ['day01-android-touch.html','day01-android-offline.html']){
    const source=fs.readFileSync(path.join(__dirname,filename),'utf8');
    assert.doesNotMatch(source,remoteSrcset,filename+' must not fetch remote srcset images');
    assert.doesNotMatch(source,unquotedRemoteSrcset,filename+' must not fetch unquoted remote srcset images');
  }
  assert.match('<img srcset="local.png 1x, https://cdn.invalid/2.png 2x">',remoteSrcset);
  assert.match('<source srcset="//cdn.invalid/image.webp 2x">',remoteSrcset);
  assert.doesNotMatch('<img srcset="local.png 1x, local@2x.png 2x">',remoteSrcset);
  assert.match('<img srcset=https://cdn.invalid/2.png>',unquotedRemoteSrcset);
  assert.match('<source srcset=//cdn.invalid/image.webp>',unquotedRemoteSrcset);
  assert.match('<img srcset=local.png,https://cdn.invalid/2.png>',unquotedRemoteSrcset);
  assert.doesNotMatch('<img srcset=local.png>',unquotedRemoteSrcset);
});

// External SVG <use> references can silently request network resources from an
// otherwise standalone HTML bundle. This is a source-only offline regression.
test('Android offline variants reject remote SVG use href references',()=>{
  const remoteUse=/<use\b[^>]*\b(?:href|xlink:href)\s*=\s*['"]?(?:https?:\/\/|\/\/)/i;
  for(const filename of ['day01-android-touch.html','day01-android-offline.html']){
    const source=fs.readFileSync(path.join(__dirname,filename),'utf8');
    assert.doesNotMatch(source,remoteUse,filename+' must not load remote SVG symbols');
  }
  assert.match('<use href="https://cdn.invalid/icons.svg#x">',remoteUse);
  assert.match('<use xlink:href="//cdn.invalid/icons.svg#x">',remoteUse);
  assert.doesNotMatch('<use href="#local-symbol">',remoteUse);
});


// Regression #20: a successful decision rebuilds the choice buttons. Keep
// keyboard focus on a persistent document heading, never a removed button.
// This is a source contract; browser/TalkBack behavior still needs device QA.
test('Android decision focus moves to persistent heading only after success',()=>{
  assert.match(html,/<h2\s+id="doc-title"\s+tabindex="-1">/i);
  const scripts=[...html.matchAll(/<script>([\s\S]*?)<\/script>/g)].map(m=>m[1]);
  const ui=scripts[2];
  assert.match(ui,/if\(action==="decision"\)el\("doc-title"\)\.focus\(\)/);
  assert.match(ui,/g\.decide\(item\.id,option\.id\);return "decision";/);
  assert.match(ui,/catch\(e\)\{el\("error"\)\.textContent=e\.message;\}/);
});

// #20: Inline event-handler attributes bypass the documented DOMContentLoaded
// wiring and can break offline touch behavior or complicate accessibility review.
test('Android HTML exports use script listeners rather than inline event attributes',()=>{
  const inlineHandler=/<[a-z][^>]*\s(?:on[a-z]+)\s*=/i;
  for(const filename of ['day01-android-touch.html','day01-android-offline.html']){
    const source=fs.readFileSync(path.join(__dirname,filename),'utf8');
    assert.doesNotMatch(source,inlineHandler,filename+' must not bind inline event handlers');
  }
  assert.match('<button onclick="decide()">',inlineHandler);
  assert.match('<img src="x" onerror="fetchSomething()">',inlineHandler);
  assert.match('<button onkeydown="decide()">',inlineHandler);
  assert.match('<form onsubmit="send()">',inlineHandler);
  assert.match('<input onfocus="track()">',inlineHandler);
  assert.doesNotMatch('<button data-onclick="none">',inlineHandler);
});

// Regression #20: inspecting a document rebuilds the UI; focus must land on
// the persistent inspection result only after successful inspection/render.
// Source-level contract only; browser and Android TalkBack still require QA.
test('Android inspection focus follows successful inspect',()=>{
  assert.match(html,/<div id="inspection" tabindex="-1" aria-live="polite">/i);
  const scripts=[...html.matchAll(/<script>([\s\S]*?)<\/script>/g)].map(m=>m[1]);
  const ui=scripts[2];
  assert.match(ui,/g\.inspect\(g\.view\(\)\.selected\);el\("error"\)\.textContent="";render\(\);el\("inspection"\)\.focus\(\);/);
  assert.match(ui,/catch\(e\)\{el\("error"\)\.textContent=e\.message;\}/);
});

// #20: EndDay replaces the active game screen with the report. The new screen
// must receive keyboard focus after a successful close, not on a failed close.
// This is a source contract; Android/TalkBack still needs device validation.
test('Android end-day transfers focus to the visible report after success',()=>{
  assert.match(html,/<section id="report" hidden tabindex="-1">/i);
  const scripts=[...html.matchAll(/<script>([\s\S]*?)<\/script>/g)].map(m=>m[1]);
  const ui=scripts[2];
  assert.match(ui,/if\(s\.closed\)\{el\("game"\)\.hidden=true;el\("report"\)\.hidden=false;/);
  // Keep the focus call inside the closed-report branch, after the report is shown.
  // A stray focus call elsewhere must not satisfy this regression.
  const closedBranch=/if\(s\.closed\)\{el\("game"\)\.hidden=true;el\("report"\)\.hidden=false;[\s\S]*?el\("report"\)\.focus\(\);\s*\}/;
  assert.match(ui,closedBranch);
  assert.doesNotMatch('if(s.closed){el("game").hidden=true;el("report").hidden=false;}el("report").focus();',closedBranch);
  assert.doesNotMatch('if(s.closed){el("game").hidden=true;el("report").hidden=false;}',closedBranch);
  assert.match(ui,/g\.endDay\(\);el\("error"\)\.textContent="";render\(\);/);
  assert.match(ui,/catch\(e\)\{el\("error"\)\.textContent=e\.message;\}/);
});

// #20: Android users must be able to pinch-zoom text. A viewport that disables
// scaling makes the landscape document and report inaccessible to low-vision users.
test('Android offline variants preserve user viewport zoom',()=>{
  for(const filename of ['day01-android-touch.html','day01-android-offline.html']){
    const source=fs.readFileSync(path.join(__dirname,filename),'utf8');
    const viewport=source.match(/<meta\s+name=["']viewport["']\s+content=["']([^"']+)["']/i);
    assert.ok(viewport,filename+' must declare viewport');
    assert.doesNotMatch(viewport[1],/user-scalable\s*=\s*(?:no|0)(?:\s*[,;]|$)/i);
    assert.doesNotMatch(viewport[1],/maximum-scale\s*=\s*1(?:\.0+)?(?:\s*[,;]|$)/i);
  }
});
