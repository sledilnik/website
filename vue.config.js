const path = require('path');
const PrerenderSPAPlugin = require('prerender-spa-plugin')
const Renderer = PrerenderSPAPlugin.PuppeteerRenderer

const paths = {
  src: path.resolve(path.join(__dirname, 'src')),
  dist: path.resolve(path.join(__dirname, 'dist')),
}

if (process.env.NODE_ENV != 'production') {
  process.env.VUE_APP_TITLE = process.env.VUE_APP_TITLE + ' (preview)'
  process.env.VUE_APP_DESC = process.env.VUE_APP_DESC + ' (preview)'
}

indexTemplate = process.env.CADDY_BUILD == '1' ? 'index_caddy.html' : 'index.html'

const cartesian =
  (...a) => a.reduce((a, b) => a.flatMap(d => b.map(e => [d, e].flat())));

function generatePrerenderRoutes() {
  const langs = ['sl', 'en']
  const paths = ['stats', 'world', 'restrictions', 'about', 'faq']
  return cartesian(langs, paths).map(pair => pair.join('/'))
}

console.log("Using template", indexTemplate)

module.exports = {
  productionSourceMap: process.env.NODE_ENV != 'production',
  publicPath: process.env.C19_PUBLIC_PATH || '/',
  outputDir: process.env.C19_OUTPUT_DIR || 'dist',
  filenameHashing: true,
  devServer: {
    disableHostCheck: true,
  },
  pages: {
    index: {
      entry: ['src/index.js'],
      template: indexTemplate,
      filename: 'index.html',
    },
    embed: {
      entry: ['src/embed.js'],
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
  pluginOptions: {
    webpackBundleAnalyzer: {
      analyzerMode: process.env.DISPLAY ? 'server' : 'disabled',
      openAnalyzer: process.env.DISPLAY ? true : false,
    },
  },
  configureWebpack: {
    plugins: [
      new PrerenderSPAPlugin({
        staticDir: path.join(__dirname, 'dist'),
        // routes: generatePrerenderRoutes(),
        routes: ['/sl/stats'],
  
        renderer: new Renderer({
          inject: {
            foo: 'bar'
          },
          headless: true,
          // renderAfterDocumentEvent: 'render-event'
        })
      })
    ]
  },
  chainWebpack: config => {

    // config.output.filename('[name].[hash:8].js')
    // config.output.chunkFilename('[name].[hash:8].js')

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
