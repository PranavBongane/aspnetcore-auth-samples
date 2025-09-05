# **🔐 JWT Authentication \- ASP.NET Core**

This project is a self-contained example of **JWT (JSON Web Token) authentication** within an ASP.NET Core Web API.

## **🚀 Getting Started**

### **Clone the Repository**

Begin by cloning the main repository to your local machine:

git clone git@github.com:PranavBongane/aspnetcore-auth-samples.git  
cd aspnetcore-auth-samples

### **Running the Project**

Navigate into the jwt-auth project directory and run the application using the dotnet CLI:

cd jwt-auth/JwtAuth.Api  
dotnet run

**Testing the API**

Use tools like **Scalar** or **cURL** to interact with the API endpoints. The example requests below will guide you.

## **📌 Project Details: JWT Authentication (/jwt-auth)**

* **Status**: Authentication is **implemented**. Authorization   
* **Key Endpoints**:  
  * POST /api/auth/login: Generates a **JWT token**.  
* **Example Requests**:  
  **Login Request**  
  curl https://localhost:7242/api/Auth/login \\  
*   \--request POST \\  
*   \--header 'Content-Type: application/json' \\  
*   \--data '{  
*   "userName": "Pranav",  
*   "password": "MadBongs@1999"  
* }'

  **Response**  
  {  
*   "userId": "3fa2061b-a51e-413b-3cd3-08ddeb855179",  
*   "accessToken": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUHJhbmF2IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzZmEyMDYxYi1hNTFlLTQxM2ItM2NkMy0wOGRkZWI4NTUxNzkiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsImV4cCI6MTc1NzA1OTA2NCwiaXNzIjoiTXlBcHAiLCJhdWQiOiJNeUFwcF9Vc2VycyJ9.E1nC9AqTkMJfrfZEpTVD\_j\_8NMaAnoWBN5MViT9Hx5U9HAT\_jHx6WVF1dKC8Un5dWfYABZKgrIE6e1Jza9GdRQ",  
*   "refreshToken": "YalGJB5phe/4u5AtStEQOtDq1oejhpCG+2aZ2h/7vYI="  
* }  
* 

  **Calling a Protected Endpoint**  
  curl \-X GET https://localhost:5001/api/secure/hello \\  
    \-H "Authorization: Bearer eyJhbGciOiJIUzI1..."

## **🛠 Tech Stack**

* ASP.NET Core 8.0  
* C\#  
* JWT Bearer Authentication  
* Scalar / cURL for testing
