const github = require('@actions/github')
const core = require('@actions/core')
const { execSync, spawnSync, execFileSync, execFile } = require('child_process')

const ghToken = process.env['INPUT_TOKEN']
const context = github.context;
const gh = github.getOctokit(ghToken)

// core.info(JSON.stringify(context.payload, null, 1))

async function createDeployment() {
    const payload = {
        owner: context.payload.repository.owner.login,
        repo: context.payload.repository.name,
        ref: context.ref,
        environment: "testenv",
    }
    core.info(`Creating deployment: ${JSON.stringify(payload)}`)
    return await gh.repos.createDeployment({
        owner: context.payload.repository.owner.login,
        repo: context.payload.repository.name,
        ref: context.ref,
        environment: "testenv",
    })
}

async function deleteDeployment(id) {
    const payload = {
        id
    }
    core.info(`Deliting deployment: ${JSON.stringify(payload)}`)
    return await gh.repos.deleteDeployment(payload)
}

async function setDeploymentState(id, state) {
    const payload = {
        id,
        state
    }
    core.info(`Setting deployment state: ${JSON.stringify(payload)}`)
    return await gh.repos.setDeploymentState(payload)
}

async function helmDeploy(namespace, releaseName, chartName) {
    try {
        const args = ['upgrade', releaseName, chartName, `--namespace ${namespace}`, '--install', '--atomic']
        core.info(`running: helm ${args.join(' ')}`)
        execFileSync("helm", args, {'stdio': [0, 1, 2]})
    } catch (ex) {
        core.error(`Error running helm ${ex}`)
        core.setFailed(ex)
    }
}

async function helmUndeploy(releaseName) {
    try {
        execFileSync("helm", ['uninstall', releaseName], {'stdio': [0, 1, 2]})
    } catch (ex) {
        core.error(`Error running helm ${ex}`)
        core.setFailed(ex)
    }
}

async function deploy() {
    core.info("Starting deploy")

    const namespace = process.env['INPUT_NAMESPACE']
    const releaseName = process.env['INPUT_RELEASENAME']
    const chartName = process.env['INPUT_CHARTNAME']
    const chartVersion = process.env['INPUT_CHARTVERSION']

    let deployment = undefined;
    try {
        deployment = await createDeployment()
    } catch {
        core.error("Failed to create deployment")
        core.setFailed(ex)
    }

    try {
        setDeploymentState(deployment.data.id, "pending")
        helmDeploy(namespace, releaseName, chartName, chartVersion)
        setDeploymentState(deployment.data.id, "success")
    } catch (ex) {
        setDeploymentState(deployment.data.id, "failed")
        core.setFailed(ex)
    }
}

async function undeploy() {
    core.info("Starting undeploy")
}

function main() {
    try {
        var args = process.argv.slice(2);
        switch (args[0]) {
            case 'deploy':
                deploy()
                break;
            case 'undeploy':
                undeploy()
                break;
            default:
                core.setFailed("Unknown action")
        }
    } catch (ex) {
        core.setFailed(ex)
    }
}

main()
