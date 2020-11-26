#!/bin/sh -l

set -e
set -v # print commands, do not expand variables
#set -x # print commands, expand vars

mkdir -p ${HOME}/.kube/
echo "${INPUT_KUBECONFIG}" > ${HOME}/.kube/config

create_deployment() {
    node /index.js createDeployment
}

create_deployment

helm list --namespace ${INPUT_NAMESPACE}

# helm upgrade ${INPUT_RELEASE_NAME} ./helm-chart \
#     --install \
#     --atomic \
#     --namespace=${INPUT_NAMESPACE} \
#     --set=image.tag=${IMAGE_TAG} \
#     --set=ingress.rule="${INPUT_INGRESS_RULE}"