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

Require-Text 'VERSION.txt' '^v0\.0\.15$' 'VERSION.txt nao esta em v0.0.15.'
Require-Text 'Services\TeamsWebhookService.cs' 'UriSchemeHttps' 'Webhook Teams nao exige HTTPS.'
Require-Text 'Program.cs' 'AddRateLimiter' 'Rate limit do login nao foi registrado.'
Require-Text 'Program.cs' 'ad-session-enabled' 'Revalidacao de conta AD nao encontrada.'
Require-Text 'Data\DatabaseInitializer.cs' 'UX_Trainings_Code_Version' 'Indice unico Code+Version nao encontrado.'
Require-Text 'Services\TrainingSnapshotService.cs' 'expectedHash' 'Validacao de snapshot nao encontrada.'
Forbid-Text 'Views\Training\SecurityAwareness.cshtml' 'data-correct' 'Gabarito ainda esta exposto em data-correct.'
Forbid-Text 'Services\SimplePdfService.cs' 'v0\.0\.8' 'PDF ainda contem versao v0.0.8 hardcoded.'
Forbid-Text 'Controllers\AdminController.cs' 'ex\.Message' 'AdminController ainda exibe detalhes de exception.'

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Host 'Validacoes estaticas v0.0.15: OK'

if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    Push-Location $root
    try {
        dotnet restore .\Automind.Treinamentos.csproj
        if ($LASTEXITCODE -ne 0) { throw 'dotnet restore falhou.' }
        dotnet build .\Automind.Treinamentos.csproj -c Release --no-restore
        if ($LASTEXITCODE -ne 0) { throw 'dotnet build falhou.' }
        dotnet run --project .\tests\Automind.Treinamentos.SelfTests\Automind.Treinamentos.SelfTests.csproj -c Release
        if ($LASTEXITCODE -ne 0) { throw 'SelfTests v0.0.15 falharam.' }
        dotnet list .\Automind.Treinamentos.csproj package --vulnerable --include-transitive
        if ($LASTEXITCODE -ne 0) { throw 'scan de dependencias falhou.' }
    }
    finally { Pop-Location }
}
else {
    Write-Warning 'dotnet nao encontrado: build e scan de dependencias nao foram executados.'
}

if (Get-Command gitleaks -ErrorAction SilentlyContinue) {
    & gitleaks detect --source $root --no-banner --redact
    if ($LASTEXITCODE -ne 0) { throw 'Gitleaks encontrou problema ou falhou.' }
}
else {
    Write-Warning 'Gitleaks nao encontrado: secret scan completo/historico nao executado.'
}
