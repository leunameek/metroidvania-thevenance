
const componentNodes=Object.fromEntries(await Promise.all(Object.entries(STATE.components).map(async([k,v])=>[k,await figma.getNodeByIdAsync(v.id)])));
const links=[],frames={};
function instance(p,key,label,w=400,value=null){const n=keep(componentNodes[key].createInstance());p.appendChild(n);const meta=STATE.components[key];const props={};if(meta.props.label)props[meta.props.label]=label;if(meta.props.value&&value!==null)props[meta.props.value]=value;n.setProperties(props);n.resize(w,key==="progress"?12:key==="status"?60:52);
const texts=n.findAllWithCriteria({types:["TEXT"]});texts.forEach((t,i)=>{const tw=meta.props.value?(i===0?w*.64-24:w*.36-24):w-32;t.resize(tw,10);t.textAutoResize="HEIGHT";});if(key!=="progress"){let th=Math.max(...texts.map(t=>t.height),24);n.resize(w,Math.max(52,th+24));}
n.findAll().forEach(c=>createdNodeIds.push(c.id));return n;}
function button(p,label,to=null,w=360,focused=false){const n=instance(p,to===null?"buttonNormal":focused?"buttonFoco":"buttonNormal",(focused?"◆ ":"")+label,w);n.name="Acción · "+label;if(to)links.push({from:n.id,to});return n;}
function progress(p,w,f=.5){const n=instance(p,"progress","",w);const v=n.children[0];v.resize(Math.max(1,w*f),12);return n;}
function status(p,s,w=600){return instance(p,"status",s,w);}
function positioned(p,name,x,y,w,dir="VERTICAL",gap=16,bg=null,pad=0){const n=auto(p,name,w,dir,gap,bg,pad);n.x=x;n.y=y;return n;}
function backdrop(s,object=false){fill(s,"world");const horizon=box(s,"Reserva de escenario",48,100,1184,460,"world");stroke(horizon);
const t=text(horizon,object?"MODELO EXISTENTE · INSPECCIÓN":"ESCENARIO EXISTENTE · ESPACIO RESERVADO","note",600,"secondary");t.x=24;t.y=24;
if(!object){const player=box(horizon,"Reserva de personaje",290,260,92,148,"panel");stroke(player);const pt=text(player,"Personaje","note",88,"secondary");pt.x=4;pt.y=58;}
return horizon;}
function bodyPanel(s,name,x,y,w){const p=positioned(s,name,x,y,w,"VERTICAL",16,"screen",24);stroke(p);return p;}
function commonBack(s,to="J01",label="Volver"){const r=positioned(s,"Regreso",64,s.height-78,250,"VERTICAL",8);button(r,label,to,250);}
function actionList(p,acts,w,focus=true){(acts||[]).forEach((a,i)=>button(p,a[0],a[1],w,focus&&i===0));}
function header(s,title){const a=positioned(s,"Encabezado",64,36,1152);text(a,title,"title",1152);}
const titleNav=[["Accesibilidad","I02"],["Audio","I05"],["Gráficos","I06"],["Controles","I07"],["Créditos","I08"]];
const gameNav=[["Accesibilidad","A02"],["Jugabilidad","A04"],["Audio","A05"],["Gráficos","A06"],["Controles","A07"],["Voz","A08"],["Manos y cámara","A09"]];
function settings(s,d){
header(s,d.title);const nav=positioned(s,"Secciones",64,130,260,"VERTICAL",8);
const entries=d.origin==="title"?titleNav:gameNav;for(const a of entries)button(nav,a[0],a[1],260,a[0]===d.tab);
const vp=box(s,"Contenido desplazable",368,116,848,496,"screen");vp.clipsContent=true;vp.overflowDirection="VERTICAL";
const col=auto(vp,"Contenido de "+d.tab,808,"VERTICAL",8,"screen",16);col.x=0;col.y=0;
text(col,d.tab,"title",770);
if(d.body)text(col,d.body,"body",770);
for(const row of d.rows||[]){const n=instance(col,row[2]||"Selector",row[0],770,row[1]);if(d.large){for(const t of n.findAllWithCriteria({types:["TEXT"]})){t.fontSize=27;t.textAutoResize="HEIGHT";}n.resize(770,Math.max(...n.findAllWithCriteria({types:["TEXT"]}).map(t=>t.height))+24);}}
actionList(col,d.actions,770,false);
if(d.more)button(col,"Más opciones ↓",d.more,770);
const scroll=box(s,"Carril de desplazamiento",1204,126,6,470,"world");box(scroll,"Posición de scroll",0,d.id==="A01"||d.id==="A03"?290:0,6,150,"focus");
commonBack(s,d.origin==="title"?"I01":"J01",d.origin==="title"?"Volver al menú":"Volver");
}
function draw(s,d){
if(d.kind==="home"){
fill(s,"world");const left=box(s,"Arte de fondo · reserva lateral",40,70,245,570,"panel");stroke(left);const t=text(left,"PERSONAJE\nY TELA\n\nArte existente","label",200,"secondary");t.x=24;t.y=225;
const emblem=positioned(s,"Título · reserva del emblema",330,64,620,"VERTICAL",8,"panel",24);text(emblem,"EL ASEDIO\nDE BACATÁ","display",572).textAlignHorizontal="CENTER";
const menu=positioned(s,"Acciones principales",(1280-(d.large?560:430))/2,330,d.large?560:430,"VERTICAL",24);
for(const [i,a] of d.actions.entries()){const b=button(menu,a[0],a[1],d.large?560:430,i===0);if(d.large){const t=b.findAllWithCriteria({types:["TEXT"]})[0];t.fontSize=27;t.textAutoResize="HEIGHT";b.resize(560,76);}}
}else if(d.kind==="settings") settings(s,d);
else if(d.kind==="world"){
backdrop(s);if(d.zone){const a=bodyPanel(s,"Nombre de zona",410,38,460);text(a,d.zone,"title",412).textAlignHorizontal="CENTER";}
if(d.health){const a=bodyPanel(s,"Vida contextual",48,32,245);text(a,"Vida · "+d.health,"label",197);progress(a,197,.6);}
if(d.camera){const a=positioned(s,"Cámara activa",855,40,360);status(a,"Cámara activa · C para pausar",360);}
if(d.notice){const a=bodyPanel(s,"Objetivo temporal",64,136,435);text(a,d.notice,"body",387);}
if(d.tutorial){const a=bodyPanel(s,"Consejo contextual",380,492,540);text(a,d.tutorial,"body",492);}
if(d.prompt){const a=positioned(s,"Interacción contextual",360,s.height-152,560);if(d.target)button(a,d.prompt,d.target,560,true);else status(a,d.prompt,560);}
if(d.caption){const a=bodyPanel(s,"Subtítulo seguro",330,s.height-76,620);text(a,d.caption,"note",572).textAlignHorizontal="CENTER";}
}else if(d.kind==="inspect"){
backdrop(s,true);const model=box(s,"Reserva del objeto 3D",165,190,410,335,"panel");stroke(model);const tx=text(model,"OBJETO 3D EXISTENTE\n\nGiro / inclinación / zoom","body",330,"secondary");tx.x=40;tx.y=116;
const a=bodyPanel(s,"Panel de inspección",818,140,398);text(a,d.title,"title",350);text(a,d.body,"body",350);if(d.camera)text(a,"Cámara activa","note",350);progress(a,350,d.progress);text(a,d.progressText,"label",350);
actionList(a,d.actions,350,false);button(a,d.complete?"Volver · E":"Cerrar · Esc","E03",350,true);
}else if(d.kind==="combat"){
backdrop(s);const enemy=box(s,"Reserva del guardián",590,225,120,205,"panel");stroke(enemy);const et=text(enemy,"Guardián","note",100,"secondary");et.x=10;et.y=85;
const hp=bodyPanel(s,"Vida del jugador",48,36,225);text(hp,"Vida · 100 / 100","label",177);progress(hp,177,1);
const top=bodyPanel(s,"Turno y resistencia",390,32,470);text(top,d.title,"title",422).textAlignHorizontal="CENTER";text(top,d.enemy,"body",422).textAlignHorizontal="CENTER";
const a=bodyPanel(s,"Comandos por fase",824,223,392);text(a,d.body,"body",344);
if(d.seconds){text(a,d.seconds,"title",344);progress(a,344,.6);}actionList(a,d.actions,344);
if(d.voice)text(a,d.voice,"note",344);
}else if(d.kind==="pause"){
backdrop(s);const a=bodyPanel(s,"Menú de pausa",96,80,450);text(a,"Pausa","display",402);actionList(a,d.actions,402);
}else if(d.kind==="journal"){
header(s,"Diario");const nav=positioned(s,"Secciones del diario",64,130,260,"VERTICAL",8);for(const a of [["Mapa","J02"],["Objetivos","J04"],["Archivo","J06"]])button(nav,a[0],a[1],260,a[0]===d.tab);
const a=bodyPanel(s,"Contenido del diario",368,116,848);text(a,d.tab,"title",800);
if(d.map){const row=auto(a,"Mapa y rutas conocidas",800,"HORIZONTAL",24);const map=auto(row,"Trazado descubierto",430,"VERTICAL",8,"world",16);
const svg=keep(figma.createNodeFromSvg('<svg width="390" height="220" viewBox="0 0 390 220" xmlns="http://www.w3.org/2000/svg"><path d="M55 170 L130 170 L130 110 L225 110 L225 45 L315 45 M130 110 L80 50" fill="none" stroke="#595959" stroke-width="6"/><circle cx="55" cy="170" r="12" fill="#252525"/><rect x="68" y="38" width="24" height="24" fill="#FFFFFF" stroke="#595959" stroke-width="3"/><rect x="303" y="33" width="24" height="24" fill="#FFFFFF" stroke="#595959" stroke-width="3"/></svg>'));svg.name="Ejemplo de ruta descubierta";map.appendChild(svg);text(map,"● Tú   □ Punto conocido\nSin revelar zonas ocultas","note",390);
text(row,d.body,"body",340);
}else text(a,d.body,"body",800);
actionList(a,d.actions,800);commonBack(s);
}else if(d.kind==="device"){
header(s,d.title);const a=bodyPanel(s,"Prueba de dispositivo",64,114,680);text(a,d.body,"body",632);status(a,d.status,632);if(d.progress!==undefined)progress(a,632,d.progress);actionList(a,d.actions,632);
if(d.preview){const p=bodyPanel(s,"Vista previa opcional",812,164,404);const b=box(p,"Reserva de cámara",0,0,356,230,"world");const t=text(b,"VISTA PREVIA\nSOLO EN CALIBRACIÓN","label",300,"secondary");t.x=28;t.y=76;text(p,"Mostrar vista previa · Opcional\nIlumina tus manos y deja espacio.","note",356);}
commonBack(s,d.preview?"A09":"A08");
}else if(d.kind==="states"){
header(s,d.title);const row=positioned(s,"Estados con texto",64,122,1152,"HORIZONTAL",32);const c1=auto(row,"Estados 1",550);const c2=auto(row,"Estados 2",550);d.states.forEach((a,i)=>status(i<Math.ceil(d.states.length/2)?c1:c2,a,550));commonBack(s,d.id==="B10"?"B08":d.id==="D10"?"D05":"E02");
}else if(d.kind==="loading"){
const a=positioned(s,"Carga",280,220,720,"VERTICAL",24);text(a,d.title,"display",720);text(a,d.body,"body",720);progress(a,720,d.progress);
}else if(d.kind==="dialogue"){
backdrop(s);const a=bodyPanel(s,"Diálogo y avance",180,406,920);text(a,"[Nombre del hablante]","label",872);text(a,d.body,"body",872);const row=auto(a,"Acciones del diálogo",872,"HORIZONTAL",24);actionList(row,d.actions,420);
}else if(d.kind==="modal"){
fill(s,"world");const a=bodyPanel(s,"Confirmación",290,174,700);text(a,d.title,"title",652);if(d.body)text(a,d.body,"body",652);actionList(a,d.actions,652);
}else{
if(d.kind!=="terminal")backdrop(s);const a=bodyPanel(s,"Lectura y acciones",96,76,760);text(a,d.title,"title",712);if(d.body)text(a,d.body,"body",712);actionList(a,d.actions,712);
}
}
function composeGroup(group,y){
const section=keep(figma.createSection());section.name=group.title;section.x=80;section.y=y;section.fills=[paint("panel")];section.resizeWithoutConstraints(4224,160+Math.ceil(group.screens.length/3)*860);
const head=positioned(section,"Guía de sección",48,32,4064,"VERTICAL",8);text(head,group.title,"display",4064);text(head,group.subtitle,"body",4064);
const ids=[];
for(const [i,d] of group.screens.entries()){const x=48+(i%3)*1376,sy=160+Math.floor(i/3)*860;
const s=keep(figma.createFrame());section.appendChild(s);s.name=d.id+" · "+d.title;s.resize(1280,d.format==="1610"?800:720);s.x=x;s.y=sy;s.clipsContent=true;fill(s,"screen");frames[d.id]=s.id;ids.push(s.id);draw(s,d);
const note=positioned(section,d.id+" · Notas",x,sy+s.height+12,1280,"VERTICAL",8);text(note,d.id+"  |  "+d.note,"note",1272,"secondary");
}
return {id:section.id,name:section.name,screenIds:ids,bounds:{x:section.x,y:section.y,width:section.width,height:section.height}};
}
