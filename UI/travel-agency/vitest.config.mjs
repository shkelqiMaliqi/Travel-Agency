import { defineConfig, mergeConfig } from 'vitest/config';
import viteConfig from './vite.config.mjs';
export default defineConfig((env) => mergeConfig(viteConfig(env), {}));
