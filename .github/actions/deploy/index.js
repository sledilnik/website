const github = require('@actions/github')
const core = require('@actions/core')
const { execSync, spawnSync, execFileSync, execFile } = require('child_process');
const { version } = require('os');

const ghToken = process.env['INPUT_TOKEN']
const context = github.context;
const gh = github.getOctokit(ghToken)

// core.info(JSON.stringify(context.payload, null, 1))

async function createDeployment() {
    const payload = {
        owner: context.payload.repository.owner.login,
        repo: context.payload.repository.name,
        ref: context.ref,
        environment: process.env['INPUT_DEPLOYENV'],
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
    return await gh.repos.createDeploymentStatus(payload)
}

async function helm(args) {
    try {
        core.info(`running: helm ${args.join(' ')}`)
        execFileSync("helm", args, { 'stdio': [0, 1, 2] })
    } catch (ex) {
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
        core.setFailed(`Failed to create deployment: ${ex}`)
    }

    const opts = {
        namespace,
        releaseName,
        chartName,
        chartVersion
    }
    core.info(`Helm deploy opts ${JSON.stringify(opts)}`)

    try {
        setDeploymentState(deployment.data.id, "pending")
        helm(['upgrade', releaseName, chartName, '--install', '--atomic', '--namespace', namespace, '--version', chartVersion])
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
