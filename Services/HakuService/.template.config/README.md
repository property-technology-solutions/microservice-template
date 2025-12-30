# Enterprise Microservice Template

This is a `dotnet new` template for creating enterprise-grade microservices.

## Installation

```bash
# From local directory
dotnet new install ./Services/HakuService

# Verify
dotnet new list | grep microservice
```

## Usage

```bash
# Basic usage
dotnet new microservice \
  --serviceName OrderService \
  --entityName Order \
  --output ./Services/OrderService

# Full options
dotnet new microservice \
  --serviceName ProductService \
  --entityName Product \
  --entityPlural Products \
  --databaseName productdb \
  --port 5002 \
  --output ./Services/ProductService
```

## Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `--serviceName` | Yes | - | Service name (e.g., OrderService) |
| `--entityName` | Yes | - | Entity name (e.g., Order) |
| `--entityPlural` | No | {entity}s | Plural form |
| `--databaseName` | No | {entity}db | Database name |
| `--port` | No | 5000 | HTTP port |
| `--useLocalBuildingBlocks` | No | true | Use project references |

## Uninstall

```bash
dotnet new uninstall ./Services/HakuService
```

