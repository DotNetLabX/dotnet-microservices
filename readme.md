# **.NET Microservices — DDD, Vertical Slice & Clean Architecture**

### **Real-World Reference Project for the Udemy Course**

This repository contains the **complete reference project** used in my Udemy course:

🎓 **Udemy Course**
[.NET 8/9 — Master Microservices with DDD & Vertical Architecture]([https://www.udemy.com/your-course-link-here](https://www.udemy.com/course/net-master-microservices-with-ddd-vertical-architecture/?referralCode=BBB4A0CA199A52D67853))

It represents a **complete MVP** inspired by a real system I partially architected and worked on, combined with patterns and decisions from multiple projects across the last years.

---

## 🚀 **What You’ll Find Here**

A modern backend architecture built around:

### **Core Architectural Patterns**

* Domain-Driven Design (DDD)
* Vertical Slice Architecture
* CQRS (Command/Query separation)
* Clean Architecture (practical variant)
* **Modular Monolith foundation**, with internal reusable modules
* Clear separation between **Modules** and **Microservices**, showing how modules evolve into services
* Event-Driven Design

### **Tech Stack**

* **.NET 8 / .NET 9**
* gRPC for all interservice communication
* EF Core (SQL Server, PostgreSql)
* MongoDB GridFS (File Storage)
* Redis (as Database)
* RabbitMQ (MassTransit)
* Mapster
* FastEndpoints
* MediatR
* YARP API Gateway
* Docker Compose
* Postman
* Internal **Modules** used inside microservices (e.g., FileStorage)

---

## 🧱 **Solution Structure**

```
/src
  /BuildingBlocks
  /Modules
    /ArticleTimeline
    /EmailService
    /FileStorage
  /Services
    /Submission
    /Review
    /Production
    /Journals
    /Auth
    /ArticleHub
  /ApiGateway
/docker-compose
```

Each microservice follows DDD + Vertical Slice + CQRS, with its own domain model, its own persistence layer, and gRPC contracts.

---

## 📚 **How to Use This Repository**

1. Clone the repo
2. Open the main solution in **Visual Studio 2022** or Rider
3. Run `docker-compose up -d`
4. Start the required microservices depending on the module you're studying
5. Open Postman and use the included **Collection & Environment files**
6. Follow the lectures in Udemy for the full walkthrough
7. Explore the **Modules** folder to understand the modular monolith foundation and their integration into microservices

---

## 🧪 **Testing**

* Postman collections included inside the course
* Every feature has a matching handler, request, validator and test scenario
* All services run isolated for easy debugging

---

## 🎓 **Related Material**

Inside the Udemy course you will find:

* Architecture Diagrams (C4 + DDD)
* Module Handbooks (PDF)
* Postman Collections
* Code snippets and detailed explanations

---

## 📌 **Status**

This project will continue to evolve following the course roadmap.
Upcoming additions include:

* Production Microservice deep dive
* API Gateway architecture
* Advanced Eventing & Observability

---

## 🙋‍♂️ **Stay in Touch**

Connect on LinkedIn:
[Laurentiu Dumit]([https://www.linkedin.com/in/your-profile-here](https://www.linkedin.com/in/laurentiu-dumitrescu/)
