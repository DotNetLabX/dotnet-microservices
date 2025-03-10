
# ArticleHub Microservice Specification

## Overview
The **ArticleHub Microservice** is designed to serve as the central hub for tracking and managing the progress of articles across different stages, from submission through review to publication. It will expose relevant data to consumers and handle integration events related to article lifecycle changes (e.g., submission, review, publication). Data will be stored in a **PostgreSQL** database, and the microservice will use **EF Core** for ORM operations.

## Key Responsibilities
1. **Handle Article Lifecycle:**
   - Manage and track article states (e.g., Submitted, Reviewed, Published).
   - Respond to events such as `ArticleSubmittedEvent`, `ArticleReviewedEvent`, `ArticlePublishedEvent`, etc.
   - Update article metadata (e.g., status, assigned authors, timestamps).

2. **Event Handling:**
   - Consume integration events from other microservices (e.g., Submission, Review, Production).
   - Use **EF Core** to update the database based on these events.
   - Ensure that state transitions, validations, and business rules are applied correctly.

3. **Exposing Data:**
   - The microservice will expose article data for querying through **Hasura** (GraphQL).
   - Hasura will provide real-time access to article data, including basic information like article ID, title, status, and other attributes.
   - The microservice will provide real-time updates via **GraphQL subscriptions**.

4. **Database Structure:**
   - The microservice will use **PostgreSQL** for data storage, with entities like **Article**, **Journal**, **Actor**, etc.
   - The **Article** table will store details like:
     - `id`, `title`, `status`, `submission_date`, `review_date`, `publication_date`.
   - The **Actor** table will store roles associated with each article (e.g., author, reviewer, editor).
   - Relationships between tables (e.g., articles and actors) will be modeled using **EF Core**.

5. **API Design:**
   - **Query API**: Expose article information through **Hasura** using GraphQL.
     - Example queries: `GetArticle`, `GetArticlesByStatus`, `GetArticleById`.
     - Subscriptions for real-time updates on article status changes.
   - **Mutation API**: Handle updates via integration events.
     - Event listeners will update the article status or metadata.
   - **Role-Based Access Control**: Ensure that only authorized roles can access specific data.
     - Permissions for reading articles and interacting with data are managed through **Hasura**.

6. **Integration with Other Microservices:**
   - The **ArticleHub** microservice will consume events from other microservices such as:
     - **Submission Service**: Handles article submissions, which trigger the `ArticleSubmittedEvent`.
     - **Review Service**: Marks articles as reviewed, triggering the `ArticleReviewedEvent`.
     - **Production Service**: Marks articles as published, triggering the `ArticlePublishedEvent`.
   - The **ArticleHub** service will process these events and update the article status accordingly.

7. **Data Flow and Event Processing:**
   - Upon receiving an event, the **ArticleHub** will:
     1. Validate the event and business logic.
     2. Use **EF Core** to update the **Article** entity in the database.
     3. Trigger any necessary workflows (e.g., notifying the frontend of an article’s status change).
   
8. **Event Sourcing/Tracking:**
   - Track article status and history for auditing purposes.
   - Each article transition (e.g., from "Draft" to "Submitted") will be recorded.

9. **Resilience and Scalability:**
   - The microservice will handle retries, dead-letter queues, and error handling for integration events.
   - It will be scalable to handle high read-to-write ratios, supporting a 99/1 ratio (read-heavy workloads).

## Key Components

### Entities & Models
- **Article**:
  - `Id`: Unique identifier.
  - `Title`: Title of the article.
  - `Status`: The current status of the article (e.g., Draft, Submitted, Reviewed, Published).
  - `CreatedAt`, `UpdatedAt`: Timestamps for creation and updates.
  - `Authors`: Relationship to the **Actor** entity (authors and roles).
- **Actor**:
  - `Id`: Unique identifier.
  - `PersonId`: Foreign key to the **Person** entity.
  - `Role`: Enum for roles like Author, Reviewer, Editor.

### Event Handling
- Events will be consumed from other services (e.g., **Submission**, **Review**, **Production**).
- Event-driven architecture will be used, leveraging **MediatR** to process incoming events and trigger necessary domain logic.
- Integration events will be used to update the article's status in the database.

### Database Schema
- PostgreSQL tables for **Article**, **Actor**, **Journal**, etc.
- **Hasura** will expose these tables via GraphQL and handle subscriptions.

### Hasura Integration
- **Hasura** will be used for querying and exposing the data, leveraging **GraphQL**.
- It will allow real-time updates (subscriptions) and role-based access control for different users interacting with the articles.

## Workflow Example

### Event: Article Submitted
- The **Submission Service** publishes an `ArticleSubmittedEvent`.
- **ArticleHub** consumes the event, processes the status change, and updates the `Article` entity in PostgreSQL.
- **Hasura** updates in real-time, and consumers can query the updated article status.

### Event: Article Reviewed
- The **Review Service** publishes an `ArticleReviewedEvent`.
- **ArticleHub** listens to the event and updates the article's status to "Reviewed".
- **Hasura** reflects this change for querying.

### Event: Article Published
- The **Production Service** triggers an `ArticlePublishedEvent`.
- **ArticleHub** processes the event and marks the article as "Published".
- **Hasura** updates this status in real-time.

## Technology Stack
- **Backend**: ASP.NET Core Web API with **EF Core** for ORM.
- **Database**: **PostgreSQL**.
- **Event Handling**: **MediatR** for handling integration events.
- **GraphQL**: **Hasura** for exposing data via GraphQL.
- **Real-time**: **Hasura Subscriptions** for real-time updates.
- **Event Messaging**: For integration events, possibly using a message broker like **RabbitMQ** or **Kafka**.

## Summary of Responsibilities
1. **Track article lifecycle states** (Submitted, Reviewed, Published).
2. **Consume integration events** from other microservices.
3. **Expose article data** through **GraphQL** using **Hasura**.
4. **Process business logic** around article submission, review, and publication.
5. **Enable real-time querying** and update notifications for frontend and consumers.





these are the models:
public class Article
{
    public int Id { get; set; }
    public required string Title { get; set; }
      public required string Doi { get; set; }
      public ArticleStage Stage { get; set; }
      public required int JournalId { get; set; }

      public required virtual int SubmitedById { get; set; }
      public virtual Person SubmitedBy { get; set; } = null!;
      public DateTime SubmitedOn { get; set; }

      public DateTime AcceptedOn { get; set; }
      public DateTime? PublishedOn { get; set; }

      public Journal Journal { get; init; } = null!;

      private readonly List<ArticleContributor> _contributors = new();
      public IReadOnlyCollection<ArticleContributor> Contributors => _contributors.AsReadOnly();
}
public class Journal
{
    public int Id { get; init; }
    public required string Abbreviation { get; init; }
      public required string Name{ get; init; }
} 
public class ArticleContributor
{
      public int Id { get; set; }

    public string Affiliation { get; init; } = null!;

      public UserRoleType Role { get; init; }

      public int ArticleId { get; init; }

      public required int PersonId { get; init; }


}
public class Person
{
      public required string FirstName { get; init; }
      public required string LastName { get; init; }
      public string FullName => FirstName + " " + LastName;

      public string? Title { get; init; }
      public required string Email { get; init; }
}
A few questions for you:
- do I need to build any schema for Hasura? or Hasura is crating the schema automatically based on the tables?
- can I execute the query into the this model since it is going to be the same always
- can you build a Query class for the endpoint similar with this one and make sure we are including only the filter part not the fields we want to get?  