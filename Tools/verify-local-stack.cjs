// End-to-end smoke checks against docker-compose.local.yml. Uses only its local test inbox.
const assert = require('node:assert/strict');
const fs = require('node:fs/promises');
require('node:dns').setDefaultResultOrder('ipv4first');
const { setTimeout: delay } = require('node:timers/promises');
const api = process.env.STACK_API_URL || 'http://localhost:8088';
const inbox = process.env.STACK_MAIL_URL || 'http://localhost:8025';
const evidence = { checkedAt: new Date().toISOString(), checks: [] };
async function check(name, fn) { const details = await fn(); evidence.checks.push({name,passed:true,...details}); console.log('PASS '+name); }
async function request(path, method='GET', body, token) {
  const response = await fetch(api+path,{method, headers:{...(body?{'Content-Type':'application/json'}:{}),...(token?{Authorization:'Bearer '+token}:{})},body:body?JSON.stringify(body):undefined});
  const json = await response.json();
  assert.ok(response.ok, `${method} ${path} returned ${response.status}: ${json.message}`);
  return {data:json.data,response};
}
async function emailFor(address, subject, previousIds) {
  for(let attempt=0;attempt<20;attempt++) {
    const list=await fetch(inbox+'/api/v1/messages').then(r=>r.json());
    const match=list.messages.find(m=>!previousIds.has(m.ID)&&m.To.some(to=>to.Address===address)&&subject.test(m.Subject));
    if(match) return fetch(inbox+'/api/v1/message/'+match.ID).then(r=>r.json());
    await delay(1000);
  }
  throw Error('Expected message was not delivered to the local inbox.');
}
(async()=>{
  let adminToken;
  await check('two API instances receive gateway traffic',async()=>{
    const upstreams=new Set();
    for(let i=0;i<8;i++){
      const r=await fetch(api+'/api/v1/packages');assert.equal(r.status,200);
      upstreams.add(r.headers.get('x-upstream'));await delay(120);
    }
    assert.equal(upstreams.size,2);assert.ok(!upstreams.has(null));
    return {upstreams:[...upstreams]};
  });
  await check('RabbitMQ notification worker delivers admin MFA to the local inbox',async()=>{
    const existing=await fetch(inbox+'/api/v1/messages').then(r=>r.json());
    const previous=new Set(existing.messages.map(m=>m.ID));
    const {data:challenge}=await request('/api/v1/auth/login','POST',{userNameOrEmail:'admin@travelagency.com',password:'Admin123!'});
    assert.equal(challenge.requiresMfa,true);assert.ok(!challenge.mfaCode);
    const mail=await emailFor('admin@travelagency.com',/MFA/,previous);
    const code=mail.Text.match(/\b\d{6}\b/)?.[0];assert.ok(code);
    const {data:session}=await request('/api/v1/auth/verify-mfa','POST',{email:'admin@travelagency.com',code});
    assert.ok(session.token);adminToken=session.token;
    return {delivery:'RabbitMQ -> notification service -> Mailpit',apiExposesCode:false};
  });
  await check('catalog writes invalidate caches across both API instances',async()=>{
    const name='StackSmoke'+Date.now();
    const path='/api/v1/places?search='+name;
    assert.equal((await request(path)).data.length,0);
    const {data:created,response}=await request('/api/v1/places','POST',{place_Name:name,place_Description:'Local infrastructure smoke test'},adminToken);
    assert.equal(response.status,201);assert.ok(response.headers.get('location'));
    try {
      for(let i=0;i<2;i++)assert.equal((await request(path)).data.length,1);
      await request('/api/v1/places/'+created.id,'PUT',{place_Name:name+'Updated',place_Description:'Updated'},adminToken);
      for(let i=0;i<2;i++)assert.equal((await request('/api/v1/places/'+created.id)).data.place_Name,name+'Updated');
    } finally { await request('/api/v1/places/'+created.id,'DELETE',undefined,adminToken); }
    for(let i=0;i<2;i++)assert.equal((await request(path)).data.length,0);
    return {filteredLists:true,details:true};
  });
  await check('password recovery uses email and a single-use token',async()=>{
    const username='smoke'+Date.now(),email=username+'@example.test';
    const {data:user}=await request('/api/v1/auth/register','POST',{u_Name:'Smoke',u_Surname:'Test',u_Email:email,u_Username:username,password:'LocalSmoke123!',confirmPassword:'LocalSmoke123!'});
    try {
      const existing=await fetch(inbox+'/api/v1/messages').then(r=>r.json());
      const {data:known}=await request('/api/v1/auth/forgot-password','POST',{email});
      const {data:unknown}=await request('/api/v1/auth/forgot-password','POST',{email:'absent-'+Date.now()+'@example.test'});
      assert.deepEqual(known,unknown);assert.ok(!known.resetCode);
      const mail=await emailFor(email,/Reset your/,new Set(existing.messages.map(m=>m.ID)));
      const token=mail.Text.match(/\b[A-F0-9]{64}\b/)?.[0];assert.ok(token);
      const body={email,resetCode:token,newPassword:'ChangedSmoke123!',confirmPassword:'ChangedSmoke123!'};
      await request('/api/v1/auth/reset-password','POST',body);
      const replay=await fetch(api+'/api/v1/auth/reset-password',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(body)});
      assert.equal(replay.status,400);
      assert.ok((await request('/api/v1/auth/login','POST',{userNameOrEmail:email,password:'ChangedSmoke123!'})).data.token);
    } finally { await request('/api/v1/users/'+user.userId,'DELETE',undefined,adminToken); }
    return {genericResponse:true,singleUse:true};
  });
  await check('Prometheus scrapes both API replicas',async()=>{
    for(let attempt=0;attempt<12;attempt++){
      const body=await fetch('http://localhost:9091/api/v1/targets').then(r=>r.json());
      const targets=body.data.activeTargets.filter(t=>t.labels.job==='travel-agency-api');
      if(targets.length===2&&targets.every(t=>t.health==='up'))return {targets:targets.map(t=>({instance:t.labels.instance,health:t.health}))};
      await delay(1000);
    }
    throw Error('Both API scrape targets did not become healthy.');
  });
  await check('production frontend compresses scripts and caches versioned assets',async()=>{
    const root=await fetch('http://localhost:3101');assert.equal(root.status,200);
    const html=await root.text();assert.ok(!html.includes('cdn.jsdelivr.net'));
    const main=html.match(/src="([^"]*\/static\/js\/main[^\"]*\.js)"/)[1];
    const script=await fetch('http://localhost:3101'+main,{headers:{'Accept-Encoding':'gzip'}});
    assert.equal(script.headers.get('content-encoding'),'gzip');
    assert.match(script.headers.get('cache-control'),/immutable/);
    assert.equal((await fetch('http://localhost:3101/packages')).status,200);
    assert.equal((await fetch('http://localhost:3101/api/v1/packages')).status,200);
    return {compression:'gzip',immutableAssets:true,sameOriginApi:true};
  });
  await fs.writeFile('Documentation/infrastructure-verification.json',JSON.stringify(evidence,null,2)+'\n');
})().catch(e=>{console.error(e.message);process.exitCode=1;});
