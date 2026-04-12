# AtollBlog

A blog site built with [Atoll](https://github.com/damianh/atoll) — the static site generator for .NET.

## Getting Started

Run the development server:

```bash
dotnet run
```

Then open your browser to `http://localhost:5000`.

## Project Structure

```
AtollBlog/
├── AtollBlog.csproj          # Project file
├── Program.cs                 # ASP.NET Core entry point
├── atoll.json                 # Atoll site configuration
├── BlogPostSchema.cs          # Frontmatter schema for blog posts
├── ContentConfig.cs           # Content collection configuration
├── Pages/
│   ├── IndexPage.cs           # Home page (/)
│   ├── AboutPage.cs           # About page (/about)
│   ├── BlogIndexPage.cs       # Blog listing (/blog)
│   └── BlogPostPage.cs        # Individual post (/blog/{slug})
├── Layouts/
│   └── BlogLayout.cs          # Site-wide HTML layout
├── Components/
│   └── PostCard.cs            # Blog post card component
├── Islands/
│   └── ThemeToggle.cs         # Theme toggle island (client:load)
├── Content/
│   └── blog/                  # Markdown blog posts
│       ├── welcome.md
│       └── getting-started.md
└── public/
    └── scripts/
        └── theme-toggle.js    # Client-side JS for ThemeToggle island
```

## Writing Blog Posts

Add Markdown files to `Content/blog/` with YAML frontmatter:

```markdown
---
title: My Post Title
description: A short description.
pubDate: 2026-03-15
author: Your Name
tags: tag1, tag2
draft: false
---

Your content here...
```

Fields:
- `title` — Post title (required)
- `description` — Short summary (required)
- `pubDate` — Publication date in `YYYY-MM-DD` format (required)
- `author` — Author name
- `tags` — Comma-separated tags
- `draft` — Set to `true` to hide from listing

## Customizing

- **Layout**: Edit `Layouts/BlogLayout.cs` to change the site header, footer, or styles
- **Home page**: Edit `Pages/IndexPage.cs`
- **About page**: Edit `Pages/AboutPage.cs`
- **Post card**: Edit `Components/PostCard.cs` to change how posts appear in the listing
- **Theme toggle**: Edit `Islands/ThemeToggle.cs` and `public/scripts/theme-toggle.js`

## Learn More

- [Atoll Documentation](https://github.com/damianh/atoll)
- [Content Collections](https://github.com/damianh/atoll)
- [Islands Architecture](https://github.com/damianh/atoll)
