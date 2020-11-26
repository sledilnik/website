const github = require('@actions/github')
const core = require('@actions/core')
const { execFileSync } = require('child_process')

const ghToken = process.env['INPUT_TOKEN']

const context = github.context;
// core.info(JSON.stringify(context.payload, null, 1))
const gh = github.getOctokit(ghToken)

async function createDeployment() {
    core.info("Creating deployment")
    return await gh.repos.createDeployment({
        owner: context.payload.repository.owner.login,
        repo: context.payload.repository.name,
        ref: context.ref,
        environment: "testenv",
    })
}

async function helmDeploy() {
    let x = execFileSync("/usr/bin/helm", ["--list"])
    core.info("neki", x)
}

async function deploy() {
    core.info("Starting deploy")
    createDeployment()
    // set deploy status to pending
    helmDeploy()

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
            createDeployment()
            break;
        case 'undeploy':
            undeploy()
            break;
        default:
            core.setFailed("Unknown action")
        }
    } catch(ex) {
        core.setFailed(ex)
    }
}

main()
