
await Promise.all([{family:"Noto Sans",style:"Regular"},{family:"Noto Sans",style:"Bold"},{family:"Noto Serif",style:"Regular"}].map(f=>figma.loadFontAsync(f)));
const variables=Object.fromEntries(await Promise.all(Object.entries(STATE.variables).map(async([k,id])=>[k,await figma.variables.getVariableByIdAsync(id)])));
const createdNodeIds=[];
function keep(n){createdNodeIds.push(n.id);return n;}
function paint(key){return figma.variables.setBoundVariableForPaint({type:"SOLID",color:{r:0,g:0,b:0}},"color",variables[key]);}
function fill(n,key){n.fills=[paint(key)];}
function stroke(n,key="border",weight=1){n.strokes=[paint(key)];n.strokeWeight=weight;}
function text(p,s,style="body",w=640,key="text"){const n=keep(figma.createText());n.name=s.split("\n")[0].slice(0,70);n.textStyleId=STATE.styles[style];n.characters=s;n.resize(w,10);n.textAutoResize="HEIGHT";fill(n,key);p.appendChild(n);return n;}
function auto(p,name,w=640,dir="VERTICAL",gap=16,bg=null,pad=0){const n=keep(figma.createAutoLayout(dir));n.name=name;n.resize(w,10);n.layoutSizingHorizontal="FIXED";n.layoutSizingVertical="HUG";n.itemSpacing=gap;if(variables["space"+gap])n.setBoundVariable("itemSpacing",variables["space"+gap]);if(pad){for(const k of ["paddingTop","paddingBottom","paddingLeft","paddingRight"]){n[k]=pad;if(variables["space"+pad])n.setBoundVariable(k,variables["space"+pad]);}} n.fills=bg?[paint(bg)]:[];if(p)p.appendChild(n);return n;}
function box(p,name,x,y,w,h,key="panel"){const n=keep(figma.createFrame());n.name=name;n.resize(w,h);fill(n,key);p.appendChild(n);n.x=x;n.y=y;return n;}
function line(p,w,key="border"){const n=keep(figma.createRectangle());n.name="Separador";n.resize(w,1);fill(n,key);p.appendChild(n);return n;}
