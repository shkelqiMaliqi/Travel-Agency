const {chromium} = require('../UI/travel-agency/node_modules/@playwright/test');
const fs = require('node:fs/promises');
const zlib = require('node:zlib');
const path = require('node:path');
(async()=>{
  const browser = await chromium.launch();
  const results=[];
  for(const route of ['/','/packages','/loginpage']){
    const context=await browser.newContext();
    const page=await context.newPage();
    await page.goto((process.env.PERF_URL||'http://127.0.0.1:3100')+route,{waitUntil:'domcontentloaded'});
    await page.locator('main h1, main h2').first().waitFor();
    await page.waitForTimeout(1000);
    results.push(await page.evaluate(route=>({route,
      domContentLoadedMs:Math.round(performance.getEntriesByType('navigation')[0].domContentLoadedEventEnd),
      firstContentfulPaintMs:Math.round(performance.getEntriesByName('first-contentful-paint')[0]?.startTime||0),
      scripts:performance.getEntriesByType('resource').filter(r=>r.initiatorType==='script').map(r=>({url:new URL(r.name).pathname,transferBytes:r.transferSize})),
      linkResources:performance.getEntriesByType('resource').filter(r=>r.initiatorType==='link').map(r=>r.name)
    }),route));
    await context.close();
  }
  const dir=path.resolve(__dirname,'../UI/travel-agency/build/static/js');
  const main=(await fs.readdir(dir)).find(n=>/^main\..*\.js$/.test(n));
  const content=await fs.readFile(path.join(dir,main));
  const report={measuredAt:new Date().toISOString(),mainBundle:{file:main,bytes:content.length,gzipBytes:zlib.gzipSync(content).length},pages:results};
  console.log(JSON.stringify(report,null,2));
  if(process.argv[2])await fs.writeFile(process.argv[2],JSON.stringify(report,null,2)+'\n');
  await browser.close();
})();
