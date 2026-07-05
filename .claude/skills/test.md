# Run Tests

Run NUnit tests for this repository.

## Test project

`Tests/Tests.csproj` — NUnit 4 tests for `Server/DataBase` (MongoDB repository layer). Uses Mongo2Go for an in-process MongoDB instance, so no external DB is needed.

## Usage

```bash
# Run all tests
dotnet test Tests/Tests.csproj

# Run with verbose output
dotnet test Tests/Tests.csproj --logger "console;verbosity=detailed"

# Filter by test name (substring match)
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~<TestName>"

# Filter by NUnit category
dotnet test Tests/Tests.csproj --filter "TestCategory=<Category>"
```

## What to do

When the user asks to run tests (or says "прогони тесты", "запусти тесты", etc.):

1. Run `dotnet test Tests/Tests.csproj` via the Bash or PowerShell tool.
2. If the user passes a filter argument, append `--filter "FullyQualifiedName~<arg>"`.
3. Report: how many passed, how many failed, and for each failure — the test name and the assertion message.
4. If there are build errors, show the compiler error, not just "build failed".

## Test files

- `Tests/UserRepositoryTests.cs`
- `Tests/ContactRepositoryTests.cs`
- `Tests/MessageRepositoryTests.cs`
