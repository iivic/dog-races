# HTTP Test Files

This directory contains HTTP test files for testing API endpoints using REST clients like:

- Visual Studio Code REST Client extension
- JetBrains HTTP Client
- Postman (import .http files)
- Any other tool that supports .http format

## File Organization

- `Wallet.http` - Wallet management endpoints
- `Race.http` - Race management endpoints
- `Betting.http` - Betting system endpoints
- `Results.http` - Result processing endpoints

## Usage

1. Install a REST client extension in your IDE
2. Open any `.http` file
3. Click the "Send Request" link above each request
4. View responses inline

## Environment Variables

All files use the common variable:

- `@DogRaces.Api_HostAddress` - Base URL for the API (default: http://localhost:5095)

Update this variable in each file if your API runs on a different port.
