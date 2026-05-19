This folder contains helper scripts for generating and storing secrets for deployment.

Scripts:

- `generate_jwt_key.sh` / `generate_jwt_key.ps1` — generate a 64-byte base64 symmetric key suitable for `Jwt:Key`.
- `store_secrets_az.sh` — store secrets into Azure Key Vault using the `az` CLI.
- `store_secrets_gh.sh` — store secrets into GitHub repository secrets using the `gh` CLI.

Usage examples:

1. Generate a JWT key (Linux/macOS):

```bash
./scripts/generate_jwt_key.sh > jwt.key
cat jwt.key
```

2. Store secrets in Azure Key Vault:

```bash
# Login to Azure and create a Key Vault if needed
az login
az keyvault create -n my-bookquotes-kv -g myResourceGroup --location eastus
# Store secrets
./scripts/store_secrets_az.sh my-bookquotes-kv "$(cat jwt.key)" "Host=...;Database=bookquotes;Username=...;Password=..."
```

3. Store secrets in GitHub (needs `gh auth login`):

```bash
./scripts/store_secrets_gh.sh SarbastHesso/BookQuotes "$(cat jwt.key)" "Host=...;Database=bookquotes;Username=...;Password=..."
```

Security notes:

- Do NOT commit any real secrets into this repository. Use the scripts locally and pass secrets via stdin or environment variables.
- Use Key Vault + managed identity or GitHub OIDC for CI to avoid long-lived credentials where possible.
