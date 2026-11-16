import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig(({ mode }) => {
  const env=loadEnv(mode, process.cwd(), ['VITE_', 'REACT_APP_']);
  return {
    plugins:[react()],
    define:{
      'process.env.REACT_APP_API_URL':JSON.stringify(env.VITE_API_URL || env.REACT_APP_API_URL || 'http://localhost:5132/api/v1'),
      'process.env.PUBLIC_URL':JSON.stringify('')
    },
    server:{host:'127.0.0.1',port:3000,strictPort:true,watch:{ignored:['**/node_modules.cra-backup/**','**/tmp/**']}},
    optimizeDeps:{entries:['index.html']},
    build:{
      outDir:'build',
      rolldownOptions:{output:{
        entryFileNames:'static/js/main.[hash].js',
        chunkFileNames:'static/js/[name].[hash].js',
        assetFileNames:'static/media/[name].[hash][extname]'
      }}
    },
    test:{
      globals:true,environment:'jsdom',setupFiles:['./src/setupTests.js'],
      include:['src/**/*.test.{js,jsx}'],
      css:false
    }
  };
});
