# covid19-web


## Structure

| folder | content |
| ------ | ------- |
| `src/assets` | Static content (images, media files) |
| `src/components` | Reusable page components |
| `src/content` | Markdown content for static pages |
| `src/pages` | Actual pages of website (each page is one component, StaticPage is reused with different content) |
| `src/App.vue` | Main app component (renders `router-view`) |
| `src/main.js` | Webpack entrypoint |

## Development

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

* https://vuejs.org/v2/guide/
* https://router.vuejs.org/
* https://webpack.js.org/concepts/