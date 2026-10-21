# AnomalyPath — Аномальное путешествие

2D-игра на Unity: картинка-локация, кнопочные действия, пошаговый бой, лут, крафт, репутация.
Сеттинг — постапокалипсис с аномальными зонами.

- Движок: **Unity 6000.6.6f1**, шаблон Universal 2D (URP 17.6, Renderer2D).
- Геймплей и правила: [`docs/GAME_MANIFEST.md`](docs/GAME_MANIFEST.md) — источник истины.
- Схемы данных: [`docs/DATA_SCHEMAS.md`](docs/DATA_SCHEMAS.md).
- Стек, правила работы и история решений: [`AGENTS.md`](AGENTS.md).

## Структура

| Путь | Что там |
| --- | --- |
| `src/AnomalyPath.Core/` | локальный UPM-пакет `com.anomalypath.core`: правила, данные, репозитории, сервисы, DI — без UnityEngine |
| `src/AnomalyPath.Tests/` | пакет `com.anomalypath.tests`: общий `TestSuite` и EditMode-обёртка |
| `tests/AnomalyPath.Core.Tests/` | консольный раннер того же набора тестов |
| `Assets/Scripts/Runtime/` | `GameBootstrap`: preflight перед стартом |
| `Assets/Data/` | контент: JSON-сущности, коллекции, тексты |

## Unity

Пакеты подключены в `Packages/manifest.json` ссылками `file:../src/...` (пути считаются от папки `Packages`),
поэтому Unity компилирует те же исходники, что и `dotnet`, без копий DLL. На загрузочной сцене
`GameBootstrap` вызывает `Preflight.Run` и при ошибках не даёт игре стартовать.

```csharp
var preflight = Preflight.Run(dataRoot);   // каждый файл читается один раз
if (!preflight.CanStart)
{
    ShowErrors(preflight.Report.Errors);   // список ошибок, игра не стартует
    return;
}

var services = preflight.Services;         // готовый DI-контейнер
```

## Сборка и тесты вне Unity

```powershell
dotnet build AnomalyPath.sln -m:1 -nodeReuse:false -p:UseSharedCompilation=false
dotnet run --project tests\AnomalyPath.Core.Tests -p:UseSharedCompilation=false -- Assets\Data
```

Код возврата `0` — все тесты прошли, `1` — есть падения. Артефакты сборки лежат в `.build/`.

## Тесты в Unity EditMode

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.6.6f1\Editor\Unity.exe" -batchmode -projectPath . `
  -runTests -testPlatform EditMode -testResults TestResults.xml -quit
```

Или в редакторе: **Window → General → Test Runner → EditMode**.
