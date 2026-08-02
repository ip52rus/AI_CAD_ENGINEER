# AI CAD ENGINEER

AI CAD ENGINEER — консольное приложение на C#, предназначенное для управления Autodesk Inventor через текстовые команды.

## Текущий статус

Версия: `v0.1.0`

На данном этапе программа умеет:

- подключаться к уже запущенному Autodesk Inventor;
- запускать Inventor, если он закрыт;
- получать активный документ;
- определять имя активного документа;
- определять тип документа;
- распознавать текстовую команду без учёта регистра;
- проверять, является ли активный документ деталью.

## Используемые технологии

- C#
- .NET 10
- Autodesk Inventor Professional 2027
- Autodesk Inventor API
- Visual Studio Community 2026
- Git
- GitHub

## Структура проекта

```text
AI_CAD_ENGINEER
│
├── Core
│   ├── Application.cs
│   ├── CommandProcessor.cs
│   └── InventorManager.cs
│
├── Program.cs
├── AI_CAD_ENGINEER.csproj
└── AI_CAD_ENGINEER.slnx