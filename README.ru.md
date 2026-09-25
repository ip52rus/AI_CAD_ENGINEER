# AI CAD ENGINEER

![Autodesk Inventor 2027](https://img.shields.io/badge/Autodesk%20Inventor-2027-0696D7?style=for-the-badge&logo=autodesk&logoColor=white)
![Visual Studio Community 2026](https://img.shields.io/badge/Visual%20Studio%20Community-2026-5C2D91?style=for-the-badge&logo=visualstudio&logoColor=white)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C%23](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Windows 10](https://img.shields.io/badge/Windows-10-0078D4?style=for-the-badge&logo=windows&logoColor=white)
![Git](https://img.shields.io/badge/Git-F05032?style=for-the-badge&logo=git&logoColor=white)
![GitHub](https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white)
![OpenAI](https://img.shields.io/badge/ChatGPT%20%2F%20Codex-OpenAI-412991?style=for-the-badge&logo=openai&logoColor=white)

**Язык:** Русский · [English](README.md)

**Экспериментальная платформа автоматизации Autodesk Inventor для AI-assisted CAD workflows.**

AI CAD ENGINEER — проект на C#/.NET, который предоставляет структурированный JSON-интерфейс для чтения состояния Autodesk Inventor и выполнения детерминированных CAD-операций из внешнего AI-агента или другого клиента автоматизации.

Проект также исследовал более сложную задачу: сможет ли LLM, получив такой уровень управления Inventor, автономно создавать производственные чертежи человеческого качества. Программный слой автоматизации подтвердил свою состоятельность; гипотеза о полностью автономном AI-конструкторе не показала достаточной переносимости между разными классами деталей и сборок.

## Статус проекта

**Финальный исследовательский checkpoint: v0.68**

- **219** зарегистрированных JSON-команд
- **219** уникальных имён команд
- live E2E-проверки в Autodesk Inventor 2027 на протяжении разработки
- архитектура: внешнее reasoning + атомарные **Eyes** и **Hands**
- контроль целостности исходных CAD-документов встроен в процесс разработки
- исследовательский трек автономной генерации чертежей завершён после реальных benchmark-экспериментов

Проект был остановлен не потому, что Inventor оказалось невозможно полноценно управлять извне. Наоборот, было доказано, что Runtime способен читать и изменять широкий набор объектов Inventor. Ограничение проявилось на более высоком уровне — в устойчивости AI при выборе видов, обеспечении размерной полноты, композиции листа и визуальном качестве чертежа.

## Архитектура

```text
Внешняя LLM / клиент автоматизации
                │ JSON
                ▼
            Program.cs
                ▼
        Core/Application.cs
                ▼
    InventorCommandDispatcher
                ▼
         IInventorCommand
                ▼
     CommandSupport / ReadSupport
                ▼
        Autodesk Inventor API
```

### Eyes

Атомарные read-only операции, которые возвращают фактические данные Inventor:

- документы и листы;
- виды и связи между видами;
- DrawingCurve и ссылки на модель;
- размеры и допуски;
- рамки и основные надписи;
- примечания, символы, позиции и таблицы;
- model features и параметры;
- BRep-тела, грани и рёбра;
- эскизы и work features;
- сборки, occurrences, ссылки, constraints и BOM;
- превью модели и чертёжного листа;
- нормализованную карту layout листа.

### Hands

Одна явно заданная операция Inventor на команду:

- создание, перемещение и удаление видов;
- создание разрезов, выносных и дополнительных видов;
- создание и редактирование размеров;
- создание центровых и осевых;
- создание и редактирование примечаний и символов;
- изменение полей основной надписи;
- работа с таблицами и PartsList;
- управление видимостью occurrence внутри конкретного DrawingView;
- сохранение и экспорт;
- атомарное редактирование ячеек и ширины столбцов CustomTable.

Внешний агент определяет **что нужно сделать**. Runtime отвечает только за **корректное выполнение конкретного Inventor API-действия**.

## Почему архитектура изменилась

В ранних версиях проекта существовали встроенные `EngineeringBrain`, `DrawingManager`, логика оценки видов и принятия решений по размерам.

На этапе v0.15 эта архитектура была сознательно удалена.

Причина: решения вида «какой вид лучший», «какой размер обязателен» и «как правильно скомпоновать лист» слишком сильно зависят от design intent, технологии изготовления и визуального инженерного суждения. Жёстко кодировать такую логику внутри Runtime оказалось архитектурно невыгодно.

Новая схема:

```text
LLM reasoning
    ↓
факты от Eyes
    ↓
явный план действий
    ↓
атомарные Hands
    ↓
Inventor
```

Тег `v0.15-before-cleanup` сохраняет раннюю архитектуру в Git history.

## Реализованные возможности

На v0.68 dispatcher содержит **219 уникальных JSON-команд**. Основные группы:

- подключение к Inventor и работа с документами;
- создание и изменение листов;
- рамки и GOST title block;
- жизненный цикл drawing views;
- section/detail/auxiliary views;
- view breaks;
- чтение DrawingCurve;
- linear/diameter/radius/angular/ordinate/baseline/chain dimensions;
- форматирование размеров, precision, styles, layers и tolerance modes;
- center marks и centerlines;
- hole/thread notes;
- general notes и leader notes;
- feature control frames;
- surface texture symbols;
- welding symbols;
- sketched symbols;
- revision clouds и revision tables;
- edge, transition, bend, chamfer и punch annotations;
- balloons и PartsLists;
- CustomTables и HoleTables;
- экспорт PDF, DWG и DXF;
- PNG-превью листа и модели;
- drawing layout map;
- model feature / parameter / sketch / BRep Eyes;
- assembly occurrence / BOM / reference Eyes;
- чтение referenced part из контекста сборки;
- управление occurrence visibility внутри DrawingView;
- атомарное редактирование ячеек CustomTable и ширины столбцов.

Полный список команд: [docs/COMMAND_REFERENCE.md](docs/COMMAND_REFERENCE.md)

Карта верификации возможностей: [CAPABILITY_MAP.md](CAPABILITY_MAP.md)

## Примеры JSON-команд

Получить активный документ:

```json
{"command":"get_active_document"}
```

Получить occurrences активной сборки:

```json
{"command":"get_assembly_occurrences"}
```

Скрыть occurrence только внутри одного drawing view:

```json
{
  "command":"set_drawing_view_occurrence_visibility",
  "sheetName":"Лист:1",
  "viewName":"ВИД1",
  "occurrencePath":"Frame:1/Profile:3",
  "visible":false
}
```

Изменить одну ячейку CustomTable:

```json
{
  "command":"set_custom_table_cell_value",
  "sheetName":"Лист:1",
  "customTableIndex":1,
  "row":1,
  "column":1,
  "value":"D01"
}
```

## Стек разработки

### Продукты и инструменты

Проект разрабатывался и проверялся с использованием:

- **Autodesk Inventor Professional 2027** — целевая CAD-система и среда live E2E
- **Autodesk Inventor API / COM Automation** — программный слой интеграции
- **Visual Studio Community 2026** — основная среда разработки C#
- **.NET SDK 10.0.302** — runtime/toolchain
- **MSBuild 18.6.11** — проверенная система сборки
- **Windows 10 (10.0.19045)** — ОС разработки и запуска Inventor
- **Git / GitHub** — контроль версий, milestones и история проекта
- **ChatGPT / Codex / Codex CLI** — AI-assisted архитектура, исследование, итерации кода и эксперименты с Inventor workflow

AI-инструменты использовались в процессе разработки и исследований, но не являются обязательной встроенной зависимостью Runtime. Основная граница интеграции приложения — JSON-команды.

### Языки и форматы

- **C#** — основной язык реализации
- **JSON** — протокол команд и ответов между внешним агентом и Runtime
- **PowerShell** — build/test/automation сценарии
- **Markdown** — архитектурная, исследовательская и проектная документация

### Основные технологии

- **.NET 10**
- **Autodesk Inventor COM interop**
- **Autodesk Inventor object model / API**
- **Windows COM / Running Object Table**
- **OpenAI Responses API** — экспериментальный адаптер присутствует в репозитории, но не является обязательной частью активной Runtime-архитектуры

## Сборка и запуск

Проверенная среда:

- Windows 10
- Autodesk Inventor Professional 2027
- .NET 10
- Visual Studio / classic MSBuild
- Autodesk Inventor COM reference

Проверенный путь сборки:

```powershell
MSBuild.exe AI_CAD_ENGINEER.csproj /p:Configuration=Debug /p:Platform="Any CPU" /v:minimal
```

Интерактивный режим:

```powershell
AI_CAD_ENGINEER.exe
```

Single-command режим:

```powershell
AI_CAD_ENGINEER.exe --json-file ".\command.json"
```

или:

```powershell
AI_CAD_ENGINEER.exe --json "{\"command\":\"ping\"}"
```

В single-command режиме приложение подключается к уже запущенному экземпляру Inventor и возвращает один JSON-ответ с детерминированным exit code.

## Процесс разработки

```text
Capability Audit
      ↓
Нужен ли вообще новый код?
      ↓
минимальный generic Eye / Hand
      ↓
classic MSBuild
      ↓
live Inventor E2E
      ↓
direct readback
      ↓
проверка source-model integrity
      ↓
checkpoint / tag
```

Если Capability Audit показывал, что нужная возможность уже существует, новый код не писался.

Подробнее: [docs/DEVELOPMENT_PROCESS.md](docs/DEVELOPMENT_PROCESS.md)

## Исследовательские benchmark-эксперименты

### Benchmark #1 — вал

На реальной токарной детали был проверен reference-aided подход: фактический model dossier, реальные чертежи деталей того же класса, manufacturing requirements, несколько candidate plans, выполнение и visual QA.

Содержательная структура чертежа существенно улучшилась, но качественная итоговая композиция всё равно потребовала человеческой визуальной коррекции.

### Benchmark #2 — сварной каркас кресла

Второй benchmark проверял переносимость подхода на многоуровневую сборку из профильной трубы.

Runtime восстановил, в частности:

- 26 конструктивных металлических occurrences;
- 24 трубы + 2 пластины;
- 14 изготовительных типов деталей;
- 12 × Ø9 сквозных отверстий под болты M8;
- 12 × Ø11,1 отверстий в одной стенке под резьбовые заклёпочные гайки M8;
- торцы 5°, 10°, 45° и прямые;
- расхождения между Frame Generator `B_L` и финальной BRep-геометрией;
- четыре производственные единицы: левая боковина, правая боковина, сиденье и спинка.

Многоуровневая архитектура документации была спланирована и технически реализуема через Runtime. Однако первый реальный комплект листов снова потребовал существенной доработки выбора видов, размерной полноты и layout.

Именно это стало stop-criterion исходного автономного drawing-трека.

Подробнее: [docs/RESEARCH_FINDINGS.md](docs/RESEARCH_FINDINGS.md)

## Что доказал проект

Проект **доказал**, что внешнему AI/automation-процессу можно предоставить широкий, структурированный и проверяемый слой управления Autodesk Inventor.

Проект **не доказал**, что текущая LLM способна стабильно заменить опытного конструктора/чертёжника при выпуске произвольной производственной КД без существенного контроля.

Это два разных результата.

Runtime остаётся полезной основой для:

- CAD copilot;
- запросов к модели и сборке;
- drawing/model audit;
- извлечения fabrication data;
- batch automation Inventor;
- supervised drawing assistant;
- агентного управления повторяющимися CAD-операциями.

## Структура репозитория

```text
AI/
Core/
InventorControl/
  Commands/
  InventorCommandDispatcher.cs

AGENTS.md
ARCHITECTURE.md
CAPABILITY_MAP.md
CURRENT_STATE.md
ESKD_DRAWING_POLICY.md
PROJECT_REVIEW.md
ROADMAP.md
CHANGELOG.md
CONTRIBUTING.md

docs/
  COMMAND_REFERENCE.md
  DEVELOPMENT_PROCESS.md
  PROJECT_HISTORY.md
  RESEARCH_FINDINGS.md
  MILESTONES.md
```

## Правила для AI-агентов

[AGENTS.md](AGENTS.md) — основной набор правил для coding/CAD agents.

Ключевые принципы:

- Runtime = factual Eyes + atomic Hands;
- сначала аудит существующих capabilities;
- не придумывать design intent;
- final BRep — основной источник финальной геометрии;
- не менять source model как fallback;
- capability считается VERIFIED только после live Inventor E2E;
- успешный API-вызов не означает качественный чертёж.

## ESKD policy

[ESKD_DRAWING_POLICY.md](ESKD_DRAWING_POLICY.md) содержит правила ЕСКД/ГОСТ, применявшиеся внешним reasoning-layer в ходе экспериментов.

Эти правила намеренно не встроены как автоматическая логика Runtime.

## История версий

- v0.1–v0.12 — embedded analysis / Engineering Brain experiments
- `v0.15-before-cleanup` — checkpoint перед архитектурной очисткой
- v0.15 — удалён legacy decision layer
- v0.16–v0.64 — систематическое расширение Eyes/Hands
- v0.65–v0.66 — referenced-part targeting из сборок
- v0.67 — DrawingView occurrence visibility
- v0.68 — атомарное редактирование CustomTable

См. [CHANGELOG.md](CHANGELOG.md), [docs/PROJECT_HISTORY.md](docs/PROJECT_HISTORY.md) и [docs/MILESTONES.md](docs/MILESTONES.md).

## Ограничения

- для работы требуется Autodesk Inventor;
- архитектура зависит от Windows/COM;
- часть команд проверена только в определённых Inventor object/context combinations;
- несколько legacy analysis-команд сохранены для совместимости;
- engineering intent вроде допусков, точных требований к сварке, крепежу и официальных обозначений нельзя безопасно вывести только из геометрии;
- качество чертежей требует инженерной и визуальной проверки.

## Лицензия

Отдельная open-source лицензия пока не выбрана. Публичная доступность репозитория сама по себе не задаёт права повторного использования.
