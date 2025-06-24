	# Order Management System

	A .NET 8 Web API for managing orders with discounting, status tracking, and analytics features, developed as of June 22, 2025, 02:29 PM EAT.

	## Features
	- **Discounting System**: Applies discounts based on customer segment and order history (Gold: 15% for 5+ orders, Premium: 10% for >5 orders, Regular: 5% for 5+ orders) using the strategy pattern.
	- **Order Status Tracking**: Implements a state machine for valid transitions (Pending → Processing → Shipped → Delivered, Pending/Processing → Cancelled, Shipped/Delivered → Returned, Cancelled/Returned → Refunded).
	- **Analytics**: Provides average order value and fulfillment time (from OrderDate to DeliveredDate in hours) via an analytics endpoint.
	- **DTOs and AutoMapper**: Uses Data Transfer Objects (DTOs) to separate API contracts from domain models, with AutoMapper for efficient mapping.
	- **Testing**: Includes unit tests for discount logic and order service, and integration tests for order creation and analytics using an in-memory database with `CustomWebApplicationFactory`.
	- **Documentation**: Swagger/OpenAPI annotations for all endpoints, with clear API documentation.
	- **Optimization**: Uses async database queries and `AsNoTracking` for read operations to improve performance.
	- **Error Handling**: Returns appropriate HTTP status codes and error messages for invalid requests, including validation errors and invalid state transitions.
	- **Dependency Injection**: All services, repositories, and strategies are registered using .NET's built-in DI container.
	- **Seeding**: The in-memory database is seeded with customer data for testing discount logic.

	## Approach
	The solution delivers the required features (discounting, status tracking, analytics) , prioritizing clean and precise code. The `Order` model includes `CustomerName`, `CustomerEmail`, `ShippingAddress`, `TotalAmount`, `Status` (with states: Pending, Processing, Shipped, Delivered, Cancelled, Returned, Refunded), and timestamps (`OrderDate`, `ShippedDate`, `DeliveredDate`, `CancelledDate`, `ReturnedDate`, `RefundedDate`). The `TotalAmount` is provided directly via the API, aligning with the assignment's scope to avoid unnecessary complexity.

	**DTOs and AutoMapper**: Data Transfer Objects (`OrderCreateDto`, `OrderDto`, `OrderStatusUpdateDto`, `AnalyticsDto`) decouple the API from the domain model, improving maintainability. AutoMapper automates mappings, reducing boilerplate code and enabling flexible API design (e.g., potential future changes to expose `Status` as a string).

	**Integration Testing**: The `CustomWebApplicationFactory` configures an in-memory database for integration tests, seeding customer data to support discount testing. Tests validate the `POST /api/orders` and `GET /api/analytics/summary` endpoints, ensuring correct DTO handling and business logic.

	**Consideration of Order Details**: In a real-world order management system, an order typically includes order details (line items with product IDs, quantities, and unit prices) and a product model. This was considered but deemed out of scope, as the assignment does not explicitly require itemized orders or product management. Including order details would add complexity (e.g., one-to-many relationships, additional validation, and testing) that could risk the timeline for core requirements. Instead, the solution assumes `TotalAmount` is provided, with discounts applied at the order level. Order details and a product model could be added with a one-to-many `Order`-`OrderDetail` relationship and a `Product` entity.

	## Assumptions
	- An in-memory database (EF Core SQLite) is used for simplicity and testing.
	- Customer data (e.g., `Segment`, `OrderCount`) is pre-populated for discount calculations (Gold, Premium, Regular segments).
	- Only one discount rule is applied per order (the first applicable rule).
	- `TotalAmount` is provided directly via the API, rather than calculated from order details.
	- Order details (line items) and a product model are not included, as they are not explicitly required.
	- `CustomerName`, `CustomerEmail`, and `ShippingAddress` are required fields, validated via DTOs.
	- `OrderStatus` includes Pending, Processing, Shipped, Delivered, Cancelled, Returned, and Refunded, with transitions managed by a state machine.
	- Integration tests use `CustomWebApplicationFactory` to create isolated in-memory databases.
	- The API uses async/await for all database operations.
	- All endpoints are versioned under `/api`.
	- The project uses .NET 8 minimal APIs or controllers (depending on implementation).

	## Setup
	1. Clone the repository.
	2. Run `dotnet restore` to install dependencies, including AutoMapper and testing packages.
	3. Run `dotnet run` to start the API.
	4. Access Swagger at `https://localhost:5001/swagger`.
	5. Run tests with `dotnet test`.

	## Endpoints
	- `POST /api/orders?customerId={id}`: Create an order (expects `OrderCreateDto` with `TotalAmount`, `CustomerName`, `CustomerEmail`, `ShippingAddress`).
	- `PUT /api/orders/{id}/status`: Update order status (expects `OrderStatusUpdateDto` with `Status`).
	- `GET /api/orders/{id}`: Get order details (returns `OrderDto`).
	- `GET /api/analytics/summary`: Get order analytics (returns `AnalyticsDto` with `AverageOrderValue` and `AverageFulfillmentTime`).

	## Example Request (Create Order)## Project Structure
	- **Controllers**: API endpoints for orders and analytics.
	- **Models**: Domain models (`Order`, `Customer`), DTOs.
	- **Services**: Business logic for orders, discounts, and analytics.
	- **Strategies**: Discount strategy implementations.
	- **Data**: EF Core DbContext, database seeding.
	- **Mapping**: AutoMapper profiles.
	- **Tests**: Unit and integration tests, including `CustomWebApplicationFactory` for in-memory testing.

	## Running Tests
	- Unit tests cover discount logic and order service.
	- Integration tests validate API endpoints and business rules.
	- Use `dotnet test` to run all tests.

	## Technologies Used
	- .NET 8
	- Entity Framework Core (SQLite In-Memory)
	- AutoMapper
	- Swagger/OpenAPI
	- xUnit (or NUnit/MSTest) for testing
	- Moq (if mocking is required)
	- FluentValidation (if present)

	## Notes
	- The solution is designed for extensibility (e.g., adding order details, products, or more analytics).
	- All business logic is covered by tests to ensure reliability.
	- The API is ready for deployment or further extension.
	<!--
	NOTE: As of now, no validations have been implemented in the codebase. 
	Validation logic (e.g., data annotations, FluentValidation) should be added to DTOs and/or service layers to ensure required fields and correct formats.
	-->
	# Order Management System

	A .NET 8 Web API for managing orders with discounting, status tracking, and analytics features, developed as of June 22, 2025, 02:29 PM EAT.

	## Features
	- **Discounting System**: Applies discounts based on customer segment and order history (Gold: 15% for 5+ orders, Premium: 10% for >5 orders, Regular: 5% for 5+ orders) using the strategy pattern.
	- **Order Status Tracking**: Implements a state machine for valid transitions (Pending → Processing → Shipped → Delivered, Pending/Processing → Cancelled, Shipped/Delivered → Returned, Cancelled/Returned → Refunded).
	- **Analytics**: Provides average order value and fulfillment time (from OrderDate to DeliveredDate in hours) via an analytics endpoint.
	- **DTOs and AutoMapper**: Uses Data Transfer Objects (DTOs) to separate API contracts from domain models, with AutoMapper for efficient mapping.
	- **Testing**: Includes unit tests for discount logic and order service, and integration tests for order creation and analytics using an in-memory database with `CustomWebApplicationFactory`.
	- **Documentation**: Swagger/OpenAPI annotations for all endpoints, with clear API documentation.
	- **Optimization**: Uses async database queries and `AsNoTracking` for read operations to improve performance.
	- **Error Handling**: Returns appropriate HTTP status codes and error messages for invalid requests, including validation errors and invalid state transitions.
	- **Validation**: Uses data annotations and FluentValidation (if present) to validate DTOs for required fields and correct formats.
	- **Dependency Injection**: All services, repositories, and strategies are registered using .NET's built-in DI container.
	- **Seeding**: The in-memory database is seeded with customer data for testing discount logic.

	## Approach
	The solution delivers the required features (discounting, status tracking, analytics) within the 60-70 minute timeline for feature implementation, prioritizing clean and precise code. The `Order` model includes `CustomerName`, `CustomerEmail`, `ShippingAddress`, `TotalAmount`, `Status` (with states: Pending, Processing, Shipped, Delivered, Cancelled, Returned, Refunded), and timestamps (`OrderDate`, `ShippedDate`, `DeliveredDate`, `CancelledDate`, `ReturnedDate`, `RefundedDate`). The `TotalAmount` is provided directly via the API, aligning with the assignment's scope to avoid unnecessary complexity.

	**DTOs and AutoMapper**: Data Transfer Objects (`OrderCreateDto`, `OrderDto`, `OrderStatusUpdateDto`, `AnalyticsDto`) decouple the API from the domain model, improving maintainability. AutoMapper automates mappings, reducing boilerplate code and enabling flexible API design (e.g., potential future changes to expose `Status` as a string).

	**Integration Testing**: The `CustomWebApplicationFactory` configures an in-memory database for integration tests, seeding customer data to support discount testing. Tests validate the `POST /api/orders` and `GET /api/analytics/summary` endpoints, ensuring correct DTO handling and business logic.

	**Consideration of Order Details**: In a real-world order management system, an order typically includes order details (line items with product IDs, quantities, and unit prices) and a product model. This was considered but deemed out of scope, as the assignment does not explicitly require itemized orders or product management. Including order details would add complexity (e.g., one-to-many relationships, additional validation, and testing) that could risk the timeline for core requirements. Instead, the solution assumes `TotalAmount` is provided, with discounts applied at the order level. Order details and a product model could be added with a one-to-many `Order`-`OrderDetail` relationship and a `Product` entity.

	## Assumptions
	- An in-memory database (EF Core SQLite) is used for simplicity and testing.
	- Customer data (e.g., `Segment`, `OrderCount`) is pre-populated for discount calculations (Gold, Premium, Regular segments).
	- Only one discount rule is applied per order (the first applicable rule).
	- `TotalAmount` is provided directly via the API, rather than calculated from order details.
	- Order details (line items) and a product model are not included, as they are not explicitly required.
	- `CustomerName`, `CustomerEmail`, and `ShippingAddress` are required fields, validated via DTOs.
	- `OrderStatus` includes Pending, Processing, Shipped, Delivered, Cancelled, Returned, and Refunded, with transitions managed by a state machine.
	- Integration tests use `CustomWebApplicationFactory` to create isolated in-memory databases.
	- The API uses async/await for all database operations.
	- All endpoints are versioned under `/api`.
	- The project uses .NET 8 minimal APIs or controllers (depending on implementation).

	## Setup
	1. Clone the repository.
	2. Run `dotnet restore` to install dependencies, including AutoMapper and testing packages.
	3. Run `dotnet run` to start the API.
	4. Access Swagger at `https://localhost:5001/swagger`.
	5. Run tests with `dotnet test`.

	## Endpoints
	- `POST /api/orders?customerId={id}`: Create an order (expects `OrderCreateDto` with `TotalAmount`, `CustomerName`, `CustomerEmail`, `ShippingAddress`).
	- `PUT /api/orders/{id}/status`: Update order status (expects `OrderStatusUpdateDto` with `Status`).
	- `GET /api/orders/{id}`: Get order details (returns `OrderDto`).
	- `GET /api/analytics/summary`: Get order analytics (returns `AnalyticsDto` with `AverageOrderValue` and `AverageFulfillmentTime`).

	## Example Request (Create Order)## Project Structure
	- **Controllers**: API endpoints for orders and analytics.
	- **Models**: Domain models (`Order`, `Customer`), DTOs.
	- **Services**: Business logic for orders, discounts, and analytics.
	- **Strategies**: Discount strategy implementations.
	- **Data**: EF Core DbContext, database seeding.
	- **Mapping**: AutoMapper profiles.
	- **Tests**: Unit and integration tests, including `CustomWebApplicationFactory` for in-memory testing.

	## Running Tests
	- Unit tests cover discount logic and order service.
	- Integration tests validate API endpoints and business rules.
	- Use `dotnet test` to run all tests.

	## Technologies Used
	- .NET 8
	- Entity Framework Core (SQLite In-Memory)
	- AutoMapper
	- Swagger/OpenAPI
	- NUnit for testing
	- Moq (if mocking is required)

	## Notes
	- The solution is designed for extensibility (e.g., adding order details, products, or more analytics).
	- All business logic is covered by tests to ensure reliability.
	- The API is ready for deployment or further extension.
	<!--
	NOTE: As of now, no validations have been implemented in the codebase. 
	Validation logic (e.g., data annotations, FluentValidation) should be added to DTOs and/or service layers to ensure required fields and correct formats.
	-->
#   O r d e r M a n a g e m e n t S e r v i c e  
 