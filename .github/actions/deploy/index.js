const github = require('@actions/github')
const core = require('@actions/core')
const { execSync, spawnSync, execFileSync, execFile } = require('child_process')

const helmBin = '/usr/bin/helm'
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

async function helmDeploy(releaseName) {
    try {
        execFileSync("helm", ['upgrade', '--install', '--atomic', releaseName], {'stdio': [0, 1, 2]})
    } catch (ex) {
        core.error(`Error running helm ${ex}`)
    }
}

async function helmUndeploy(releaseName) {
    try {
        execFileSync("helm", ['uninstall', releaseName], {'stdio': [0, 1, 2]})
    } catch (ex) {
        core.error(`Error running helm ${ex}`)
    }
}

async function deploy() {
    core.info("Starting deploy")
    // createDeployment()
    // set deploy status to pending
    helmDeploy('testrelase')
    // set deploy status to success/fail
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
