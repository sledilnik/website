#!/bin/sh -l

set -e
# set -v # print commands, do not expand variables
# set -x # print commands, expand vars

mkdir -p ${HOME}/.kube/
echo "${INPUT_KUBECONFIG}" > ${HOME}/.kube/config
chmod 400 ${HOME}/.kube/config

helm repo add ${INPUT_HELMREPONAME} ${INPUT_HELMREPOURL}
helm repo update

exec node /index.js