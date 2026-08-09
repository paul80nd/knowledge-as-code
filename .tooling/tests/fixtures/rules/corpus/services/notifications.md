---
id: svc-notifications
tier: descriptive
status: live
repo: notifications
platform: azure-function
criticality: important
depends-on:
data-stores:
owner: alex.doe
---

# Notifications

`Service: svc-notifications` `LIVE`

## What it does

Sends the messages the borrower notifications capability describes. It is the target of that capability's
`implemented-by`, and of the catalogue's `depends-on`.

## Where it lives

The `notifications` repository.

## Environments

| Environment | URL                                 |
|-------------|-------------------------------------|
| Production  | https://notifications.example.com   |
