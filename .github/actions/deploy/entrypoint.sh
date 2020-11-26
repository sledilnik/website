#!/bin/sh -l

mkdir -p ${HOME}/.kube/
echo "${INPUT_KUBECONFIG}" > ${HOME}/.kube/config


create_deployment() {
    curl --request POST \
        --url https://api.github.com/repos/overlordtm/data-api/deployments \
        --header "Authorization: Bearer ${INPUT_TOKEN}" \
        --header 'Content-Type: application/json' \
        --data '{
            "ref": "${GITHUB_REF}",
            "environment": "pr-1",
            "payload": {
                "releaseName": "data-api-pr-1",
                "namespace": "default"
            }
        }'
}

create_deployment 

helm list --namespace ${INPUT_NAMESPACE}

# helm upgrade ${INPUT_RELEASE_NAME} ./helm-chart \
#     --install \
#     --atomic \
#     --namespace=${INPUT_NAMESPACE} \
#     --set=image.tag=${IMAGE_TAG} \
#     --set=ingress.rule="${INPUT_INGRESS_RULE}"