const github = require('@actions/github')
const core = require('@actions/core')
const { execFileSync } = require('child_process')

const ghToken = process.env['INPUT_TOKEN']

const context = github.context;
// core.info(JSON.stringify(context.payload, null, 1))
const gh = github.getOctokit(ghToken)

async function createDeployment() {
    return await gh.repos.createDeployment({
        owner: context.payload.repository.owner.login,
        repo: context.payload.repository.name,
        ref: context.ref,
        environment: "testenv",
    })
}

async function neki() {
    let x = await execFileSync("helm", ["--list"])
    core.info(x)
}

function main() {
    try {
        var args = process.argv.slice(2);
        switch (args[0]) {
        case 'createDeployment':
            createDeployment()
            break;
        default:
            core.setFailed("Unknown action")
        }
    } catch(ex) {
        core.setFailed(ex)
    }
}

main()
