# **🔐 ASP.NET Core Authentication Samples**

This repository contains **self-contained examples** of different authentication strategies in **ASP.NET Core Web APIs**.  
 Each project demonstrates a different approach with real-world features and testing instructions.

---

## **📂 Projects**

### **1\. JWT Authentication (/jwt-auth)**

A sample project demonstrating **JWT (JSON Web Token) authentication**.

#### **🚀 Running the Project**

* `cd jwt-auth/JwtAuth.Api`  
* `dotnet run`

#### **🔑 Key Endpoints**

* `POST /api/auth/login` → Generates a JWT access token and refresh token.

* `GET /api/secure/hello` → Example of a protected endpoint requiring `Bearer` token.

#### **Example: Login Request**

* `curl https://localhost:7242/api/Auth/login \`  
*   `--request POST \`  
*   `--header 'Content-Type: application/json' \`  
*   `--data '{`  
*     `"userName": "Pranav",`  
*     `"password": "MadBongs@1999"`  
*   `}'`

**Response**

* `{`  
*   `"userId": "3fa2061b-a51e-413b-3cd3-08ddeb855179",`  
*   `"accessToken": "<JWT_TOKEN>",`  
*   `"refreshToken": "<REFRESH_TOKEN>"`  
* `}`

**Calling a Protected Endpoint**

* `curl -X GET https://localhost:5001/api/secure/hello \`  
*   `-H "Authorization: Bearer <JWT_TOKEN>"`

#### **🛠 Tech Stack**

* ASP.NET Core 8.0

* JWT Bearer Authentication

* Scalar / cURL for testing

---

### **2\. HMAC Authentication – Product Management API (/hmac-auth)**

A **.NET 9 Web API** with **HMAC authentication** and **Entity Framework Core (code-first)** for managing products.

#### **🚀 Running the Project**

* `cd hmac-auth/ProductApi`  
* `dotnet run`

Default ports:

* HTTP → `http://localhost:5001`

* HTTPS → `https://localhost:7260`

Scalar UI docs available at:  
 `https://localhost:7260/scalar/v1`

#### **🔑 Features**

* **CRUD for Products** (`/api/products`)

* **HMAC Authentication**

  * Requests signed with HMAC-SHA256

  * `Authorization` header carries clientId, signature, nonce, timestamp

  * Server validates signatures, timestamps, and replays (nonce cache)

* **Role-based Authorization**

  * Only clients with `ProductManager` role can access product APIs

* **EF Core Code-first**

  * Auto-migration at startup

#### **🔑 Example Endpoints**

* `GET /api/products/getAll/` → List all products

* `GET /api/products/getById/{id}` → Get product by ID

* `POST /api/products/create` → Create new product

* `PUT /api/products/update{id}` → Update product

* `DELETE /api/products/delete{id}` → Delete product

#### **🔐 Request Flow**

1. Client signs request using secret key → adds HMAC signature headers.

2. Server recomputes signature using stored secret → compares.

3. If valid, issues an identity with roles.

4. Authorization policy (`RequireProductManager`) enforces access.

5. Controller executes EF Core CRUD operations.

#### **🛠 Tech Stack**

* ASP.NET Core 9.0

* EF Core (code-first, SQL Server / SQLite)

* Custom HMAC Authentication Handler

* Scalar for API documentation

---

## **📘 Summary**

This repo currently demonstrates:

* ✅ **JWT Authentication** (token-based, stateless auth)

* ✅ **HMAC Authentication** (shared-secret, signed requests, replay protection)

Each sample is self-contained and runnable. More authentication strategies will be added over time.

---

