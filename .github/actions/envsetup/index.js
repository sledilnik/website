const core = require('@actions/core');
const github = require('@actions/github');
import { toUpper, snakeCase, forOwn, map } from 'lodash';

const context = github.context;

const gh = github.getOctokit(core.getInput('token'))

function debug() {
    core.info(`Environment: ${JSON.stringify(process.env, null, 1)}`)
    core.info(`Context: ${JSON.stringify(context, null, 1)}`)
}

async function abort() {
    try {
        debug()
        const params = {
            owner: context.repo.owner.name,
            repo: context.repo.name,
            run_id: process.env.GITHUB_RUN_ID,
        }
        core.info(`Aborting current workflow: ${JSON.stringify(params)}`)
        await gh.actions.cancelWorkflowRun(params)
    } catch (ex) {
        core.setFailed(`Failed to abort workflow: ${ex}`)
    }
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

function hasLabel(label) {
    return map(context.payload.pull_request.labels, (label) => label.name).includes(label)
}

function pullRequestConfig() {

    const deployLabel = core.getInput('prDeployLabel')

    const shouldDeploy = context.payload.action != 'closed' && hasLabel(deployLabel)

    if (!shouldDeploy) {
        abort()
    }

    return {
        ImageTag: `pr-${prNumber()}`,
    }
}

function stageConfig() {
    return {
        ImageTag: "latest",
    }
}

function prodConfig() {
    return {
        ImageTag: `pr-${getTag()}`,
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
    // debug()
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