# Light.Results

*A lightweight .NET library implementing the Result Pattern where each result is serializable and deserializable. Comes
with integrations for ASP.NET Core Minimal APIs and MVC, `HttpResponseMessage`, and CloudEvents JSON format.*

[![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)](https://github.com/feO2x/Light.Results/blob/main/LICENSE)
[![NuGet](https://img.shields.io/badge/NuGet-1.0.0-blue.svg?style=for-the-badge)](https://www.nuget.org/packages/Light.Results/0.1.0/)
[![Documentation](https://img.shields.io/badge/Docs-Changelog-yellowgreen.svg?style=for-the-badge)](https://github.com/feO2x/Light.Results/releases)

## ✨ Key Features

- 🧱 **Simple result model** — a `Result` / `Result<T>` is either a success value or one or more errors.
- 📝 **Structured errors** — errors can include message, code, target, category, and metadata.
- 🗂️ **Serializable metadata system** — metadata uses dedicated JSON-like types (instead of `Dictionary<string, object>`) so results stay reliably serializable.
- 🔁 **Functional helpers included** — common operations like `Map`, `Bind`, `Match`, and `Tap` are built in.
- 🌐 **HTTP support** — results can be serialized/deserialized for HTTP, including RFC-9457 / RFC-7807 Problem Details style payloads.
- ☁️ **CloudEvents JSON support** — results can be read/written for asynchronous messaging scenarios with CloudEvents Spec 1.0.
- 🧩 **ASP.NET Core integration** — dedicated packages for Minimal APIs and MVC allow you to easily transform `Result` / `Result<T>` to HTTP responses, supporting RFC-9457 / RFC-7807 Problem Details.
- ⚡ **Performance-oriented** — designed for minimal overhead using fast conversions and minimal allocations to reduce GC pressure.

## 📦 Installation

Install only the packages you need for your scenario.

- Core Result Pattern, Metadata, Functional Operators, and serialization support for HTTP and CloudEvents:

```bash
dotnet add package Light.Results
```

- ASP.NET Core Minimal APIs integration with support for Dependency Injection and `IResult`:

```bash
dotnet add package Light.Results.AspNetCore.MinimalApis
```

- ASP.NET Core MVC integration with support for Dependency Injection and `IActionResult`:

```bash
dotnet add package Light.Results.AspNetCore.Mvc
```

If you only need the Result Pattern itself, install `Light.Results` only.

## 🚀 HTTP Quick Start

### Minimal APIs

```csharp
using System;
using System.Collections.Generic;
using Light.Results;
using Light.Results.AspNetCore.MinimalApis;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLightResultsForMinimalApis();

var app = builder.Build();

app.MapPut("/users/{id:guid}", (Guid id, UpdateUserDto dto) =>
{
	var result = UpdateUser(id, dto);
	return result.ToMinimalApiResult(); // LightResult<T> implements IResult
});

app.Run();

static Result<UserDto> UpdateUser(Guid id, UpdateUserDto dto)
{
	List<Error> errors = [];

	if (id == Guid.Empty)
	{
		errors.Add(new Error
		{
			Message = "User id must not be empty",
			Code = "user.invalid_id",
			Target = "id",
			Category = ErrorCategory.Validation
		});
	}

	if (string.IsNullOrWhiteSpace(dto.Email))
	{
		errors.Add(new Error
		{
			Message = "Email is required",
			Code = "user.email_required",
			Target = "email",
			Category = ErrorCategory.Validation
		});
	}

	if (errors.Count > 0)
	{
		return Result<UserDto>.Fail(errors.ToArray());
	}

	var response = new UserDto
	{
		Id = id,
		Email = dto.Email
	};

	return Result<UserDto>.Ok(response);
}

public sealed record UpdateUserDto
{
	public string? Email { get; init; }
}

public sealed record UserDto
{
	public Guid Id { get; set; }
	public string Email { get; init; } = string.Empty;
}
```

### MVC

```csharp
using System;
using System.Collections.Generic;
using Light.Results;
using Light.Results.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("users")]
public sealed class UsersController : ControllerBase
{
	[HttpPut("{id:guid}")]
	public LightActionResult<UserDto> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
	{
		var result = ValidateAndUpdate(id, dto);
		return result.ToMvcActionResult();
	}

	private static Result<UserDto> ValidateAndUpdate(Guid id, UpdateUserDto dto)
	{
		List<Error> errors = [];

		if (id == Guid.Empty)
		{
			errors.Add(new Error
			{
				Message = "User id must not be empty",
				Code = "user.invalid_id",
				Target = "id",
				Category = ErrorCategory.Validation
			});
		}

		if (string.IsNullOrWhiteSpace(dto.Email))
		{
			errors.Add(new Error
			{
				Message = "Email is required",
				Code = "user.email_required",
				Target = "email",
				Category = ErrorCategory.Validation
			});
		}

		if (errors.Count > 0)
		{
			return Result<UserDto>.Fail(errors.ToArray());
		}

		return Result<UserDto>.Ok(new UserDto
		{
			Id = id,
			Email = dto.Email!
		});
	}
}

public sealed record UpdateUserDto
{
	public string? Email { get; set; }
}

public sealed record UserDto
{
	public Guid Id { get; init; }
	public string Email { get; init; } = string.Empty;
}
```

MVC setup in `Program.cs`:

```csharp
builder.Services.AddControllers();
builder.Services.AddLightResultsForMvc();

var app = builder.Build();
app.MapControllers();
```

### HTTP Response On the Wire

For both examples above (Minimal APIs and MVC), the HTTP response shape is the same.

Successful update (`200 OK`):

```http
PUT /users/6b8a4dca-779d-4f36-8274-487fe3e86b5a
Content-Type: application/json

{
	"email": "ada@example.com"
}
```

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
	"id": "6b8a4dca-779d-4f36-8274-487fe3e86b5a",
	"email": "ada@example.com"
}
```

Validation failure (`400 Bad Request`):

```http
PUT /users/00000000-0000-0000-0000-000000000000
Content-Type: application/json

{}
```

```http
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{
	"type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
	"title": "Bad Request",
	"status": 400,
	"detail": "One or more validation errors occurred.",
    // By default, we use ASP.NET Core compatible errors format here for backwards-compatibility.
    // We encourage you to use ValidationProblemSerializationFormat.Rich instead!
    "errors": {
		"id": ["User id must not be empty"],
		"email": ["Email is required"]
	},
	"errorDetails": [
		{
			"target": "id",
			"index": 0,
			"code": "user.invalid_id",
			"category": "Validation"
		},
		{
			"target": "email",
			"index": 0,
			"code": "user.email_required",
			"category": "Validation"
		}
	]
}
```

### Deserializing Result<T> back from HttpResponseMessage

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Json;
using Light.Results;
using Light.Results.Http.Reading;

using var httpClient = new HttpClient
{
	BaseAddress = new Uri("https://localhost:5001")
};

var request = new UpdateUserDto { Email = "ada@example.com" };

using var response = await httpClient.PutAsJsonAsync(
	"/users/6b8a4dca-779d-4f36-8274-487fe3e86b5a",
	request
);

Result<UserDto> result = await response.ReadResultAsync<UserDto>();

if (result.IsValid)
{
	Console.WriteLine($"Updated user: {result.Value.Email}");
}
else
{
	foreach (var error in result.Errors)
	{
		Console.WriteLine($"{error.Target}: {error.Message}");
	}
}
```

## ☁️ CloudEvents Quick Start

The following example uses `RabbitMQ.Client` to publish and consume a CloudEvents JSON message carrying `Result<UserDto>`.

### Publish to RabbitMQ

```csharp
using System;
using Light.Results;
using Light.Results.CloudEvents;
using Light.Results.CloudEvents.Writing;
using RabbitMQ.Client;

var factory = new ConnectionFactory { HostName = "localhost" };
await using var connection = await factory.CreateConnectionAsync();
await using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queue: "users.updated", durable: true, exclusive: false, autoDelete: false);

var result = Result<UserDto>.Ok(new UserDto
{
	Id = Guid.Parse("6b8a4dca-779d-4f36-8274-487fe3e86b5a"),
	Email = "ada@example.com"
});

byte[] cloudEvent = result.ToCloudEvent(
	successType: "users.updated",
	failureType: "users.update.failed",
	source: "urn:light-results:sample:user-service",
	subject: "users/6b8a4dca-779d-4f36-8274-487fe3e86b5a"
);

var properties = new BasicProperties();
properties.ContentType = CloudEventsConstants.CloudEventsJsonContentType;

await channel.BasicPublishAsync(
	exchange: "",
	routingKey: "users.updated",
	mandatory: false,
	basicProperties: properties,
	body: cloudEvent
);
```

### Consume from RabbitMQ

```csharp
using Light.Results;
using Light.Results.CloudEvents.Reading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "localhost" };
await using var connection = await factory.CreateConnectionAsync();
await using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queue: "users.updated", durable: true, exclusive: false, autoDelete: false);

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (_, eventArgs) =>
{
	Result<UserDto> result = eventArgs.Body.ReadResult<UserDto>();

	if (result.IsValid)
	{
		Console.WriteLine($"Updated user: {result.Value.Email}");
	}
	else
	{
		foreach (var error in result.Errors)
		{
			Console.WriteLine($"{error.Target}: {error.Message}");
		}
	}

	await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
};

await channel.BasicConsumeAsync(queue: "users.updated", autoAck: false, consumer: consumer);
```
