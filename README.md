# Payment Gateway Backend - Technical Documentation

A robust, microservices-based Fintech backend project built with **.NET 10**, designed to mimic core functionalities of payment providers like Stripe or Paystack.

---

## 🏗 Architecture Overview

The project follows a **Microservices Architecture** with an **Event-Driven** core, ensuring scalability, resilience, and loose coupling between components.

### 🧩 Core Components

1.  **[API Gateway (PaymentGateway.Api)](file:///c:/Users/hp/Documents/trae_projects/PersonalProjects/PaymentGateway/src/PaymentGateway.Api)**
    - Built with **YARP (Yet Another Reverse Proxy)**.
    - Acts as a unified entry point for all client requests.
    - Routes traffic to the appropriate downstream microservices.

2.  **[Merchant Service](file:///c:/Users/hp/Documents/trae_projects/PersonalProjects/PaymentGateway/src/PaymentGateway.MerchantService)**
    - Manages merchant onboarding and profiles.
    - Handles secure **API Key generation** and management.
    - Configures merchant-specific settings like Webhook URLs.

3.  **[Payment Service](file:///c:/Users/hp/Documents/trae_projects/PersonalProjects/PaymentGateway/src/PaymentGateway.PaymentService)**
    - The core transactional engine.
    - Implements **Redis-based Idempotency** to prevent duplicate charges.
    - Validates payment details (Credit Card format, currency, etc.).
    - Publishes `PaymentAuthorizedEvent` to the message broker upon successful authorization.

4.  **[Settlement Worker](file:///c:/Users/hp/Documents/trae_projects/PersonalProjects/PaymentGateway/src/PaymentGateway.SettlementWorker)**
    - An asynchronous background worker using **MassTransit**.
    - Consumes authorization events to update the **Transaction Ledger**.
    - Updates merchant balances and marks payments as `Captured`.
    - Publishes `PaymentStatusChangedEvent` for downstream notifications.

5.  **[Webhook Worker](file:///c:/Users/hp/Documents/trae_projects/PersonalProjects/PaymentGateway/src/PaymentGateway.WebhookWorker)**
    - Listens for status changes and notifies merchants via HTTP POST.
    - Implements retry logic (extensible) for reliable delivery.

---

## 🛠 Tech Stack & Tools

| Category | Technology |
| :--- | :--- |
| **Framework** | .NET 10 (Preview) |
| **API Gateway** | YARP |
| **Database** | PostgreSQL (Entity Framework Core) |
| **Messaging** | RabbitMQ (MassTransit) |
| **Caching** | Redis (StackExchange.Redis) |
| **Validation** | FluentValidation |
| **Error Handling** | RFC 7807 Problem Details Middleware |
| **Logging** | Serilog |
| **Containerization** | Docker & Docker Compose |

---

## 🔄 Core Data Flows

### 💳 Payment Processing Flow
1.  **Client** sends a payment request to the **API Gateway** with an `IdempotencyKey`.
2.  **API Gateway** routes the request to the **Payment Service**.
3.  **Payment Service** checks **Redis** for the `IdempotencyKey`:
    - If found, returns the cached response.
    - If not, validates the request using **FluentValidation**.
4.  **Payment Service** authorizes the payment (mocked), persists it to **PostgreSQL**, and publishes a `PaymentAuthorizedEvent`.
5.  **Settlement Worker** consumes the event, updates the **Merchant Balance**, and creates a **Transaction Ledger** entry.
6.  **Webhook Worker** receives the status update and notifies the **Merchant's API**.

---

## 🛡 Reliability & Security

-   **Idempotency**: All critical POST operations are idempotent via Redis, ensuring that network retries don't result in double-spending.
-   **Standardized Errors**: A global exception handling middleware ensures that all services return a consistent error format (`ProblemDetails`).
-   **Masked Sensitive Data**: Card numbers are masked (`**** **** **** 1234`) before being persisted to the database.
-   **API Key Security**: Merchants must provide a unique, base64-encoded API key for all transactional requests.

---

## 🚀 Getting Started

### Prerequisites
- .NET 10 SDK
- Docker Desktop

### 1. Spin up Infrastructure
```bash
docker-compose up -d
```
This starts PostgreSQL, RabbitMQ, and Redis.

### 2. Run the Services
You can run each project using `dotnet run` or use a solution-level runner in VS/Trae.

### 3. API Endpoints
- **Create Merchant**: `POST /api/merchant`
- **Initiate Payment**: `POST /api/payment` (Requires `IdempotencyKey` and `ApiKey`)

---

## 🗺 Future Roadmap
- [ ] Implement **Distributed Locking** with Redis for balance updates.
- [ ] Add **OAuth2/OpenID Connect** for Dashboard authentication.
- [ ] Integrate **Prometheus & Grafana** for metrics monitoring.
- [ ] Implement **Event Sourcing** for the Transaction Ledger.

---

## 🧪 Testing & Security

### 🔬 Unit & Integration Testing
The project includes a comprehensive test suite in the `tests/` directory:
- **Unit Tests**: Using **xUnit** and **Moq** to test business logic in isolation (Validators, Controllers).
- **Integration Tests**: (Planned) Using `WebApplicationFactory` to test end-to-end API flows.

To run tests:
```bash
dotnet test
```

### 🛡 Penetration & Security Testing
For a fintech application, security is paramount. We recommend the following tools and practices:

1. **Static Analysis (SAST)**:
   - Use **SonarQube** or **Snyk** to scan for vulnerabilities in the code (e.g., SQL Injection, Insecure Deserialization).
   - Regularly update NuGet packages to avoid known CVEs.

2. **Dynamic Analysis (DAST)**:
   - **OWASP ZAP**: Run automated scans against the API Gateway to find common vulnerabilities like Cross-Site Scripting (XSS) or insecure headers.
   - **Burp Suite**: Perform manual penetration testing for logic flaws in the payment flow.

3. **Security Checklist**:
   - [x] Masking sensitive PII (Card Numbers).
   - [x] API Key authentication.
   - [x] Idempotency for critical operations.
   - [ ] Implement Rate Limiting at the Gateway level.
   - [ ] Use HTTPS/TLS 1.3 for all communications.
   - [ ] Implement Row-Level Security (RLS) in the database for merchant data isolation.
