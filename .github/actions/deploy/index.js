const github = require('@actions/github')
const core = require('@actions/core')
const { execSync, spawnSync, execFileSync, execFile } = require('child_process')

const ghToken = process.env['INPUT_TOKEN']
const context = github.context;
const gh = github.getOctokit(ghToken)

// core.info(JSON.stringify(context.payload, null, 1))

async function createDeployment() {
    core.info("Creating deployment")
    return await gh.repos.createDeployment({
        owner: context.payload.repository.owner.login,
        repo: context.payload.repository.name,
        ref: context.ref,
        environment: "testenv",
    })
}

async function setDeploymentState(id, state) {
    core.info(`Setting deployment state ${id} ${state}`)
    return await gh.repos.setDeploymentState({
        id,
        state
    })
}

async function helmDeploy(releaseName, chartName) {
    try {
        execFileSync("helm", ['upgrade', releaseName, chartName, '--install', '--atomic'], {'stdio': [0, 1, 2]})
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
    try {
        const deployment = await createDeployment()
        setDeploymentState(deployment.data.id, "pending")
        helmDeploy('testrelase', 'some/chart')
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
