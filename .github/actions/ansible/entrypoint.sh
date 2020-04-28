#!/bin/sh

mkdir -p ~/.ssh
echo -n $DEPLOY_KEY | base64 -d > ~/.ssh/id_rsa
chmod 600 ~/.ssh/id_rsa
ansible-playbook -v -i ansible/hosts ansible/deploy.yml