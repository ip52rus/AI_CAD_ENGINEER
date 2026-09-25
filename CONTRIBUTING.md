# Contributing

Спасибо за интерес к AI CAD ENGINEER.

Проект опубликован как инженерная и исследовательская кодовая база. Наиболее полезны изменения, которые улучшают надёжность Inventor automation, атомарные CAD capabilities, factual readback или supervised workflows.

## Перед изменением

Для существенной новой возможности сначала сформулируйте Capability Audit:

1. Какой реальный workflow заблокирован?
2. Почему существующие команды не решают задачу?
3. Какой минимальный API capability отсутствует?
4. Можно ли обойтись без нового кода?

Если существующего capability достаточно, предпочтителен workflow/documentation fix вместо новой команды.

## Принципы

- минимальный scope;
- один Hand = одно атомарное действие;
- Eye не принимает инженерных решений;
- не добавлять предметно-специфичные команды вида `create_chair_frame_drawing`;
- не придумывать отсутствующий design intent;
- source model не должен неожиданно изменяться;
- BRep является источником конечной геометрии;
- ошибки должны быть явными, без опасных fallback;
- live Inventor E2E обязателен для API changes.

## Build

Проект рассчитан на Windows с установленным Autodesk Inventor.

```powershell
MSBuild.exe AI_CAD_ENGINEER.csproj /p:Configuration=Debug /p:Platform="Any CPU"
```

## Проверка изменений

Минимальный набор:

- build PASS;
- отсутствие новых duplicate registrations, если меняется command registry;
- live Inventor smoke/E2E test;
- исходная модель после read-only / drawing-local workflow остаётся неизменной;
- Git diff содержит только относящиеся к задаче изменения.

## Pull request

В описании PR желательно указать:

- problem / blocker;
- API capability;
- files changed;
- E2E scenario;
- source-model integrity result;
- known limitations.

## CAD test files

Не добавляйте в репозиторий чужие производственные модели, коммерческие чертежи, секретные данные или файлы с неподтверждёнными правами.

## AI-assisted changes

Если код создаётся AI-агентом, агент должен следовать [AGENTS.md](AGENTS.md).

## Лицензия

На момент публикации отдельная лицензия ещё не выбрана. До её добавления не следует интерпретировать публичную доступность репозитория как автоматически предоставленную лицензию на любое внешнее распространение или коммерческое использование.
