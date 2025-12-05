# Changelog – Wavebreak SMS Scheduler Integration Pack

All notable changes to this project will be documented in this file.

This project follows a simple versioning approach suitable for portfolio and integration-pack style repositories.

------------------------------------------------------------
## [1.0.0] – Initial Release
### Added
- Full project scaffold using Azure Functions (.NET Isolated)
- Timer-triggered ScheduledSmsJob with retry logic
- CsvContactLoader and JsonContactLoader implementations
- ISmsClient abstraction with UltimateSmsClient implementation
- SmsSettings configuration model with dry-run mode
- Program.cs host configuration with DI and HttpClient setup
- Example data files (CSV/JSON)
- Documentation:
  - README.md
  - overview.md
  - setup-azure.md
  - config-reference.md
  - component-reference.md

### Notes
- This is the first complete version of the Wavebreak SMS Scheduler Integration Pack.
- Designed to demonstrate enterprise-quality integration architecture.

------------------------------------------------------------
## [Unreleased]
### Planned
- TelnyxSmsClient implementation
- BlobContactLoader for Azure Blob data sources
- FileMakerContactLoader via Data API
- Template-based SMS message rendering
- Metrics export (Databox, Application Insights)
- Unit tests for loaders and retry logic
