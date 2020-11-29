const github = require('@actions/github')
const core = require('@actions/core')
const { execFileSync } = require('child_process');

const ghToken = process.env['INPUT_TOKEN']
const context = github.context;
const gh = github.getOctokit(ghToken)

// core.info(JSON.stringify(context.payload, null, 1))

async function createDeployment() {

    const transient_environment = process.env['GITHUB_EVENT_NAME'] === 'pull_request'

    const payload = {
        owner: context.payload.repository.owner.login,
        repo: context.payload.repository.name,
        ref: context.ref,
        environment: process.env['INPUT_DEPLOYENV'],
        auto_inactive: true,
        transient_environment
    }
    core.info(`Creating deployment: ${JSON.stringify(payload)}`)
    try {
        return await gh.repos.createDeployment(payload)
    } catch (ex) {
        return new Error(ex)
    }
}

async function deleteDeployment() {
    const env = process.env['INPUT_DEPLOYENV']
    core.info(`Deleting deployment environment: ${env}`)
    try {
        deployments = await gh.repos.listDeployments({
            owner: context.payload.repository.owner.login,
            repo: context.payload.repository.name,
            environment: env,
        })

        deployments.array.forEach(deployment => {
            setDeploymentState(deployment.data.id, 'inactive').then(() => {
                gh.repos.deleteDeployment(deployment.data.id)
            })
        });        
    } catch {
        core.info(`Failed to delete deployment envrionment: ${env}`)
    }
}

async function setDeploymentState(deployment_id, state) {

    const actionId = context.runId
    const log_url = `https://github.com/overlordtm/website/runs/${actionId}`
    const environment_url = process.env['INPUT_DEPLOYURL']

    const payload = {
        owner: context.payload.repository.owner.login,
        repo: context.payload.repository.name,
        deployment_id,
        state,
        log_url,
        environment_url,
        mediaType: {"previews": ["flash", "ant-man"]}
    }
    core.info(`Setting deployment state: ${JSON.stringify(payload)}`)
    try {
        const status = await gh.repos.createDeploymentStatus(payload)
        core.info(`Deployment status created: ${JSON.stringify(status)}`)
        return status
    } catch (ex) {
        throw new Error(ex)
    }
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

    var deployment = undefined;
    try {
        deployment = await createDeployment()
        setDeploymentState(deployment.data.id, "in_progress")
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
        helm(['uninstall', releaseName, '--namespace', namespace])
    } catch (ex) {
        core.setFailed(`Helm uninstall failed: ${ex}`)
    }

    try {
        deleteDeployment()
    } catch (ex) {
        core.setFailed(`Failed to delete deployment: ${ex}`)
    }

}

try {
    if (process.env['INPUT_ACTION'] == 'undeploy') {
        undeploy()
    } else {
        deploy()
    }
} catch (ex) {
    core.setFailed(ex)
}
