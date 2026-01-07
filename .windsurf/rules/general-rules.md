---
trigger: always_on
---

Light.Results is a lightweight, high-performance library implementing the Result pattern for .NET. It stands out for reducing allocations and being able to serialize and deserialize results across different protocols (HTTP via RFC-9457, gRPC, Asynchronous Messaging). Extensibility is less important than performance.

## General Rules

- `<ImplicitUsings>disable</ImplicitUsings>` - use explicit using statements for clarity
- Light.Results project is built with .NET Standard 2.0, but you can use C# 14 features
- All other projects use .NET 10
