# TODO - Sentinel Assets API

✅ **Desenvolvimento Completo (todos passos feitos)**

- [x] 1. Criar solution e csproj
- [x] 2. Program.cs com DI setup
- [x] 3. Modelos (Asset.cs)
- [x] 4. DbContext
- [x] 5. Controller Assets
- [x] 6. Service Notification
- [x] 7. JWT Auth
- [x] 8. README.md
- [x] 9. Infrastructure IAM
- [x] 10. Migrations e run

---

🚀 **PASSOS POSTGRES - NOVA DB!**

- [x] ✅ Docker Postgres criado (`docker compose up -d`)
- [ ] **1. Restore + Migrations Postgres** (src/SentinelAssetsAPI):

  ```
  dotnet restore
  dotnet ef migrations remove --force  # Limpa SQLite
  dotnet ef migrations add InitialPostgres
  dotnet ef database update
  ```

  ```
  dotnet ef migrations add InitialCreate
  dotnet ef database update
  ```

  _Resultado: assets.db com tabela Assets_

- [ ] **2. Configurar AWS SNS** (para notifications no POST):

  ```
  aws configure
  ```

  _Access Key / Secret / Region=us-east-1 (use infra/iam-policy.json)_

- [ ] **3. Executar API**:

  ```
  dotnet restore
  dotnet run
  ```

  _Swagger: https://localhost:52xx/swagger_

- [ ] **4. Testar Fluxo Completo**:
  ```
  1. POST /api/auth/login → TOKEN (admin/password)
  2. GET /api/assets -H "Authorization: Bearer TOKEN" → []
  3. POST /api/assets (...) → Salva DB + SNS email!
  ```

**Após marcar todos [x], API PRODUÇÃO PRONTA!** 🎉
