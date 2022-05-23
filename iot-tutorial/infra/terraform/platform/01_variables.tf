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
