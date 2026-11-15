// Local preview of the production build, including compressed responses and SPA routes.
const http = require('node:http');
const fs = require('node:fs/promises');
const path = require('node:path');
const zlib = require('node:zlib');
const root = path.resolve(__dirname, '../UI/travel-agency/build');
const types = { '.html':'text/html', '.js':'application/javascript', '.css':'text/css', '.json':'application/json', '.svg':'image/svg+xml', '.png':'image/png', '.ico':'image/x-icon' };
http.createServer(async (req,res) => {
  try {
    const pathname = decodeURIComponent(new URL(req.url,'http://localhost').pathname);
    let file = path.resolve(root, '.'+pathname);
    if (!file.startsWith(root+path.sep) && file!==root) { res.writeHead(403).end(); return; }
    try { if (!(await fs.stat(file)).isFile()) file=path.join(root,'index.html'); }
    catch { file=path.join(root,'index.html'); }
    let body=await fs.readFile(file);
    res.setHeader('Content-Type', types[path.extname(file)]||'application/octet-stream');
    res.setHeader('Cache-Control', file.includes(path.sep+'static'+path.sep)?'public, max-age=31536000, immutable':'no-cache');
    res.setHeader('Vary','Accept-Encoding');
    if (/gzip/.test(req.headers['accept-encoding']||'') && /\.(js|css|html|json)$/.test(file)) {
      body=zlib.gzipSync(body); res.setHeader('Content-Encoding','gzip');
    }
    res.end(body);
  } catch { res.writeHead(500).end('Preview could not read the production build.'); }
}).listen(Number(process.env.PORT||3100),'127.0.0.1',()=>console.log('Production preview: http://localhost:'+(process.env.PORT||3100)));
