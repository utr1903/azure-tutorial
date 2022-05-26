#!/bin/bash

#######################
### New Relic Setup ###
#######################

terraform -chdir=../terraform/02_newrelic init

terraform -chdir=../terraform/02_newrelic plan \
  -var account_id=$NEWRELIC_ACCOUNT_ID \
  -var api_key=$NEWRELIC_API_KEY \
  -out tfplan

terraform -chdir=../terraform/02_newrelic apply tfplan
