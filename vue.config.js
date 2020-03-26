const path = require('path');

const paths = {
  src: path.resolve(path.join(__dirname, 'src')),
  dist: path.resolve(path.join(__dirname, 'dist')),
}

process.env.VUE_APP_TITLE='COVID-19 Sledilnik'
process.env.VUE_APP_DESC='COVID-19 Sledilnik Slovenija | COVID-19 Slovenia Tracking'

module.exports = {
  devServer: {
    disableHostCheck: true, 
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
      .test( /\.fs(x|proj)?$/)
      .use('fable-loader')
        .loader('fable-loader')
        .end()

    config.resolve.modules.prepend(paths.src)
  }
}
