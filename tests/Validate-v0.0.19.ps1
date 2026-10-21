$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$failures = New-Object System.Collections.Generic.List[string]

function Require-Text([string]$File, [string]$Pattern, [string]$Message) {
    $path = Join-Path $root $File
    if (-not (Select-String -Path $path -Pattern $Pattern -Quiet)) { $failures.Add($Message) }
}
function Forbid-Text([string]$File, [string]$Pattern, [string]$Message) {
    $path = Join-Path $root $File
    if (Select-String -Path $path -Pattern $Pattern -Quiet) { $failures.Add($Message) }
}

Require-Text 'VERSION.txt' '^v0\.0\.19$' 'VERSION.txt nao esta em v0.0.19.'
Require-Text 'Automind.Treinamentos.csproj' '<Version>0\.0\.19</Version>' 'Assembly nao esta em v0.0.19.'
Require-Text 'Services\TeamsWebhookService.cs' 'UriSchemeHttps' 'Webhook Teams nao exige HTTPS.'
Require-Text 'Services\TeamsWebhookService.cs' 'DeliveryConfirmed' 'Webhook Teams nao diferencia confirmacao de entrega.'
Require-Text 'Data\DatabaseInitializer.cs' 'TeamsReminderHistory' 'Tabela de historico Teams nao encontrada.'
Require-Text 'Data\DatabaseInitializer.cs' 'TrainingReminderSchedules' 'Tabela de agenda Teams nao encontrada.'
Require-Text 'Data\DatabaseInitializer.cs' 'ImportLegacyTeamsReminderAuditAsync' 'Importacao dos Audit-*.jsonl nao encontrada.'
Require-Text 'Services\TeamsReminderWorker.cs' 'BackgroundService' 'Worker da agenda Teams nao encontrado.'
Require-Text 'Services\TeamsReminderWorker.cs' 'cycleKey' 'Protecao de ciclo automatico contra duplicidade nao encontrada.'
Require-Text 'Views\Admin\Pending.cshtml' 'Aceito pelo Workflow' 'Estado visual de envio nao confirmado nao encontrado.'
Require-Text 'Views\Admin\Pending.cshtml' 'markDuePending' 'Selecao de usuarios que precisam de aviso nao encontrada.'
Require-Text 'Program.cs' 'AddHostedService<TeamsReminderWorker>' 'Worker Teams nao foi registrado.'
Require-Text 'Program.cs' 'AddRateLimiter' 'Rate limit do login nao foi preservado.'
Require-Text 'Program.cs' 'ad-session-enabled' 'Revalidacao de conta AD nao foi preservada.'
Require-Text 'Data\DatabaseInitializer.cs' 'UX_Trainings_Code_Version' 'Indice unico Code+Version nao foi preservado.'
Require-Text 'Services\TrainingSnapshotService.cs' 'expectedHash' 'Validacao de snapshot nao foi preservada.'
Forbid-Text 'Views\Training\SecurityAwareness.cshtml' 'data-correct' 'Gabarito esta exposto em data-correct.'
Forbid-Text 'Controllers\AdminController.cs' 'ex\.Message' 'AdminController exibe detalhes de exception.'

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Host 'Validacoes estaticas v0.0.19: OK'

if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    Push-Location $root
    try {
        dotnet restore .\Automind.Treinamentos.csproj
        if ($LASTEXITCODE -ne 0) { throw 'dotnet restore falhou.' }
        dotnet build .\Automind.Treinamentos.csproj -c Release --no-restore
        if ($LASTEXITCODE -ne 0) { throw 'dotnet build falhou.' }
        dotnet run --project .\tests\Automind.Treinamentos.SelfTests\Automind.Treinamentos.SelfTests.csproj -c Release
        if ($LASTEXITCODE -ne 0) { throw 'SelfTests v0.0.19 falharam.' }
        dotnet list .\Automind.Treinamentos.csproj package --vulnerable --include-transitive
        if ($LASTEXITCODE -ne 0) { throw 'scan de dependencias falhou.' }
    }
    finally { Pop-Location }
}
else {
    Write-Warning 'dotnet nao encontrado: build, SelfTests e scan de dependencias nao foram executados.'
}

if (Get-Command gitleaks -ErrorAction SilentlyContinue) {
    & gitleaks detect --source $root --no-banner --redact
    if ($LASTEXITCODE -ne 0) { throw 'Gitleaks encontrou problema ou falhou.' }
}
else {
    Write-Warning 'Gitleaks nao encontrado: secret scan completo/historico nao executado.'
}
