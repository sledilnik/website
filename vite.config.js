import { defineConfig } from 'vite'
import { resolve } from 'path'
import vue from '@vitejs/plugin-vue2'

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [vue()],
    build: {
        rollupOptions: {
        //   external: new Regexp('src/visualizations/.*'),
          input: {
            main: resolve(__dirname, 'index.html'),
            // embed: resolve(__dirname, 'index_embed.html'),
          },
        },
      },
    resolve: {
        extensions: ['.mjs', '.js', '.mts', '.ts', '.jsx', '.tsx', '.json', '.vue'],
        alias: [
            {
                find: '@',
                replacement: resolve(__dirname, 'src')
            }
        ]
    },
    assetsInclude: ['**/*.md'],
    css: {
        preprocessorOptions: {
            scss: {
                additionalData: '@import "src/style/bootstrap.scss";'
            }
        }
    }
    // optimizeDeps: {
    //     entries: ['index.html']
    // }
})
