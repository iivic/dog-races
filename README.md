# 🏁 Dog Race Betting System

A real-time dog race betting system built with .NET 9, Entity Framework Core, and PostgreSQL. The system allows users to place bets on simulated dog races with automatic race generation, odds calculation, and ticket processing.

## 🏗️ Architecture

The solution follows a **Layered arhitecture** approach with **Domain-Driven Design (DDD)** principles:

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
├─────────────────────────────────────────────────────────────┤
│  DogRaces.Api          │  DogRaces.BackgroundServices       │
│  • Minimal APIs        │  • Race Scheduling Worker          │
│  • Swagger/OpenAPI     │  • Ticket Processing Worker        │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                         │
├─────────────────────────────────────────────────────────────┤
│              DogRaces.Application                           │
│  • CQRS with MediatR   • Commands & Queries                │
│  • Feature Folders     • Validation Logic                  │
│  • Business Rules      • Data Contracts                    │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                    Domain Layer                             │
├─────────────────────────────────────────────────────────────┤
│                DogRaces.Domain                              │
│  • Rich Domain Entities  • Value Objects                   │
│  • Business Logic        • Domain Services                 │
│  • Invariants            • Enums & Constants               │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                        │
├─────────────────────────────────────────────────────────────┤
│             DogRaces.Infrastructure                         │
│  • Entity Framework Core  • Database Configurations        │
│  • PostgreSQL Provider    • Migrations                     │
│  • Repository Pattern     • External Services              │
└─────────────────────────────────────────────────────────────┘
```

## 🎯 Key Features

### 🏇 Race Management

- **Automatic Race Generation**: Background service creates races every 2 seconds
- **Race Lifecycle**: Scheduled → Betting Open → Betting Closed → Running → Finished
- **Random Results**: Each race uses 100 random numbers (1-6) to determine outcomes
- **Fun Race Names**: Auto-generated creative race names
- **Configurable Settings**: Minimum active races, race duration, betting windows

### 💰 Betting System

- **Multiple Bet Types**: Winner, Top2, Top3
- **Dynamic Odds**: Calculated based on statistical simulation of race outcomes
- **Ticket System**: Multiple bets per ticket with combined odds
- **Validation**: Stake limits, race availability, timing checks
- **Payment Flow**: Reserve → Validate → Commit pattern

### 🎲 Odds Calculation

- **Statistical Approach**: Uses 100 random numbers to simulate ~33 race outcomes
- **Bet Type Specific**: Different minimum odds for Winner (1.1x), Top2 (1.05x), Top3 (1.03x)
- **Fair Odds**: Based on actual probability calculations
- **Real-time Updates**: Odds generated at race creation

### 💳 Wallet System

- **In-Memory Simulation**: 100 units starting balance
- **Transaction Tracking**: All wallet operations logged
- **Fund Management**: Reserve, commit, release, payout operations
- **Balance Validation**: Prevents overdrafts and invalid operations

### 🔄 Automated Processing

- **Race Scheduling Worker**: Maintains minimum active races (7 by default)
- **Ticket Processing Worker**: Processes finished tickets every 5 seconds
- **Result Determination**: Automatic race finishing and result calculation
- **Payout Processing**: Automatic winnings distribution

### 🛡️ Security & Error Handling

- **Global Exception Middleware**: Catches all unhandled exceptions
- **Structured Error Responses**: Consistent JSON error format
- **Environment-Aware**: Detailed errors in development, safe messages in production
- **Request Tracing**: Unique trace IDs for error tracking
- **Comprehensive Logging**: All exceptions logged with context

## 🚀 Getting Started

### Prerequisites

- **.NET 9 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **PostgreSQL 13+** - [Download here](https://www.postgresql.org/download/)
- **Docker** (optional, for containerized database use in integration tests)

### 🗄️ Database Setup

#### Option 1: Local PostgreSQL

```bash
# Create database
createdb dograces

# Update connection string in appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=dograces;Username=your_user;Password=your_password"
  }
}
```

#### Option 2: Docker PostgreSQL

```bash
# Run PostgreSQL in Docker
docker run --name dograces-postgres \
  -e POSTGRES_DB=dograces \
  -e POSTGRES_USER=dograces \
  -e POSTGRES_PASSWORD=dograces123 \
  -p 5432:5432 \
  -d postgres:15

# Connection string for Docker
"DefaultConnection": "Host=localhost;Database=dograces;Username=dograces;Password=dograces123"
```

### 📦 Installation & Setup

1. **Clone and Build**

```bash
git clone <repository-url>
cd DogRaces
dotnet restore
dotnet build
```

2. **Run Database Migrations**

```bash
cd DogRaces.Infrastructure
dotnet ef database update --startup-project ../DogRaces.Api
```

3. **Start the Applications**

**Terminal 1 - API Server:**

```bash
cd DogRaces.Api
dotnet run
```

**Terminal 2 - Background Services:**

```bash
cd DogRaces.BackgroundServices
dotnet run
```

### 🌐 Access Points

- **API Documentation**: http://localhost:5000/swagger
- **API Base URL**: http://localhost:5000/api
- **Health Check**: http://localhost:5000/health

## 🧪 Testing

### 🔧 Running Tests

```bash
# Run all tests
dotnet test

# Run specific test projects
dotnet test Tests/UnitTests/DogRaces.Domain.UnitTests/
dotnet test Tests/UnitTests/DogRaces.Application.UnitTests/
dotnet test Tests/IntegrationTests/DogRaces.Api.IntegrationTests/

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "TicketProcessingIntegrationTests"
```

### 📋 Test Categories

#### **Unit Tests**

- **Domain Tests**: Entity validation, business rules, value objects
- **Application Tests**: Command/query handlers, business logic

#### **Integration Tests**

- **API Tests**: End-to-end API functionality
- **Database Tests**: Entity Framework operations
- **Wallet Tests**: Payment flow validation
- **Ticket Processing**: Complete betting workflow

#### **Test Database**

Integration tests use **Testcontainers** with PostgreSQL for isolated testing:

- Automatic Docker container management
- Clean database per test run
- No manual setup required

## 🎮 Usage Examples

### 📊 API Endpoints

- **Wallet Balance**: `GET /api/wallet/balance`
- **Active Races**: `GET /api/races/active`
- **Race Odds**: `GET /api/races/{id}/odds`
- **Place Bet**: `POST /api/tickets/place-bet`
- **Reset Wallet**: `POST /api/wallet/reset` (testing only)

## 🔧 Configuration

### Database Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=dograces;Username=user;Password=pass"
  }
}
```

### System Configuration (Database)

The system uses database-stored configuration with these defaults:

- **Minimum Active Races**: 7
- **Race Duration**: 30 seconds
- **Betting Close Time**: 5 seconds before race start
- **Minimum Ticket Stake**: 0.1
- **Maximum Ticket Win**: 10,000
- **Worker Intervals**: Race scheduling (2s), Ticket processing (5s)

## 🎯 Business Rules

### 🏇 Race Rules

- Races have 6 selections (dogs numbered 1-6)
- Each race generates 100 random numbers for result determination
- Betting closes 5 seconds before race start
- Race results are top 3 positions from random selection

### 💰 Betting Rules

- **Winner**: Dog must finish 1st
- **Top2**: Dog must finish 1st or 2nd
- **Top3**: Dog must finish 1st, 2nd, or 3rd
- All bets on a ticket must win for the ticket to win
- Payout = stake × combined odds of all bets

### 💳 Payment Rules

1. **Validation**: Check race availability, stake limits, odds
2. **Reserve Funds**: Hold stake amount in wallet
3. **Revalidation**: Ensure race hasn't started
4. **Finalization**: Commit funds and approve ticket

### 🏆 Payout Rules

- Winning tickets: Stake × combined odds
- Losing tickets: No payout (stake lost)
- Processing occurs automatically after race completion

## 🚨 Troubleshooting

### Common Issues

**Database Connection Issues:**

```bash
# Check PostgreSQL is running
pg_isready -h localhost -p 5432

# Verify connection string in appsettings.json
# Run migrations if database is empty
dotnet ef database update --startup-project DogRaces.Api
```

**Port Conflicts:**

```bash
# API runs on port 5000 by default
# Change in launchSettings.json if needed
# Background services run on different ports
```

**Test Failures:**

```bash
# Ensure Docker is running for integration tests
docker --version

# Clean and rebuild
dotnet clean
dotnet build
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Write tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## 🎉 Quick Start

1. **Start the system**: Run both API and Background Services
2. **Wait for races**: System generates races automatically (2-3 seconds)
3. **Access Swagger**: Visit http://localhost:5000/swagger to interact with APIs
4. **Place bets**: Use the API to place bets on active races
5. **Check results**: Monitor wallet balance to see if you won!

🎊 **Enjoy betting on the races!** 🏁
