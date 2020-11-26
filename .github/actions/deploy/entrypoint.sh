#!/bin/sh -l

echo "${INPUT_KUBECONFIG}" > ~/.kube/config

helm upgrade ${INPUT_RELEASE_NAME} ./helm-chart \
    --install \
    --atomic \
    --namespace=${ INPUT_NAMESPACE } \
    --set=image.tag= \
    --set=ingress.rule="${INPUT_INGRESS_RULE}" \