# References

These are the sources used for the project. The numbers match the references in the comments and README.

[1] Microsoft, "Session and state management in ASP.NET Core," Microsoft Learn. [Online]. Available: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-10.0. [Accessed: 17-Oct-2026]. Used for server-side sessions, cookie options, cache registration and session middleware.

[2] Microsoft, "Foreign and principal keys in relationships," Microsoft Learn. [Online]. Available: https://learn.microsoft.com/en-us/ef/core/modeling/relationships/foreign-and-principal-keys. [Accessed: 17-Oct-2026]. Used for the composite event/category foreign key and the one-to-one enrolment/result relationship.

[3] Microsoft, "Integration tests in ASP.NET Core," Microsoft Learn. [Online]. Available: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0. [Accessed: 17-Oct-2026]. Used for WebApplicationFactory, real HTTP clients and replacement of the test database provider.

[4] Swashbuckle.AspNetCore maintainers, "Migrating to Swashbuckle.AspNetCore v10," project documentation. [Online]. Available: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/migrating-to-v10.md. [Accessed: 17-Oct-2026]. Used for Swagger UI, XML comments, request schemas and response-type metadata.

[5] Microsoft, "PasswordHasher<TUser> Class," Microsoft Learn. [Online]. Available: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.passwordhasher-1?view=aspnetcore-10.0. [Accessed: 17-Oct-2026]. Used for salted password hashing and verification.

[6] The Independent Institute of Education, "Programming 2B PROG6212/w Portfolio of Evidence paper and marking rubrics," 2026. Assessment brief, poe(2).pdf. Used for the functional requirements and Part 2 rubric.

[7] M. Mamaila, "RaceDay Platform Part 1 Planning and Architecture," Sep. 2026. Part 1 document, progpart1.docx. Used for the six-entity design, original ERD and original 16 API routes.

[8] M. Mamaila, "Cybersecurity Awareness Bot PROG6221 Portfolio of Evidence Part 3," 2026. Earlier project, prog6221-g4-2026-poe-bxredyy-master.zip. Used as a guide for the code and comment style.

[9] Microsoft, "Visual Studio Platform Targeting and Compatibility," Microsoft Learn. [Online]. Available: https://learn.microsoft.com/en-us/visualstudio/releases/2026/compatibility. [Accessed: 17-Oct-2026]. Used to align the revised project with Visual Studio 2026 and .NET 10.

## Sources checked during the reference review

[10] GeeksforGeeks, "Relationships in SQL - One-to-One, One-to-Many, Many-to-Many," Sep. 8, 2025. [Online]. Available: https://www.geeksforgeeks.org/sql/relationships-in-sql-one-to-one-one-to-many-many-to-many/. [Accessed: 18-Oct-2026]. For the explanation of primary keys, foreign keys and table relationships.

[11] GeeksforGeeks, "Role-Based Access Control (RBAC)," May 18, 2026. [Online]. Available: https://www.geeksforgeeks.org/ethical-hacking/role-based-access-control-1/. [Accessed: 18-Oct-2026]. For the explanation of roles, permissions and audit logging.

[12] GeeksforGeeks, "HTTP Status Codes," Oct. 6, 2026. [Online]. Available: https://www.geeksforgeeks.org/computer-networks/what-are-http-status-codes/. [Accessed: 18-Oct-2026]. For the meanings of the HTTP responses returned by the API.

[13] GeeksforGeeks, "Unit Testing," Jun. 24, 2026. [Online]. Available: https://www.geeksforgeeks.org/software-testing/unit-testing-software-testing/. [Accessed: 18-Oct-2026]. For arrange, act and assert, and normal, invalid and boundary test cases. HTTP/database integration test setup is covered separately by [3].

[14] GeeksforGeeks, "ASP.NET Tutorial," Oct. 3, 2025. [Online]. Available: https://www.geeksforgeeks.org/c-sharp/asp-net-tutorial/. [Accessed: 18-Oct-2026]. For general ASP.NET setup and separation of application responsibilities. The .NET 10 details use the Microsoft sources.

[15] GeeksforGeeks, "REST Fundamentals," Oct. 15, 2026. [Online]. Available: https://www.geeksforgeeks.org/node-js/rest-api-introduction/. [Accessed: 18-Oct-2026]. For HTTP methods and resource URLs. This project uses server-side sessions as required by the brief; it does not claim to use stateless authentication.

[16] GeeksforGeeks, "Testing APIs with Swagger," Jul. 23, 2025. [Online]. Available: https://www.geeksforgeeks.org/python/testing-apis-with-swagger/. [Accessed: 18-Oct-2026]. For sending requests and checking responses through Swagger UI. The article example uses Python; RaceDay uses the C# Swashbuckle setup in [4].

[17] Microsoft, "RangeAttribute Class (System.ComponentModel.DataAnnotations)," [Online]. Available: https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.rangeattribute?view=net-10.0. [Accessed: 18-Oct-2026]. For range validation and the invariant-culture options.

[18] Microsoft, "Role-based authorization in ASP.NET Core," [Online]. Available: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-10.0. [Accessed: 18-Oct-2026]. For Authorize role attributes in ASP.NET Core.

[19] .NET Foundation, "WebApplicationFactory.cs," [Online]. Available: https://source.dot.net/Microsoft.AspNetCore.Mvc.Testing/WebApplicationFactory.cs.html. [Accessed: 18-Oct-2026]. For checking the factory disposal order while fixing SQL Server test cleanup.

