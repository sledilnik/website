# COVID-19 Sledilnik website

[![Build](https://github.com/sledilnik/website/workflows/Build/badge.svg)](https://github.com/sledilnik/website/actions)
[![Translation status](https://hosted.weblate.org/widgets/sledilnik/-/website/svg-badge.svg)](https://hosted.weblate.org/engage/sledilnik/?utm_source=widget)


## License
This software is licensed under [GNU Affero General Public License](LICENSE).


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

Every push to `master` is automatically deployed (if build successful) to https://covid-19.sledilnik.org

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

### FSharpLint

[FSharpLint](https://github.com/fsprojects/FSharpLint) is used to check the
F# code. In order to use it, you need to run `build.bat` script, which will
1. install FSharpLint locally (if it was not already installed), 
1. run it on the source code,
1. run F# unit tests.

The configuration is stored in the `fsharplint-config.json` file. You can
find more information on how to configure or suppress various rules
[here](https://fsprojects.github.io/FSharpLint/how-tos/rule-configuration.html).

#### Using FSharpLint in Rider

If you use JetBrains Rider as your IDE, you can integrate FSharpLint into 
your workflow inside IDE by following these steps:

1. Make sure you have FSharpLint installed (see the above section).
1. Click `File` | `Settings` menu option.
1. In the Settings window, find `Tools`/`External Tools` tree item.
1. Press on the plus (`+`) button to add a new external tool.
1. In the `Edit Tool` window, enter the following values:
    - `Name`: `FSharpLint`
    - `Program`: `dotnet`
    - `Arguments`: `fsharplint --format msbuild lint -l fsharplint-config.json SloCovid19Website.sln`
    - `Advanced Options`: check all checkboxes
    - `Output filters`: `$FILE_PATH$\($LINE$\,$COLUMN$\,.*`
1. Press `OK` button to confirm.

Now you should have a new menu item under `Tools` | `External Tools` 
| `FSharpLint`.

## Resources

* vue (framework) https://vuejs.org/v2/guide/
* vue-router (view routing) https://router.vuejs.org/
* vue-bootstrap (styled components) https://bootstrap-vue.js.org/
* webpack (bundler) https://webpack.js.org/concepts/
