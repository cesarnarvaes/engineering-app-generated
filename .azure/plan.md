# Azure Deployment Plan

## Project Overview
- **Name**: Enterprise CRM Web System
- **Type**: Full-stack web application
- **Mode**: NEW (greenfield project)
- **Date**: March 24, 2026

## Requirements Analysis
- **Classification**: Line of Business Application
- **Scale**: Enterprise (multi-role, multi-user)
- **Budget**: Standard (production-ready)
- **Security**: High (role-based access control)

## Technology Stack
- **Frontend**: Vue.js (SPA)
- **Backend**: C# .NET 8+ with Minimal APIs
- **Database**: Azure SQL Database
- **Background Jobs**: Hangfire
- **Real-time Communication**: SignalR
- **Authentication**: Role-based (systemadmin, userstaff, manager, businessuser)

## Architecture Components

### Frontend (Vue.js Client)
- **Service**: Azure Static Web Apps
- **Features**: 
  - SPA with Vue 3 + Vite
  - SignalR client integration
  - Role-based UI components
  - JWT token management

### Backend (C# Minimal APIs)
- **Service**: Azure Container Apps
- **Features**:
  - .NET 8+ Minimal APIs
  - JWT authentication middleware
  - Role-based authorization
  - SignalR Hub
  - Hangfire dashboard
  - Swagger/OpenAPI documentation

### Database
- **Service**: Azure SQL Database (General Purpose)
- **Features**:
  - User management tables
  - Role definitions
  - Application data tables
  - Hangfire job storage

### Background Processing
- **Service**: Hangfire (integrated with API)
- **Features**:
  - Job scheduling
  - Recurring tasks
  - Job monitoring dashboard

## Azure Services Map

| Component | Azure Service | SKU/Tier |
|-----------|---------------|----------|
| Frontend | Static Web Apps | Standard |
| Backend API | Container Apps | Consumption |
| Database | SQL Database | General Purpose (S2) |
| Authentication | Entra ID | Free tier |
| Secrets | Key Vault | Standard |
| Container Registry | Container Registry | Basic |
| Monitoring | Application Insights | Standard |

## Infrastructure Recipe
- **Tool**: Azure Developer CLI (azd)
- **IaC**: Bicep templates
- **Deployment**: Container-based

## Security Plan
- JWT-based authentication
- Role-based authorization (RBAC)
- Azure Key Vault for secrets
- Managed Identity for service-to-service auth
- HTTPS everywhere
- CORS configuration

## User Roles & Permissions
1. **systemadmin**: Full system access, user management
2. **userstaff**: Standard user operations, limited access
3. **manager**: Team management, reporting access
4. **businessuser**: Business operations, data entry

## Development Environment Setup
- Docker for local development
- Hot reload for both frontend and backend
- Local SQL Server or SQL LocalDB
- Environment-specific configuration

## Deployment Steps
1. ✅ **Plan Created** - Current step
2. ⏳ **Research Components** - Load service references
3. ⏳ **Confirm Azure Context** - Subscription and location
4. ⏳ **Generate Artifacts** - Infrastructure and config files
5. ⏳ **Harden Security** - Apply security best practices
6. ⏳ **Validate** - Pre-deployment checks
7. ⏳ **Deploy** - Execute deployment

## Azure Context
- **Application Name**: BizCrm
- **Subscription**: cesarng@gmail.com (default)
- **Region**: Central US

## Status
**Current**: ✅ Plan Approved - Executing
**Next**: Generate application artifacts
- ✅ Research Components Complete
- ✅ Azure Context Confirmed  
- ⏳ Generate Artifacts (In Progress)