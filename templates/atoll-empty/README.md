# AtollEmpty

A minimal Atoll project scaffold.

## Getting Started

```bash
dotnet run
```

The development server will start at http://localhost:4321.

## Project Structure

```
AtollEmpty/
├── AtollEmpty.csproj    # Project file
├── Program.cs           # ASP.NET Core hosting setup
├── atoll.json           # Atoll configuration
├── Pages/               # Page components (create AtollComponent + IAtollPage)
├── Layouts/             # Layout components (create AtollComponent wrappers)
└── public/              # Static assets served as-is
```

## Creating Your First Page

Add a page in `Pages/Index.cs`:

```csharp
using Atoll.Components;
using Atoll.Routing;

namespace AtollEmpty.Pages;

[Layout(typeof(Layouts.MainLayout))]
public sealed class Index : AtollComponent, IAtollPage
{
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<h1>Hello, Atoll!</h1>");
        return Task.CompletedTask;
    }
}
```

## Learn More

- [Atoll Documentation](https://github.com/example/atoll)
- [Sample Projects](https://github.com/example/atoll/tree/main/samples)
