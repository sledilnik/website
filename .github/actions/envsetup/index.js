const core = require('@actions/core');
const github = require('@actions/github');
import { toHtml } from '@fortawesome/fontawesome-svg-core';
import { toUpper, snakeCase, forOwn, map } from 'lodash';

const context = github.context;

const gh = github.getOctokit(core.getInput('token'))

/**
 * Print env variables and event context to help debugging
 */
function debug() {
    core.info(`Environment: ${JSON.stringify(process.env, null, 1)}`)
    core.info(`Context: ${JSON.stringify(context, null, 1)}`)
}

/**
 * Aborts current workflow (in case it does not need to be finsihed)
 */
async function abort(reason) {
    try {
        const params = {
            owner: context.payload.repository.owner.login,
            repo: context.payload.repository.name,
            run_id: context.runId
        }
        core.warning(`Aborting current workflow: ${reason}`)
        await gh.actions.cancelWorkflowRun(params)
    } catch (ex) {
        debug()
        core.setFailed(`Failed to abort workflow: ${ex}`)
    }
}

/**
 * Get tag of current build.
 * 
 * @throws Error if current build is not tagged
 */
function getTag() {
    const ref = context.ref
    if (!ref)
        throw new Error("GITHUB_REF is not defined")
    if (!ref.startsWith("refs/tags/"))
        throw new Error(`Not a tag ref (${ref})`)
    return ref.replace(/^refs\/tags\//, "")
}

/**
 * Get current build's PR number
 */
function prNumber() {
    if (!context.payload.pull_request) {
        throw new Error('Not a pull request!')
    }
    return context.payload.pull_request.number
}

/**
 * Check if current build's PR has given label
 * 
 * @param String label 
 */
function hasLabel(label) {
    return map(context.payload.pull_request.labels, (label) => label.name).includes(label)
}

function pullRequestConfig() {

    const deployLabel = core.getInput('prDeployLabel')

    if (!hasLabel(deployLabel)) {
        abort(`PR does not have label '${deployLabel}'`)
    }

    return {
        ImageTag: `pr-${prNumber()}`,
        ReleaseName: `website-pr-${prNumber()}`,
        DeployEnv: `pr-${prNumber()}`,
        DeployNamespace: 'sledilnik-pr',
        IngressRule: `Host(\`pr-${prNumber()}.sledilnik.org\`)`
    }
}

function stageConfig() {
    return {
        ImageTag: 'latest',
        ReleaseName: 'website-stage',
        DeployEnv: 'stagging',
        DeployNamespace: 'sledilnik-stage',
        IngressRule: `Host(\`stage.sledilnik.org\`)`
    }
}

function prodConfig() {
    return {
        ImageTag: `pr-${getTag()}`,
        ReleaseName: 'website-prod',
        DeployEnv: 'production',
        DeployNamespace: 'sledilnik-prod',
        IngressRule: `Host(\`www.sledilnik.org\`) || Host(\`sledilnik.org\`) || Host(\`covid-19.sledilnik.org\`) || Host(\`www.covid-19.sledilnik.org\`)`
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
    try {
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
    } catch (ex) {
        debug()
        core.setFailed(`Error: ${ex.message}`);
    }
}

main()