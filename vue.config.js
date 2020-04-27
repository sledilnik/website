const path = require('path');

const paths = {
  src: path.resolve(path.join(__dirname, 'src')),
  dist: path.resolve(path.join(__dirname, 'dist')),
}

if (process.env.NODE_ENV != 'production') {
  process.env.VUE_APP_TITLE = process.env.VUE_APP_TITLE + ' (preview)'
  process.env.VUE_APP_DESC = process.env.VUE_APP_DESC + ' (preview)'
}

indexTemplate = process.env.CADDY_BUILD == '1' ? 'public/index_caddy.html' : 'public/index.html'

console.log("Using template", indexTemplate)

module.exports = {
  publicPath: process.env.C19_PUBLIC_PATH || '/',
  outputDir: process.env.C19_OUTPUT_DIR || 'dist',
  devServer: {
    disableHostCheck: true,
  },
  pages: {
    index: {
      entry: 'src/index.js',
      template: indexTemplate,
      filename: 'index.html',
    },
    embed: {
      entry: 'src/embed.js',
      template: indexTemplate,
      filename: 'embed.html',
    },
  },
  css: {
    loaderOptions: {
      scss: {
        prependData: '@import "@/../src/style/bootstrap.scss";'
      },
      sass: {
        prependData: '@import "@/../src/style/bootstrap.scss"'
      },
    }
  },
  chainWebpack: config => {
    // Markdown Loader
    config.module
      .rule('md')
      .test(/\.md$/)
      .use('html-loader')
      .loader('html-loader')
      .end()
      .use('markdown-loader')
      .loader('markdown-loader')
      .end()
    // Fable loader
    config.module
      .rule('fable')
      .test(/\.fs(x|proj)?$/)
      .use('fable-loader')
      .loader('fable-loader')
      .end()

    config.resolve.modules.prepend(paths.src)
  }
}
