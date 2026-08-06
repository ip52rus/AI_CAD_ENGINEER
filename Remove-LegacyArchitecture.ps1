param(
    [switch]$Apply
)

$ErrorActionPreference = "Stop"

function Fail([string]$Message) {
    Write-Host ""
    Write-Host "ОШИБКА: $Message" -ForegroundColor Red
    exit 1
}

$projectRoot = (Get-Location).Path
$projectFile = Join-Path $projectRoot "AI_CAD_ENGINEER.csproj"

if (-not (Test-Path -LiteralPath $projectFile)) {
    Fail "Запусти скрипт из корня проекта, где находится AI_CAD_ENGINEER.csproj."
}

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Fail "Git не найден."
}

$branch = (git branch --show-current).Trim()

if ($branch -ne "cleanup/legacy-architecture") {
    Fail "Текущая ветка: '$branch'. Ожидалась: 'cleanup/legacy-architecture'."
}

# Проверяем только изменения отслеживаемых файлов.
# Сам скрипт и README могут быть неотслеживаемыми — это не мешает очистке.
git diff --quiet
if ($LASTEXITCODE -ne 0) {
    Fail "Есть незакоммиченные изменения в отслеживаемых файлах. Сначала закоммить или отмени их."
}

git diff --cached --quiet
if ($LASTEXITCODE -ne 0) {
    Fail "Есть подготовленные к коммиту изменения. Сначала закоммить или убери их из индекса."
}

$targets = @(
    "Core\CommandProcessor.cs",
    "Drawing",
    "Engineering",
    "Import\Inventor",
    "Infrastructure\Reporting"
)

Write-Host "=============================================="
Write-Host " AI CAD ENGINEER v0.15 - Phase 1 Cleanup"
Write-Host "=============================================="
Write-Host ""
Write-Host "Ветка: $branch"
Write-Host "Корень: $projectRoot"
Write-Host ""

$existing = @()
$missing = @()

foreach ($target in $targets) {
    $fullPath = Join-Path $projectRoot $target

    if (Test-Path -LiteralPath $fullPath) {
        $existing += $target
    }
    else {
        $missing += $target
    }
}

Write-Host "Будут удалены:" -ForegroundColor Yellow

if ($existing.Count -eq 0) {
    Write-Host "  ничего — все цели уже отсутствуют"
}
else {
    foreach ($target in $existing) {
        Write-Host "  - $target"
    }
}

if ($missing.Count -gt 0) {
    Write-Host ""
    Write-Host "Уже отсутствуют:" -ForegroundColor DarkGray

    foreach ($target in $missing) {
        Write-Host "  - $target"
    }
}

if (-not $Apply) {
    Write-Host ""
    Write-Host "Это был только предварительный просмотр." -ForegroundColor Cyan
    Write-Host "Для применения выполни:"
    Write-Host "  powershell -ExecutionPolicy Bypass -File .\Remove-LegacyArchitecture.ps1 -Apply"
    exit 0
}

Write-Host ""
Write-Host "Удаление..." -ForegroundColor Yellow

foreach ($target in $existing) {
    $fullPath = Join-Path $projectRoot $target
    Remove-Item -LiteralPath $fullPath -Recurse -Force
    Write-Host "  удалено: $target"
}

$reportDir = Join-Path $projectRoot "Audit"
New-Item -ItemType Directory -Path $reportDir -Force | Out-Null

$reportPath = Join-Path $reportDir "PHASE1_CLEANUP_RESULT.md"
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

$removedLines = @()

foreach ($target in $existing) {
    $removedLines += "- $target"
}

if ($removedLines.Count -eq 0) {
    $removedLines += "- Ничего: все цели уже отсутствовали."
}

$reportLines = @(
    "# Phase 1 Legacy Cleanup",
    "",
    "Дата: $timestamp",
    "Ветка: $branch",
    "",
    "## Удалено",
    "",
    ($removedLines -join "`r`n"),
    "",
    "## Причина",
    "",
    "Удалён старый программный контур:",
    "",
    "CommandProcessor",
    "-> DrawingManager",
    "-> EngineeringBrain",
    "-> Analysis / Decision / Planning",
    "",
    "Текущий runtime:",
    "",
    "Application",
    "-> InventorCommandDispatcher",
    "-> атомарные JSON-команды",
    "-> Inventor API",
    "",
    "Новая архитектура оставляет инженерные решения ИИ,",
    "а приложению — только глаза и руки.",
    "",
    "## Следующая проверка",
    "",
    "dotnet clean",
    "dotnet build",
    "",
    "После успешной сборки проверить:",
    "",
    '{"command":"ping"}',
    '{"command":"get_active_document"}',
    '{"command":"get_drawing_sheets"}'
)

$report = $reportLines -join "`r`n"
Set-Content -LiteralPath $reportPath -Value $report -Encoding UTF8

Write-Host ""
Write-Host "Удаление завершено." -ForegroundColor Green
Write-Host "Создан отчёт: Audit\PHASE1_CLEANUP_RESULT.md"
Write-Host ""
Write-Host "Теперь выполни:"
Write-Host "  dotnet clean"
Write-Host "  dotnet build"
