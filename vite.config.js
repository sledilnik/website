import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue2'

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [vue()],
    // base: './src',
    // build: {
    //     outDir: "../dist",
    // },
    resolve: {
        alias: [
            {
                find: '@',
                replacement: 'src/'
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
