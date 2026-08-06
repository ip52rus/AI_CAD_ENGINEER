# Package v0.15 — Phase 1 Legacy Cleanup

Пакет удаляет только подтверждённый старый контур:

- `Core\CommandProcessor.cs`
- `Drawing\`
- `Engineering\`
- `Import\Inventor\`
- `Infrastructure\Reporting\`

Текущий JSON-runtime и `InventorControl` не затрагиваются.

## Установка

Распакуй содержимое архива в корень проекта:

```text
C:\AI_CAD_ENGINEER\AI_CAD_ENGINEER
```

Там же должен находиться `AI_CAD_ENGINEER.csproj`.

## 1. Предварительный просмотр

```powershell
powershell -ExecutionPolicy Bypass -File .\Remove-LegacyArchitecture.ps1
```

Скрипт ничего не удалит, а только покажет список.

## 2. Применение

```powershell
powershell -ExecutionPolicy Bypass -File .\Remove-LegacyArchitecture.ps1 -Apply
```

Скрипт проверяет:

- текущая ветка — `cleanup/legacy-architecture`;
- рабочее дерево Git чистое;
- запуск выполнен из корня проекта.

## 3. Сборка

```powershell
dotnet clean
dotnet build
```

## 4. Проверка runtime

```json
{"command":"ping"}
```

```json
{"command":"get_active_document"}
```

```json
{"command":"get_drawing_sheets"}
```

## Откат

До коммита очистки:

```powershell
git restore .
```

После коммита очистки:

```powershell
git reset --hard v0.15-before-cleanup
```
