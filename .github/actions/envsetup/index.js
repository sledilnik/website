const core = require('@actions/core');
const github = require('@actions/github');
import { toUpper, snakeCase, forOwn } from 'lodash';

const context = github.context;

function debug() {
    core.info(`Environment: ${JSON.stringify(process.env)}`)
    core.info(`Context: ${JSON.stringify(context)}`)
}

function getTag() {
    const ref = context.ref
    if (!ref)
        throw "GITHUB_REF is not defined"
    if (!ref.startsWith("refs/tags/"))
        throw `Not a tag ref (${ref})`
    return ref.replace(/^refs\/tags\//, "")
}

function prNumber() {
    return context.payload.pull_request.number
}

function pullRequestConfig() {
    core.info(`PR Labels ${context.event.pull_request.labels}`)

    const shouldDeploy = context.event.action != 'closed'

    return {
        ImageTag: `pr-${prNumber()}`,
        ShoudlDeploy: ShoudlDeploy
    }
}

function stageConfig() {
    return {
        ImageTag: "latest",
        ShoudlDeploy: true
    }
}

function prodConfig() {
    return {
        ImageTag: `pr-${getTag()}`,
        ShoudlDeploy: true
    }
}

function setup(config) {
    forOwn(config, (value, key) => {
        const varName = toUpper(snakeCase(key))
        core.info(`Exporting ${varName}=${value}`)
        core.exportVariable(varName, value)
        core.setOutput(key, value)
    });
}

function main() {
    debug()
    const event = process.env['GITHUB_EVENT_NAME']
    core.info(`Configuring build environment for event ${event}`)

    if (event === 'pull_request') {
        setup(pullRequestConfig())
    } else if (event === 'push') {
        if (github.context.ref === 'refs/heads/master') {
            setup(stageConfig())
        } else if (github.context.ref.startsWith('refs/tags/')) {
            setup(prodConfig())
        } else {
            core.setFailed('Unknown GitHub event. Supported');
        }
    } else {
        core.setFailed('Unknown GitHub event. Supported: push, pull_request');
    }
}

main()