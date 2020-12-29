const PrerenderSPAPlugin = require('prerender-spa-plugin')
const Renderer = PrerenderSPAPlugin.PuppeteerRenderer
const axios = require('axios').default;
const path = require('path');

const cartesian =
    (...a) => a.reduce((a, b) => a.flatMap(d => b.map(e => [d, e].flat())));

async function generatePrerenderRoutes() {
    const langs = ['en', 'sl', 'hr', 'de', 'it']
    const paths = ['stats', 'world', 'restrictions', 'about', 'faq', 'posts', 'data', 'ostanizdrav', 'embed']
    const basicRoutes = cartesian(langs, paths).map(pair => `/${pair.join('/')}`)

    const { data } = await axios.get('https://backend.sledilnik.org/api/v1/posts')
    const postRoutes = cartesian(langs, data.objects.filter(post => post.link_to == null).map(post => `posts/${post.id}`)).map(pair => `/${pair.join('/')}`)

    return basicRoutes.concat(postRoutes)
}

module.exports = (api, options) => {
    api.registerCommand('build:prerender', async (args) => {
        //   const PrerenderSPAPlugin = require('prerender-spa-plugin')
        const prerenderRoutes = await generatePrerenderRoutes()
        console.log(`Prerendering ${prerenderRoutes.length} URLs ${prerenderRoutes}`)

        api.chainWebpack(config => {
            config.plugin('prerender').use(PrerenderSPAPlugin, [{
                staticDir: path.join(__dirname, 'dist'),
                routes: prerenderRoutes,

                maxConcurrentRoutes: 8,

                postProcess(renderedRoute) {
                    // console.log('renderedRoute', JSON.stringify(renderedRoute))
                    return renderedRoute
                },

                renderer: new Renderer({
                    inject: {
                        foo: 'bar'
                    },
                    headless: true,
                    renderAfterDocumentEvent: 'render-event'
                })
            }])
        })

        await api.service.run('build', args)
    })
}

module.exports.defaultModes = {
    'build:prerender': 'production'
}
