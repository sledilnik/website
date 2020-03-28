# covid19-web

[![Build Status](https://travis-ci.org/slo-covid-19/website.svg?branch=master)](https://travis-ci.org/slo-covid-19/website)

## Structure

| folder | content |
| ------ | ------- |
| `src/assets` | Static content (images, media files) |
| `src/components` | Reusable page components |
| `src/content` | Markdown content for static pages |
| `src/pages` | Actual pages of website (each page is one component, StaticPage is reused with different content). Pages are rendered inside App component (in `router-view`) |
| `src/App.vue` | Main app component (renders `router-view`) |
| `src/main.js` | Webpack entrypoint |
| `.env` | Configurable values (Page name and description) |

## Deployment

https://travis-ci.org/github/slo-covid-19/website

Every push to `master` is automatically deployed (if build successfull) to https://covid-19.sledilnik.org

Every push to other branches (not `master`) is deployed to https://preview.sledilnik.org/branch-name

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