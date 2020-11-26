const github = require('@actions/github');
const core = require('@actions/core');


const context = github.context;
const gh = github.getOctokit(process.env['GH_TOKEN'])

function createDeployment() {
    console.log(JSON.stringify(context.payload, null, 1))
    // gh.repos.createDeployment({

    // })
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
