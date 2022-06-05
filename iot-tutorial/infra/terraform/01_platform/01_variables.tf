### Variables ###

## General

# project
variable "project" {
  type    = string
  default = "iot"
}

# location_long
variable "location_long" {
  type    = string
  default = "westeurope"
}

# location_short
variable "location_short" {
  type    = string
  default = "euw"
}

# stage_long
variable "stage_long" {
  type    = string
  default = "dev"
}

# stage_short
variable "stage_short" {
  type    = string
  default = "d"
}

# instance
variable "instance" {
  type    = string
  default = "001"
}

## Specific

# shared
variable "shared" {
  type    = string
  default = "shared"
}

# platform
variable "platform" {
  type    = string
  default = "platform"
}

# stats
variable "stats" {
  type    = string
  default = "stats"
}

## Resource Names

# Resource Group
variable "project_resource_group_name" {
  type    = string
}

# Service Bus
variable "project_service_bus_namespace_name" {
  type    = string
}

variable "project_service_bus_queue_name_input" {
  type    = string
}

# Event Hub
variable "project_event_hub_namespace_name" {
  type    = string
}

variable "project_event_hub_name" {
  type    = string
}

variable "project_event_hub_consumer_group_name_tsi" {
  type    = string
}

variable "project_event_hub_consumer_group_name_stats" {
  type    = string
}

variable "diagnostics_event_hub_name" {
  type    = string
}

variable "diagnostics_event_hub_consumer_group_name" {
  type    = string
}

# IoT Hub
variable "project_iot_hub_name" {
  type    = string
}

# Storage Account
variable "project_storage_account_name" {
  type    = string
}

variable "project_blob_container_name_stats" {
  type    = string
}

variable "project_blob_container_name_diags" {
  type    = string
}

# Timeseries Insights
variable "project_timeseries_insight_name" {
  type    = string
}

# MySQL Server
variable "project_mysql_server_name" {
  type    = string
}

# Kubernetes Cluster
variable "project_kubernetes_cluster_name" {
  type    = string
}

variable "project_kubernetes_cluster_nodepool_name" {
  type    = string
}
