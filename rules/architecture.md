Make sure that when generating code, above points should always be respected.

This file should only be edited by humans. Not by generations!

## Solution architecture

Note that the project has the following architecture:

- Controllers in the Budget.Api receive requests and pass them to the Budget.Application use cases.
    - Controllers should NOT have business logic in them.
    - They can contain request model validation
    - Controllers should always create use cases for POST, PUT and DELETE endpoints. 
    - They can use repositories from Budget.Infrastructure for GET endpoints. 
    - When the controller method is a GET request, it must return a response model from the Models folder in the Api project.
    - When the controller method is POST, PUT or DELETE, it must return the response class from the use case. No need to create custom models in Budget.Api
- Use cases inside the Budget.Application is where the meat of the business logic lives.
    - If any queries need to be used, always use repositories from Budget.Infrastructure.
    - If there's any logic that can be re-used, make sure that logic lives in the corresponding entities from Budget.Domain
    - Entities should NEVER be returned. Always use response classes (which must be created within the same file as the use case class) in combination with the Result class
    - When creating new use cases, you must not create new registrations in the service collection. There's already code that registers all use cases.
- Entities and core domain logic lives in Budget.Domain
    - When creating a new repository, the interface of the repository should live in the `Database/Repositories` directory of this project.
- All query and entity framework logic should reside in Budget.Infrastructure in the Database directory.
    - When creating new repositories, you must not create new registrations in the service collection. There's already code that registers all repositories.
    - Repositories must use the existing BudgetContext. NO need to create a new DbContext.
    - Repositories should always include `SaveChangesAsync`.
- Every new controller method must have at least one test in `Budget.IntegrationTests` in the `ApiTests` directory.
    - When creating a test there, it must have the same setup as the other tests:
    - It must have a `CreateController` method to easily setup the class under test. The method always has a DbContext parameter, as it must be passed.
    - A test can only call `await using var db = _fixture.CreateContext()` once, and `await db.Database.BeginTransactionAsync` MUST be called afterwards.
    - Never ever commit the transaction.
    - When inserting data as part of the arrange, `db.ChangeTracker.Clear();` must be called.
    - Use arrange, act, assert. Act can only contain one statement.
    - Do not create any mocks for any dependencies. For example, when a UseCase class needs a repository, create the actual repository and pass the dbcontext
    - Calls to methods which accept CancellationToken should use TestContext.Current.CancellationToken to allow test cancellation to be more responsive.


## Coding conventions

Here are some coding centions to adhere to:

- The order of the elements in a class: field members, properties, constructors, methods

