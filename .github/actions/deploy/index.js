const github = require('@actions/github')
const core = require('@actions/core')
const { execFileSync } = require('child_process');

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

async function deleteDeployment(deployment_id) {
    const payload = {
        deployment_id
    }
    core.info(`Deliting deployment: ${JSON.stringify(payload)}`)
    setDeploymentState(deployment.data.id, "pending")
    return await gh.repos.deleteDeployment(payload)
}

async function setDeploymentState(deployment_id, state) {
    const payload = {
        deployment_id,
        state
    }
    core.info(`Setting deployment state: ${JSON.stringify(payload)}`)
    return await gh.repos.createDeploymentStatus(payload)
}

async function helm(args) {
    try {
        core.info(`running: helm ${args.join(' ')}`)
        execFileSync("helm", args, { 'stdio': [0, 1, 1], env: process.env })
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
    const chartValues = process.env['INPUT_CHARTVALUES']

    let deployment = undefined;
    try {
        deployment = await createDeployment()
        setDeploymentState(deployment.data.id, "pending")
    } catch {
        core.setFailed(`Failed to create deployment: ${ex}`)
    }

    try {
        helm(['upgrade', releaseName, chartName, '--install', '--atomic', '--namespace', namespace, '--version', chartVersion, '-f', chartValues])
        setDeploymentState(deployment.data.id, "success")
    } catch (ex) {
        try {
            setDeploymentState(deployment.data.id, "failed")
        } catch (ex) {
            core.warning(`Failed to set deployment state to failed: ${ex}`)
        }
        core.setFailed(`Helm install failed: ${ex}`)
    }
}

async function undeploy() {
    const namespace = process.env['INPUT_NAMESPACE']
    const releaseName = process.env['INPUT_RELEASENAME']
    core.info("Starting undeploy")
    try {
        helm(['uninstall', releaseName,  '--namespace', namespace])
    } catch (ex) {
        core.setFailed(`Helm uninstall failed: ${ex}`)
    }

    try {
        deleteDeployment()
    } catch(ex) {
        core.setFailed(`Failed to delete deployment: ${ex}`)
    }
    
}

function main() {
    try {
        if (process.env['INPUT_ACTION'] == 'undeploy') {
            undeploy()
        } else {
            deploy()
        }
    } catch (ex) {
        core.setFailed(ex)
    }
}

main()
