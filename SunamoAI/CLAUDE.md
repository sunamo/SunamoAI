# INSTRUKCE PRO PŘEJMENOVÁNÍ PROMĚNNÝCH

**VŠECHNY instrukce pro pojmenování proměnných samopopisnými názvy najdeš v:**

```
E:\vs\Projects\PlatformIndependentNuGetPackages\CLAUDE.md
```

Přečti si ten soubor před jakoukoliv prací na přejmenování proměnných v tomto projektu!

**KRITICKÉ pravidla (zkrácená verze, plná verze v hlavním CLAUDE.md):**
- ❌ NIKDY nepřidávej komentář `// variables names: ok` - to dělá pouze uživatel
- ❌ NIKDY nepoužívaj doménově specifické názvy (`columnCount`, `rowSize`) pro univerzální parametry → použij `groupSize`, `chunkSize`
- ❌ NIKDY nepoužívaj jednoslovné názvy (`s`, `v`, `l`) → použij `text`, `value`, `list`
- ❌ NIKDY nepoužívaj `item` pro parametry metod → vyhrazeno pro foreach
- ✅ VŽDY maž nepoužívané parametry z hlaviček metod
- ✅ VŽDY dbej na konzistenci v rámci jednoho souboru

---

# PŮVODNÍ OBSAH TOHOTO SOUBORU:

# SunamoAI - Project Instructions

## Purpose
SunamoAI obsahuje **pouze generické AI služby** pro volání různých AI modelů (Claude, Gemini).

## What BELONGS in SunamoAI
✅ Generické AI volání služby:
- `ClaudeCliService` - volání Claude přes CLI (claude.cmd)
- `ClaudeApiService` - volání Claude přes HTTP API
- `GeminiApiService` - volání Gemini API
- Rate limit handling a retry logika
- Generické parsování AI odpovědí

## What DOES NOT BELONG in SunamoAI
❌ Business logika specifická pro konkrétní doménu:
- ~~`FinancialParserService`~~ - patří do PerfectHome.Cmd (nemovitosti)
- ~~`FinancialParseResult`~~ - patří do PerfectHome.Cmd (nemovitosti)
- Domain-specific prompty (např. pro parsování kauce, provize, energií)
- Domain-specific parsovací logika

## Architecture Pattern
```
SunamoAI (generické AI služby)
    ↓ používá
PerfectHome.Cmd (business logika pro nemovitosti)
    - FinancialParserService (volá SunamoAI.ClaudeCliService)
    - Obsahuje prompty specifické pro nemovitosti
    - Obsahuje parsování výsledků (bail, fees, commission)
```

## Dependencies
- `Anthropic.SDK` - pro Claude API
- `Mscc.GenerativeAI` - pro Gemini API
- `Microsoft.Extensions.Logging.Abstractions` - pro logging

## Usage Example
```csharp
// In domain-specific project (e.g., PerfectHome.Cmd):
var claudeService = new ClaudeCliService(logger);
var prompt = BuildDomainSpecificPrompt(data); // Domain logic
var response = await claudeService.CallClaudeCli(prompt);
var result = ParseDomainSpecificResponse(response); // Domain logic
```
