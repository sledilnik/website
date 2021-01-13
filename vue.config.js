const path = require('path');

const paths = {
  src: path.resolve(path.join(__dirname, 'src')),
  dist: path.resolve(path.join(__dirname, 'dist')),
}

if (process.env.NODE_ENV != 'production') {
  process.env.VUE_APP_TITLE = process.env.VUE_APP_TITLE + ' (preview)'
  process.env.VUE_APP_DESC = process.env.VUE_APP_DESC + ' (preview)'
}

module.exports = {
  productionSourceMap: process.env.NODE_ENV != 'production',
  publicPath: process.env.SLEDILNIK_PUBLIC_PATH || '/',
  outputDir: 'dist',
  filenameHashing: true,
  devServer: {
    disableHostCheck: true,
  },
  pages: {
    index: {
      entry: ['src/index.js'],
      template: 'index.html',
      filename: 'index.html',
    },
    embed: {
      entry: ['src/embed.js'],
      template: 'index.html',
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
  pluginOptions: {
    webpackBundleAnalyzer: {
      analyzerMode: process.env.SLEDILNIK_WBA || 'disabled',
      openAnalyzer: process.env.SLEDILNIK_WBA === 'server',
    },
  },
  chainWebpack: config => {

    config.output.filename('[name].[hash:8].js')
    config.output.chunkFilename('[name].[hash:8].js')

    if (config.plugins.has('prefetch-index')) {
      config.plugin('prefetch-index').tap(options => {
        options[0].fileBlacklist = options.fileBlacklist || [];
        options[0].fileBlacklist.push(/.*\.route\./);
        return options;
      });
    }
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
