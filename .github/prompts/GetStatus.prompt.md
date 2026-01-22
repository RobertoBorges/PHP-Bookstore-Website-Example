```prompt
---
agent: agent
---

# Get Migration Status

Retrieve and display the current status of the PHP to .NET 10 migration process.

## Rules for Status Tracking

### When Called

1. **Read the status file**: `reports/Report-Status.md`

2. **If the file doesn't exist** (migration not started):
   - Create `reports/Report-Status.md` with initial content
   - Inform user that migration has not started
   - Suggest running `/phase0-applicationdiscovery` to begin

3. **If the file exists**:
   - Summarize the current migration status
   - Show progress across all phases
   - Highlight the current phase and next steps

### Status File Structure

Ensure the status file contains:

```markdown
# Migration Status Report

**Application**: [Name]
**Source**: PHP [Version] / [Framework]
**Target**: .NET 10
**Last Updated**: [Date/Time]

## Overall Progress

| Phase | Status | Completion | Timestamp |
|-------|--------|------------|-----------|
| Phase 0: Application Discovery | ✅/⏳/❌ | X% | [Date] |
| Phase 1: Technical Assessment | ✅/⏳/❌ | X% | [Date] |
| Phase 2: Migration Planning | ✅/⏳/❌ | X% | [Date] |
| Phase 3: Code Migration | ✅/⏳/❌ | X% | [Date] |
| Phase 4: Infrastructure | ✅/⏳/❌ | X% | [Date] |
| Phase 5: Azure Deployment | ✅/⏳/❌ | X% | [Date] |
| Phase 6: CI/CD Setup | ✅/⏳/❌ | X% | [Date] |

**Overall Completion**: [X]%

## Current Phase: [Phase Name]

[Details about current phase status]

## Migration Configuration

| Setting | Value |
|---------|-------|
| .NET Architecture | [MVC/API/Blazor] |
| Frontend Strategy | [Razor/Blazor/SPA] |
| Data Access | [EF Core/Dapper] |
| Authentication | [Identity/Entra ID] |
| Azure Hosting | [App Service/Container Apps/AKS] |
| IaC Tool | [Bicep/Terraform] |
| Database | [Azure SQL/MySQL/PostgreSQL] |

## Phase Summaries

### Phase 0: Application Discovery
- **Status**: [Status]
- **Components Found**: [X] controllers, [X] models, [X] services, [X] views
- **Report**: `reports/Application-Discovery-Report.md`

### Phase 1: Technical Assessment
- **Status**: [Status]
- **Risks Identified**: [X] critical, [X] high, [X] medium
- **Report**: `reports/Technical-Assessment-Report.md`

### Phase 2: Migration Planning
- **Status**: [Status]
- **Files Planned**: [X] files
- **Estimated Effort**: [X] hours
- **Report**: `reports/Migration-Plan-Detailed.md`

### Phase 3: Code Migration
- **Status**: [Status]
- **Files Migrated**: [X] of [X]
- **Business Rules Preserved**: [X] of [X]
- **Build Status**: [Passing/Failing]

### Phase 4: Infrastructure
- **Status**: [Status]
- **IaC Files Created**: [Yes/No]
- **Validation Status**: [Passed/Pending]

### Phase 5: Azure Deployment
- **Status**: [Status]
- **Environment**: [Dev/Staging/Production]
- **Health Check**: [Passing/Failing]

### Phase 6: CI/CD Setup
- **Status**: [Status]
- **Platform**: [GitHub Actions/Azure DevOps]
- **Pipelines Created**: [Yes/No]

## Issues & Blockers

| Issue | Severity | Status | Resolution |
|-------|----------|--------|------------|
| [Issue] | 🔴/🟠/🟡 | Open/Resolved | [Details] |

## Next Steps

1. [Next action with command]
2. [Following action]

## Reports

- [Application Discovery Report](reports/Application-Discovery-Report.md)
- [Technical Assessment Report](reports/Technical-Assessment-Report.md)
- [Detailed Migration Plan](reports/Migration-Plan-Detailed.md)
```

### Status Icons

Use these status indicators:
- ✅ **Complete** - Phase finished successfully
- ⏳ **In Progress** - Currently working on this phase
- ❌ **Blocked** - Cannot proceed due to issues
- ⬜ **Not Started** - Phase not yet begun

### Completion Percentages

Calculate based on phase deliverables:
- Phase 0: Report exists = 100%
- Phase 1: Preferences confirmed + report = 100%
- Phase 2: Migration plan complete = 100%
- Phase 3: (Files migrated / Total files) × 100
- Phase 4: IaC files created + validated = 100%
- Phase 5: Deployed + health check = 100%
- Phase 6: Pipelines created + tested = 100%

### Commands Reference

Provide the appropriate next command:

| Current Phase | Status | Next Command |
|---------------|--------|--------------|
| Not started | - | `/phase0-applicationdiscovery` |
| Phase 0 | Complete | `/phase1-technicalassessment` |
| Phase 1 | Complete | `/phase2-createmigrationplan` |
| Phase 2 | Complete | `/phase3-migratecode` |
| Phase 3 | Complete | `/phase4-generateinfra` |
| Phase 4 | Complete | `/phase5-deploytoazure` |
| Phase 5 | Complete | `/phase6-setupcicd` |
| Phase 6 | Complete | Migration complete! 🎉 |

### Display Format

When presenting status to user:

1. **Show a summary first** - Overall progress percentage and current phase
2. **Highlight next action** - Clear command to run
3. **Show any blockers** - Issues that need resolution
4. **Link to detailed reports** - For more information
```
