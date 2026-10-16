#!/bin/sh
# Writes the Terraform for an auth web app with nowhere to send its logs.
set -eu

mkdir -p infra

cat > infra/main.tf <<'EOF'
terraform {
  required_providers {
    azurerm = { source = "hashicorp/azurerm", version = "~> 4.0" }
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "auth" {
  name     = "rg-auth-${var.environment}"
  location = var.location
}

resource "azurerm_service_plan" "auth" {
  name                = "asp-auth-${var.environment}"
  resource_group_name = azurerm_resource_group.auth.name
  location            = azurerm_resource_group.auth.location
  os_type             = "Linux"
  sku_name            = "P1v3"
}

resource "azurerm_linux_web_app" "auth" {
  name                = "app-auth-${var.environment}"
  resource_group_name = azurerm_resource_group.auth.name
  location            = azurerm_resource_group.auth.location
  service_plan_id     = azurerm_service_plan.auth.id
  site_config {}
}
EOF

cat > infra/variables.tf <<'EOF'
variable "environment" {
  type = string
}

variable "location" {
  type    = string
  default = "uksouth"
}
EOF

cat > README.md <<'EOF'
# auth

Signs shoppers in. The infrastructure is in `infra/`.

    terraform -chdir=infra plan
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the auth infrastructure"
