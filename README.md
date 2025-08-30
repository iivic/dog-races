# 🏁 Dog Race Betting System

A comprehensive dog race betting system built with .NET 9, featuring automated race scheduling, betting management, and real-time result processing.

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK
- PostgreSQL database

### Quick Start

1. **Setup Database Connection**

   ```bash
   # Set your PostgreSQL connection string
   export ConnectionStrings__DefaultConnection="Host=localhost;Database=DogRaces;Username=your_user;Password=your_password"
   ```

2. **Run Migrations**

   ```bash
   cd DogRaces.Infrastructure
   dotnet ef database update
   ```

3. **Start the Background Service** (Race Management)

   ```bash
   cd DogRaces.BackgroundServices
   dotnet run
   ```

4. **Start the API** (In a separate terminal)
   ```bash
   cd DogRaces.Api
   dotnet run
   ```

## 🎯 What's Implemented

### ✅ **Phase 5: Race Auto-Generation with Background Services**

#### 🏆 **Core Features**

- **Automated Race Scheduling**: Maintains minimum 5 concurrent races
- **Full Race Lifecycle**: Scheduled → Betting Closed (5s before start) → Running → Finished
- **Random Result Generation**: Each race uses 100 random numbers to determine odds and results
- **Fun Race Names**: Auto-generated creative race names
- **Real-time Processing**: Background service checks every 2 seconds

#### 🎲 **Race System**

- **Timing**: 5-second intervals between races, 10-second race duration
- **Odds Calculation**: Based on random number frequency with house edge
- **Result Selection**: Randomly picks 3 numbers from the race's sequence
- **Race Validation**: Ensures no number appears 3 times consecutively

#### 🎮 **API Endpoints**

```http
GET /api/races/active          # Get all active races
GET /api/races/count           # Get active race count
GET /api/configuration         # Get system configuration
```

#### 💰 **Wallet System**

```http
GET /api/wallet/status         # Get wallet balance and transactions
POST /api/wallet/reset         # Reset wallet to starting balance
POST /api/wallet/reserve       # Reserve funds for betting
POST /api/wallet/release       # Release reserved funds
```

### 🏗️ **Architecture**

- **Domain-Driven Design**: Rich entities with business logic
- **CQRS Pattern**: Commands and queries with MediatR
- **Layered Architecture**: Domain → Application → Infrastructure → API
- **Background Services**: Automated race management
- **Entity Framework Core**: PostgreSQL with code-first migrations

## 🧪 Testing

### Run All Tests

```bash
dotnet test --verbosity minimal
```

### Test Categories

- **Domain Unit Tests**: Entity logic and business rules
- **Integration Tests**: API endpoints with in-memory database

## 📊 Monitoring Race System

### Check Race Status

```bash
curl http://localhost:5095/api/races/active
```

### Check System Configuration

```bash
curl http://localhost:5095/api/configuration
```

### Monitor Logs

The background service provides rich logging:

- 🔒 Betting closures
- 🚀 Race starts
- 🏆 Race finishes with results
- ➕ New race creation

## 🎯 Race Lifecycle Example

```
1. Race Created    : "Thunder Valley Sprint" starts in 15 seconds
2. Betting Open    : Players can place bets (10 seconds window)
3. Betting Closed  : 5 seconds before race start
4. Race Running    : 10-second race duration
5. Race Finished   : Results determined from random sequence
6. Payout Processing: Winners receive payouts
7. Next Race      : New race created automatically
```

## 🔮 Coming Next

- **Phase 6**: Betting System (Tickets and Bets)
- **Phase 7**: API Endpoints for Core Operations
- **Phase 8**: Background Services for Race Maintenance
- **Phase 9**: Result Processing and Payout System
- **Phase 10**: Real-time Updates with SignalR

---

**Built with ❤️ using .NET 9, PostgreSQL, and Domain-Driven Design principles.**
