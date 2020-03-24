const path = require('path');

const paths = {
  src: path.resolve(path.join(__dirname, 'src')),
  dist: path.resolve(path.join(__dirname, 'dist')),
}

module.exports = {
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
