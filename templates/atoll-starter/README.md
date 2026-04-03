# AtollStarter

A starter Atoll project with a single index page and a layout. Edit and extend to build your site.

## Getting Started

```bash
dotnet run
```

Then open [http://localhost:4321](http://localhost:4321) in your browser.

## Project Structure

```
AtollStarter/
├── AtollStarter.csproj   # Project file
├── Program.cs            # Entry point — configures the Atoll middleware
├── atoll.json            # Site configuration (base URL, output dir, server port)
├── Pages/
│   └── Index.cs          # Home page at /
├── Layouts/
│   └── MainLayout.cs     # Shared HTML document layout
└── public/               # Static assets (CSS, images, JS)
```

## Adding Pages

Create a new class in `Pages/` that implements `IAtollPage`:

```csharp
[Layout(typeof(Layouts.MainLayout))]
[PageRoute("/about")]
public sealed class About : AtollComponent, IAtollPage
{
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<h1>About</h1>");
        return Task.CompletedTask;
    }
}
```

## Building for Production

```bash
dotnet run -- build
```

Output is written to the `dist/` directory.

## Learn More

- [Atoll Documentation](https://github.com/example/atoll)
- [Islands Architecture](https://github.com/example/atoll/docs/islands)
- [Content Collections](https://github.com/example/atoll/docs/content)
