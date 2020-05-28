# covid19-web

[![Build](https://github.com/sledilnik/website/workflows/Build/badge.svg)](https://github.com/sledilnik/website/actions)
[![Translation status](https://hosted.weblate.org/widgets/sledilnik/-/website/svg-badge.svg)](https://hosted.weblate.org/engage/sledilnik/?utm_source=widget)

## Structure

| folder           | content                                                                                                                                                       |
|------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `src/assets`     | Static content (images, media files)                                                                                                                          |
| `src/components` | Reusable page components                                                                                                                                      |
| `src/content`    | Markdown content for static pages                                                                                                                             |
| `src/locales`    | Translation resources                                                                                                                                         |
| `src/pages`      | Actual pages of website (each page is one component, StaticPage is reused with different content). Pages are rendered inside App component (in `router-view`) |
| `src/App.vue`    | Main app component (renders `router-view`)                                                                                                                    |
| `src/main.js`    | Webpack entrypoint                                                                                                                                            |
| `.env`           | Configurable values (Page name and description)                                                                                                               |

## Deployment

https://travis-ci.com/github/sledilnik/website

Every push to `master` is automatically deployed (if build successfull) to https://covid-19.sledilnik.org

## Preview deployment

Create PR from a branch (in this repo, not fork) to master. Label PR with label `deploy-preview` and wait few minutes. Deployment should be available at https://pr-NUM.sledilnik.org where NUM is number of your PR.

Only open PR with label `deploy-preview` are deployed. When PR is closed or label removed, deployment is stopped.

## Development

## What you need

* yarn
* node
* .net core https://dotnet.microsoft.com/download
* probably something I forgot

## Project setup
```
yarn install
```

### Compiles and hot-reloads for development
```
yarn run serve
```

### Compiles and minifies for production
```
yarn run build
```

### Run your tests

Not really that we have any tests

```
yarn run test
```

### Lints and fixes files
```
yarn run lint
```

## Resources

* vue (framework) https://vuejs.org/v2/guide/
* vue-router (view routing) https://router.vuejs.org/
* vue-bootstrap (styled components) https://bootstrap-vue.js.org/
* webpack (bundler) https://webpack.js.org/concepts/
