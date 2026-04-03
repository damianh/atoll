---
title: Welcome to Your Atoll Blog
description: You've successfully created an Atoll blog. Here's what to expect.
pubDate: 2026-01-01
author: Blog Author
tags: welcome, atoll
draft: false
---

# Welcome to Your Atoll Blog

Congratulations! You've created a new blog using the Atoll framework.

## What You Get

This blog template includes:

- **Content Collections** — type-safe Markdown content with frontmatter validation
- **Blog listing page** at `/blog` with post cards
- **Individual post pages** at `/blog/{slug}` with full content rendering
- **Theme toggle** island for light/dark mode switching
- A clean, minimal design you can customize

## Writing Posts

Add Markdown files to the `Content/blog/` directory with YAML frontmatter:

```markdown
---
title: My Post Title
description: A short description of the post.
pubDate: 2026-03-15
author: Your Name
tags: tag1, tag2
draft: false
---

# My Post Title

Your content here...
```

## Running the Site

```bash
dotnet run
```

Then open your browser to `http://localhost:5000`.
