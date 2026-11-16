// Accept the previous non-watching CI flags while keeping npm test portable.
const { spawnSync }=require('node:child_process');
const args=process.argv.slice(2).filter(a=>!['--watchAll=false','--runInBand'].includes(a));
const result=spawnSync(process.execPath,[require('node:path').join(require('node:path').dirname(require.resolve('vitest/package.json')), 'vitest.mjs'),'run',...args],{stdio:'inherit'});
process.exit(result.status ?? 1);
