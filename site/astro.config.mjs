import { defineConfig } from 'astro/config';
import mdx from '@astrojs/mdx';
import tailwindcss from '@tailwindcss/vite';

export default defineConfig({
  output: 'static',
  trailingSlash: 'never',
  integrations: [
    mdx(),
  ],
  vite: {
    plugins: [tailwindcss()],
  },
  build: {
    assets: '_assets',
  },
});
