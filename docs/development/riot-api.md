# Riot API Configuration

SkillIssue.GG uses Riot Games APIs to retrieve account and match data.

This document explains how to configure Riot API access for local development.

## Prerequisites

- .NET 10 SDK
- A Riot Developer account
- A valid Riot API key

## Configuration Section

Riot API configuration is stored under the `RiotApi` section.

The application configuration should contain only non-secret defaults:

```json
{
  "RiotApi": {
    "ApiKey": "",
    "PlatformRoute": "euw1",
    "RegionalRoute": "europe"
  }
}