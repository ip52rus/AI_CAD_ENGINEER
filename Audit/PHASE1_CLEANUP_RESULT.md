# Phase 1 Legacy Cleanup

Дата: 2026-08-06 17:28:25
Ветка: cleanup/legacy-architecture

## Удалено

- Core\CommandProcessor.cs
- Drawing
- Engineering
- Import\Inventor
- Infrastructure\Reporting

## Причина

Удалён старый программный контур:

CommandProcessor
-> DrawingManager
-> EngineeringBrain
-> Analysis / Decision / Planning

Текущий runtime:

Application
-> InventorCommandDispatcher
-> атомарные JSON-команды
-> Inventor API

Новая архитектура оставляет инженерные решения ИИ,
а приложению — только глаза и руки.

## Следующая проверка

dotnet clean
dotnet build

После успешной сборки проверить:

{"command":"ping"}
{"command":"get_active_document"}
{"command":"get_drawing_sheets"}
