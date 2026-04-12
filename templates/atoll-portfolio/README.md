# AtollPortfolio

A portfolio/personal site built with [Atoll](https://github.com/damianh/atoll) — the static site generator for .NET.

## Getting Started

Run the development server:

```bash
dotnet run
```

Then open your browser to `http://localhost:5000`.

## Project Structure

```
AtollPortfolio/
├── AtollPortfolio.csproj         # Project file
├── Program.cs                     # ASP.NET Core entry point
├── atoll.json                     # Atoll site configuration
├── Pages/
│   ├── IndexPage.cs               # Home page (/)
│   ├── AboutPage.cs               # About page (/about)
│   ├── ProjectsPage.cs            # Projects listing (/projects)
│   └── ContactPage.cs             # Contact page (/contact)
├── Layouts/
│   └── PortfolioLayout.cs         # Site-wide HTML layout with mobile nav
├── Components/
│   ├── HeroSection.cs             # Hero banner component
│   └── ProjectCard.cs             # Project card component
├── Islands/
│   ├── ContactForm.cs             # Contact form island (client:load)
│   └── MobileNav.cs               # Mobile navigation island (client:media)
└── public/
    └── scripts/
        ├── contact-form.js        # Client-side JS for ContactForm island
        └── mobile-nav.js          # Client-side JS for MobileNav island
```

## Islands Architecture

This template demonstrates three island hydration strategies:

- **`client:load`** (`ContactForm`) — Hydrates immediately on page load. Use for interactive elements needed straight away.
- **`client:media("(max-width: 768px)")`** (`MobileNav`) — Hydrates only when the media query matches. Use for mobile-only features to avoid loading JS on desktop.

See the [Atoll Islands documentation](https://github.com/damianh/atoll) to learn about `client:visible` and other directives.

## Customizing

- **Layout**: Edit `Layouts/PortfolioLayout.cs` to change the header, footer, or styles
- **Home page**: Edit `Pages/IndexPage.cs` and `Components/HeroSection.cs`
- **Projects**: Edit `Pages/ProjectsPage.cs` and `Components/ProjectCard.cs` to add your real projects
- **About page**: Edit `Pages/AboutPage.cs` to add your own bio and skills
- **Contact form**: Edit `Islands/ContactForm.cs` and `public/scripts/contact-form.js` to wire up a real backend
- **Mobile nav**: Edit `Islands/MobileNav.cs` and `public/scripts/mobile-nav.js` to add more nav links

## Learn More

- [Atoll Documentation](https://github.com/damianh/atoll)
- [Islands Architecture](https://github.com/damianh/atoll)
- [Components](https://github.com/damianh/atoll)
