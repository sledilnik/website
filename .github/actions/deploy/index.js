const github = require('@actions/github');
const core = require('@actions/core');


const context = github.context;
const gh = github.getOctokit(process.env['GH_TOKEN'])

var args = process.argv.slice(2);

function createDeployment() {
    console.log(JSON.stringify(context.payload, null, 1))
    // gh.repos.createDeployment({

    // })
}


switch (args[0]) {
case 'createDeployment':
    createDeployment()
    break;
default:
    core.setFailed("Unknown action")
}