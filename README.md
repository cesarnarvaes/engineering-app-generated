# BizCrm - Enterprise CRM Web System

A comprehensive CRM solution built with Vue.js frontend and C# Minimal APIs backend, featuring role-based authentication, real-time communication, and background job processing.

## 🚀 Features

### Core Functionality
- **Customer Relationship Management**: Complete contact and company management
- **Sales Pipeline**: Opportunity tracking and management
- **Activity Management**: Track interactions and communications
- **Role-Based Access Control**: systemadmin, userstaff, manager, businessuser roles
- **Real-time Notifications**: SignalR integration for live updates
- **Background Job Processing**: Hangfire for scheduled tasks and bulk operations

### Technical Features
- **Modern Frontend**: Vue.js 3 with Composition API, Pinia state management
- **Robust Backend**: .NET 8 Minimal APIs with Entity Framework Core
- **Secure Authentication**: JWT tokens with role-based authorization
- **Real-time Communication**: SignalR hubs for instant notifications
- **Background Processing**: Hangfire for asynchronous operations
- **Cloud-Native**: Azure Container Apps and Static Web Apps
- **Database**: Azure SQL Database with advanced security
- **Monitoring**: Application Insights integration
- **Security**: Azure Key Vault for secrets management

## 🏗️ Architecture

### Frontend (Vue.js)
```
src/frontend/
├── src/
│   ├── components/     # Reusable Vue components
│   ├── views/          # Page components
│   ├── stores/         # Pinia state stores
│   ├── services/       # API services
│   ├── router/         # Vue Router configuration
│   ├── utils/          # Utility functions
│   └── types/          # TypeScript type definitions
├── public/             # Static assets
└── package.json        # Dependencies and scripts
```

### Backend (C# Minimal APIs)
```
src/backend/
├── Models/             # Entity Framework models
├── DTOs/               # Data Transfer Objects
├── Services/           # Business logic services
├── Endpoints/          # API endpoint definitions
├── Hubs/               # SignalR hubs
├── Jobs/               # Background job definitions
├── Migrations/         # Entity Framework migrations
└── Program.cs          # Application startup
```

### Infrastructure (Bicep)
```
infra/
├── main.bicep          # Main deployment template
├── core/               # Core Azure resources
│   ├── database.bicep
│   ├── keyvault.bicep
│   ├── monitoring.bicep
│   └── registry.bicep
└── app/                # Application-specific resources
    ├── backend.bicep
    └── frontend.bicep
```

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
- [Azure Developer CLI](https://aka.ms/azd-install)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [SQL Server LocalDB](https://docs.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb) (for local development)

### Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd bizcrm
   ```

2. **Setup environment variables**
   ```bash
   cp .env.template .env
   # Edit .env file with your configuration
   ```

3. **Start local development**
   ```powershell
   # Windows
   .\scripts\local-dev.ps1
   
   # Or manually:
   # Backend
   cd src/backend
   dotnet restore
   dotnet ef database update
   dotnet watch run
   
   # Frontend (new terminal)
   cd src/frontend
   npm ci
   npm run dev
   ```

4. **Access the application**
   - Frontend: http://localhost:5173
   - Backend API: https://localhost:7001
   - Swagger UI: https://localhost:7001/swagger
   - Hangfire Dashboard: https://localhost:7001/hangfire

### Azure Deployment

1. **Initialize Azure Developer CLI**
   ```bash
   azd auth login
   azd init
   azd env new <environment-name>
   ```

2. **Deploy to Azure**
   ```powershell
   # Windows
   .\scripts\deploy.ps1 -EnvironmentName "production" -Location "centralus"
   
   # Or using azd directly:
   azd up
   ```

3. **Monitor deployment**
   ```bash
   azd monitor
   ```

## 🔐 Authentication & Authorization

### Roles
- **systemadmin**: Full system access, user management
- **manager**: Team management, advanced reporting
- **userstaff**: Standard CRM operations
- **businessuser**: Read-only access, basic operations

### Default Credentials (Development)
- **Admin**: admin@bizcrm.com / Admin123!
- **Manager**: manager@bizcrm.com / Manager123!
- **Staff**: staff@bizcrm.com / Staff123!
- **Business User**: user@bizcrm.com / User123!

## 📊 Monitoring & Observability

- **Application Insights**: Performance monitoring and diagnostics
- **Health Checks**: Built-in health endpoints
- **Logging**: Structured logging with Serilog
- **Hangfire Dashboard**: Background job monitoring
- **SignalR Metrics**: Real-time connection monitoring

## 🔧 Development

### API Documentation
- Swagger UI available at `/swagger`
- OpenAPI specification at `/swagger/v1/swagger.json`

### Database Migrations
```bash
# Add new migration
dotnet ef migrations add <MigrationName> --project src/backend

# Update database
dotnet ef database update --project src/backend

# Remove last migration
dotnet ef migrations remove --project src/backend
```

### Background Jobs
- Email notifications
- Data synchronization
- Report generation
- Cleanup tasks

### Real-time Features
- User activity notifications
- System alerts
- Live data updates
- Chat functionality

## 🚀 Deployment

### Azure Resources
- **Azure Container Apps**: Backend API hosting
- **Azure Static Web Apps**: Frontend hosting
- **Azure SQL Database**: Data storage
- **Azure Container Registry**: Container images
- **Azure Key Vault**: Secrets management
- **Azure Application Insights**: Monitoring
- **Azure Log Analytics**: Centralized logging

### CI/CD Pipeline
- GitHub Actions or Azure DevOps
- Automated testing and deployment
- Environment-specific configurations
- Database migration automation

## 📚 Documentation

- [API Documentation](docs/api.md)
- [Frontend Development Guide](docs/frontend.md)
- [Backend Development Guide](docs/backend.md)
- [Deployment Guide](docs/deployment.md)
- [Troubleshooting](docs/troubleshooting.md)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Support

For support, please contact the development team or create an issue in the repository.

---

**Built with ❤️ using Vue.js, C# Minimal APIs, and Azure**