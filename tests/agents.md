# Agents.md for Automated Tests

- All test projects use .NET 10 and xunit v3.
- For the assertion phase of a test, always use FluentAssertions.
- Do not write test methods in nested classes. Test classes can have nested types, typically for types that represent test doubles, but test methods should always be placed in a top-level class residing directly in a namespace.
- Try to avoid code duplication across several test methods. Refactor code by using, for example, data-driven tests, test fixtures, or factory methods. Honor the DRY and the Single Point of Truth principles.
