# Contributing – Wavebreak SMS Scheduler Integration Pack

Thank you for your interest in contributing to the Wavebreak SMS Scheduler Integration Pack!  
This project is structured to be clear, modular, and easy to extend.  
Below are guidelines to help ensure consistency and maintain high-quality contributions.

------------------------------------------------------------
## How to Contribute

### 1. Fork the Repository
Create your own fork of the project on GitHub and clone it locally.

### 2. Create a Feature Branch
Use descriptive names such as:
- `feature/telnyx-client`
- `feature/blob-contact-loader`
- `fix/retry-logic`
- `docs/update-readme`

Example:
```
git checkout -b feature/telnyx-client
```

### 3. Make Your Changes
Follow the existing project structure:

- **src/** → All C# code  
- **docs/** → Documentation  
- **examples/** → Sample files  
- **CHANGELOG.md** → Update when your feature is ready  

Add comments where needed and keep code clean, readable, and aligned with existing patterns.

------------------------------------------------------------
## Coding Standards

### Use Dependency Injection
All services should be registered and resolved via DI in `Program.cs`.

### Follow Existing Abstractions
When adding new SMS providers, loaders, or utilities:
- Implement `ISmsClient` for new SMS providers.
- Implement `IContactLoader` for new contact data sources.

### Maintain Async/Await Patterns
All operations (network, file, retry logic) should remain async to ensure good performance.

### Keep Logging Consistent
Use the provided `ILogger` patterns:
- Log at the start and end of major operations.
- Log retry attempts.
- Log provider responses and errors.

------------------------------------------------------------
## Adding a New SMS Provider

1. Create a new class implementing `ISmsClient`.
2. Add configuration fields to `SmsSettings` if needed.
3. Register your new provider in `Program.cs`:
```
services.AddHttpClient<ISmsClient, TelnyxSmsClient>();
```
4. Update documentation in:
- `README.md`
- `config-reference.md`
- `component-reference.md`

5. Add a CHANGES entry in `CHANGELOG.md`.

------------------------------------------------------------
## Adding a New Contact Loader

1. Create a class implementing `IContactLoader`.
2. Register it in `Program.cs`.
3. Update documentation in:
- `component-reference.md`
- `config-reference.md`

4. Place examples in `/examples/`.

------------------------------------------------------------
## Updating Documentation

Every new feature must update relevant documentation files in `/docs/`.

This project values strong documentation—treat docs as part of the code.

------------------------------------------------------------
## Testing

When adding new logic:
- Include example inputs in `/examples/`
- Add test scaffolding in `/tests/` (optional for now)
- Manual testing steps should be added to documentation

------------------------------------------------------------
## Submitting Pull Requests

1. Ensure your code builds cleanly.
2. Update documentation.
3. Update `CHANGELOG.md`.
4. Push to your branch.
5. Open a Pull Request with:
   - A clear title
   - Description of changes
   - Why the change is needed
   - Screenshots or logs if helpful

------------------------------------------------------------
## Code of Conduct

- Be respectful and constructive.
- Help maintain a professional tone.
- Focus on clarity, quality, and collaboration.

------------------------------------------------------------
## Thank You

Your contributions help make the Wavebreak SMS Scheduler Integration Pack a polished, professional-quality reference project.

Happy coding!
