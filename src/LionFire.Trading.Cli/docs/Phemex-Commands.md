# Phemex CLI Commands

The Trading CLI provides several commands for interacting with the Phemex exchange API.

## Configuration

Before using the Phemex commands, you need to configure your API credentials. You can do this in several ways:

### Method 1: appsettings.json

Edit the `appsettings.json` file in the CLI directory:

```json
{
  "Phemex": {
    "ApiKey": "your-api-key-here",
    "ApiSecret": "your-api-secret-here",
    "IsTestnet": true,
    "UseHighRateLimitApi": false,
    "SubAccountId": null,
    "RateLimitPerSecond": 10
  }
}
```

### Method 2: Environment Variables

Set environment variables (note the double underscore):

```bash
export Phemex__ApiKey="your-api-key"
export Phemex__ApiSecret="your-api-secret"
export Phemex__IsTestnet="true"
```

### Method 3: Command Line Parameters

You can override configuration using command-line flags:

```bash
lft phemex balance --api-key "your-key" --api-secret "your-secret"
```

## Available Commands

### 1. Check Account Balance

Display account balance and open positions:

```bash
# Using testnet (default)
lft phemex balance

# Using mainnet
lft phemex balance --testnet false

# With specific subaccount
lft phemex balance --subaccount 123456

# With high-rate API (mainnet only, 100 req/s limit)
lft phemex balance --testnet false --high-rate

# Verbose output
lft phemex balance -v
```

The balance command shows:
- Account balance
- Available balance
- Margin balance
- Position margin
- Order margin
- Unrealized PnL
- Open positions (if any)

### 2. List Subaccounts

Display all subaccounts associated with your main account:

```bash
# List subaccounts
lft phemex subaccounts

# On mainnet
lft phemex subaccounts --testnet false
```

### 3. Show Positions

Display detailed information about open positions:

```bash
# Show all positions
lft phemex positions

# For specific subaccount
lft phemex positions --subaccount 123456

# For BTC-denominated account
lft phemex positions --currency BTC

# USD-denominated (default)
lft phemex positions --currency USD
```

The positions command shows:
- Symbol
- Side (Buy/Sell)
- Size
- Entry price
- Mark price
- Unrealized PnL
- Realized PnL
- Leverage
- Margin

## Command Options

### Common Options

Most Phemex commands support these options:

- `--testnet, -t`: Use testnet API (default: true)
- `--verbose, -v`: Enable verbose output
- `--subaccount <ID>`: Specify subaccount ID
- `--help`: Show help for the command

### Balance Command Options

- `--api-key`: Override API key from configuration
- `--api-secret`: Override API secret from configuration
- `--high-rate, -h`: Use high-rate API endpoint (mainnet only)

### Positions Command Options

- `--currency, -c`: Currency for the account (USD or BTC, default: USD)

## API Endpoints

The CLI automatically configures the correct endpoint based on your settings:

- **Testnet**: `https://testnet-api.phemex.com` (10 req/s limit)
- **Mainnet**: `https://api.phemex.com` (10 req/s limit)
- **High-rate Mainnet**: `https://vapi.phemex.com` (100 req/s limit)

## Examples

### Example 1: Check testnet balance

```bash
lft phemex balance
```

Output:
```
╭─────────────────────────────────────────╮
│ Phemex Account Balance (ID: 123456)     │
├───────────────────┬─────────────────────┤
│ Currency          │                 USD │
│ Account Balance   │           10000.00  │
│ Available Balance │            9500.00  │
│ Margin Balance    │           10000.00  │
│ Position Margin   │             500.00  │
│ Order Margin      │               0.00  │
│ Unrealized PnL    │              25.50  │
└───────────────────┴─────────────────────┘
```

### Example 2: Show positions with details

```bash
lft phemex positions
```

Output:
```
╭────────────────────────────────────────────────────────────────────────╮
│ Open Positions (2)                                                    │
├──────────┬──────┬────────┬────────────┬───────────┬──────────────────┤
│ Symbol   │ Side │ Size   │ Entry Price│ Mark Price│ Unrealized PnL   │
├──────────┼──────┼────────┼────────────┼───────────┼──────────────────┤
│ BTCUSDT  │ Buy  │ 100    │   45000.00 │  45250.00 │      25.00 USD   │
│ ETHUSDT  │ Sell │ 500    │    3200.00 │   3195.00 │      25.00 USD   │
└──────────┴──────┴────────┴────────────┴───────────┴──────────────────┘
```

## Troubleshooting

### API Credentials Not Found

If you see the error "API credentials not configured", ensure that:
1. Your API key and secret are correctly set in configuration
2. Environment variables use double underscores (e.g., `Phemex__ApiKey`)
3. The configuration file is in the same directory as the executable

### API Errors

Common API error codes:
- `401`: Invalid API credentials
- `403`: Insufficient permissions
- `429`: Rate limit exceeded
- `10002`: Invalid signature

### Network Issues

If you're having connection issues:
1. Check your internet connection
2. Verify the API endpoint is accessible
3. Try using a different endpoint (testnet vs mainnet)
4. Check if you're behind a proxy or firewall

## Security Notes

1. **Never commit API credentials** to version control
2. Use environment variables or secure vaults for production
3. Use testnet for development and testing
4. Consider using subaccounts with limited permissions
5. Rotate API keys regularly
6. Use IP whitelisting on Phemex when possible

## Additional Resources

- [Phemex API Documentation](https://github.com/phemex/phemex-api-docs)
- [Phemex Testnet](https://testnet.phemex.com)
- [API Key Management](https://phemex.com/user/security/api-management)