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

Require-Text 'VERSION.txt' '^v0\.0\.20$' 'VERSION.txt nao esta em v0.0.20.'
Require-Text 'Automind.Treinamentos.csproj' '<Version>0\.0\.20</Version>' 'Assembly nao esta em v0.0.20.'
Require-Text 'Models\TeamsReminderModels.cs' 'public const string Sent = "sent"' 'Status final Enviado nao encontrado.'
Require-Text 'Services\TeamsWebhookService.cs' 'nao confirmou o envio ao Teams' 'Workflow sem confirmacao ainda pode ser tratado como sucesso.'
Require-Text 'Data\DatabaseInitializer.cs' 'ApplyV020TeamsReminderResetAsync' 'Migration de inicio do historico v0.0.20 nao encontrada.'
Require-Text 'Data\DatabaseInitializer.cs' 'DELETE FROM TeamsReminderHistory' 'Historico anterior nao e limpo na entrada da v0.0.20.'
Forbid-Text 'Data\DatabaseInitializer.cs' 'ImportLegacyTeamsReminderAuditAsync' 'Importacao de auditoria antiga ainda esta ativa.'
Require-Text 'Services\TeamsReminderWorker.cs' 'BackgroundService' 'Worker da agenda Teams nao encontrado.'
Require-Text 'Services\TeamsReminderWorker.cs' 'cycleKey' 'Protecao de ciclo automatico contra duplicidade nao encontrada.'
Require-Text 'Views\Admin\Pending.cshtml' 'Enviado' 'Estado visual Enviado nao encontrado.'
Require-Text 'Views\Admin\Pending.cshtml' 'Falhou' 'Estado visual Falhou nao encontrado.'
Require-Text 'Views\Admin\Pending.cshtml' 'teams-none' 'Estado visual Nao enviado nao encontrado.'
Require-Text 'Views\Admin\Pending.cshtml' 'pending-table-shell' 'Tabela responsiva de pendencias nao encontrada.'
Require-Text 'wwwroot\css\site.css' 'position:sticky;right:0' 'Coluna de acoes nao esta fixa na lateral direita.'
Require-Text 'wwwroot\css\site.css' '@media\(max-width:760px\)' 'Layout mobile da tabela nao encontrado.'
Forbid-Text 'wwwroot\css\site.css' 'min-width:1540px' 'Largura fixa antiga da tabela ainda existe.'
Forbid-Text 'Views\Admin\Pending.cshtml' 'Aceito pelo Workflow' 'Texto tecnico Aceito pelo Workflow ainda aparece na tela.'
Forbid-Text 'Views\Admin\Pending.cshtml' 'Confirmado' 'Texto tecnico Confirmado ainda aparece na tela.'
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

Write-Host 'Validacoes estaticas v0.0.20: OK'

if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    Push-Location $root
    try {
        dotnet restore .\Automind.Treinamentos.csproj
        if ($LASTEXITCODE -ne 0) { throw 'dotnet restore falhou.' }
        dotnet build .\Automind.Treinamentos.csproj -c Release --no-restore
        if ($LASTEXITCODE -ne 0) { throw 'dotnet build falhou.' }
        dotnet run --project .\tests\Automind.Treinamentos.SelfTests\Automind.Treinamentos.SelfTests.csproj -c Release
        if ($LASTEXITCODE -ne 0) { throw 'SelfTests v0.0.20 falharam.' }
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
