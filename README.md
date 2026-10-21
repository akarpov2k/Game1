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
| `src/AnomalyPath.Core/` | правила, данные, репозитории, сервисы, DI — чистый C# без Unity |
| `tests/AnomalyPath.Core.Tests/` | системные тесты Core (консольный прогон) |
| `Assets/Data/` | контент: JSON-сущности и коллекции |
| `Assets/` | Unity-слой: сцены, префабы, UI |

## Сборка и тесты

```powershell
dotnet build AnomalyPath.sln -m:1 -nodeReuse:false -p:UseSharedCompilation=false
dotnet run --project tests\AnomalyPath.Core.Tests -p:UseSharedCompilation=false -- Assets\Data
```

Код возврата `0` — все тесты прошли, `1` — есть падения.

## Проверка данных перед стартом

```csharp
var preflight = Preflight.Run(dataRoot);   // каждый файл читается один раз
if (!preflight.CanStart)
{
    ShowErrors(preflight.Report.Errors);   // список ошибок, игра не стартует
    return;
}

var services = preflight.Services;         // готовый DI-контейнер, повторной загрузки нет
```

Валидатор разбирает все JSON, проверяет сущности, каталог и наборы переводов, собирая все
ошибки за один проход, — чтобы битый контент не всплывал в рантайме.
