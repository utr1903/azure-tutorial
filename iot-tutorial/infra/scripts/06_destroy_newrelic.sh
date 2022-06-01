#!/bin/bash

#########################
### New Relic Destroy ###
#########################

terraform -chdir=../terraform/02_newrelic destroy \
  -var account_id=$NEWRELIC_ACCOUNT_ID \
  -var api_key=$NEWRELIC_API_KEY \
