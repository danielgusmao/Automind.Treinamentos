#requires -RunAsAdministrator
$ErrorActionPreference = 'Stop'

Import-Module WebAdministration

$root = 'C:\Automind.Treinamentos'
$web = Join-Path $root 'Web'
$legacyRoot = Join-Path $web 'App_Data'
$targetPool = 'Automind.Treinamentos'
$sourcePool = 'CadastroColaboradores'
$appPoolIdentity = "IIS AppPool\$targetPool"
$secretName = 'Automind__Teams__WebhookUrl'
$psPath = 'MACHINE/WEBROOT/APPHOST'

$folders = @(
    (Join-Path $root 'Data'),
    (Join-Path $root 'Treinamentos'),
    (Join-Path $root 'Evidencias'),
    (Join-Path $root 'Evidencias\Colaboradores'),
    (Join-Path $root 'Evidencias\Relatorios'),
    (Join-Path $root 'Logs'),
    (Join-Path $root 'Backup')
)

$writeFolders = @(
    (Join-Path $root 'Data'),
    (Join-Path $root 'Treinamentos'),
    (Join-Path $root 'Evidencias'),
    (Join-Path $root 'Logs'),
    (Join-Path $root 'Backup')
)

if (-not (Test-Path "IIS:\AppPools\$targetPool")) {
    throw "App Pool '$targetPool' nao existe."
}

if (-not (Test-Path "IIS:\AppPools\$sourcePool")) {
    throw "App Pool '$sourcePool' nao existe."
}

$wasStarted = (Get-WebAppPoolState -Name $targetPool).Value -eq 'Started'
if ($wasStarted) {
    Stop-WebAppPool -Name $targetPool
    Start-Sleep -Seconds 2
}

try {
    foreach ($folder in $folders) {
        New-Item -ItemType Directory -Path $folder -Force | Out-Null
    }

    $migrationMap = @{
        'Data' = (Join-Path $root 'Data')
        'Treinamentos' = (Join-Path $root 'Treinamentos')
        'Evidencias' = (Join-Path $root 'Evidencias')
        'Logs' = (Join-Path $root 'Logs')
        'Backup' = (Join-Path $root 'Backup')
    }

    if (Test-Path $legacyRoot) {
        foreach ($name in $migrationMap.Keys) {
            $source = Join-Path $legacyRoot $name
            $destination = $migrationMap[$name]
            if (Test-Path $source) {
                Get-ChildItem -Path $source -Force | ForEach-Object {
                    Copy-Item -Path $_.FullName -Destination $destination -Recurse -Force -ErrorAction Stop
                }
            }
        }
    }

    foreach ($folder in $writeFolders) {
        & icacls.exe $folder /grant "${appPoolIdentity}:(OI)(CI)M" /T /C | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Falha ao aplicar ACL em '$folder'."
        }
    }

    $sourceFilter = "system.applicationHost/applicationPools/add[@name='$sourcePool']/environmentVariables/add[@name='$secretName']"
    $sourceProperty = Get-WebConfigurationProperty -PSPath $psPath -Filter $sourceFilter -Name 'value' -ErrorAction Stop
    $sourceValue = [string]$sourceProperty.Value

    if ([string]::IsNullOrWhiteSpace($sourceValue)) {
        throw "A variavel '$secretName' nao foi encontrada no App Pool '$sourcePool'."
    }

    $targetCollection = "system.applicationHost/applicationPools/add[@name='$targetPool']/environmentVariables"
    $targetFilter = "$targetCollection/add[@name='$secretName']"
    $targetItem = Get-WebConfiguration -PSPath $psPath -Filter $targetFilter -ErrorAction SilentlyContinue

    if ($null -eq $targetItem) {
        Add-WebConfigurationProperty -PSPath $psPath -Filter $targetCollection -Name '.' -Value @{ name = $secretName; value = $sourceValue }
    }
    else {
        Set-WebConfigurationProperty -PSPath $psPath -Filter $targetFilter -Name 'value' -Value $sourceValue
    }

    $enabledName = 'Automind__Teams__Enabled'
    $enabledFilter = "$targetCollection/add[@name='$enabledName']"
    $enabledItem = Get-WebConfiguration -PSPath $psPath -Filter $enabledFilter -ErrorAction SilentlyContinue
    if ($null -eq $enabledItem) {
        Add-WebConfigurationProperty -PSPath $psPath -Filter $targetCollection -Name '.' -Value @{ name = $enabledName; value = 'true' }
    }
    else {
        Set-WebConfigurationProperty -PSPath $psPath -Filter $enabledFilter -Name 'value' -Value 'true'
    }

    Write-Host 'Estrutura criada e dados legados copiados.'
    Write-Host 'ACLs de escrita aplicadas somente nas pastas persistentes.'
    Write-Host 'Webhook Teams copiado do CadastroColaboradores sem exibir o segredo.'
    Write-Host 'Web\App_Data foi preservado para rollback e nao foi removido.'
}
finally {
    if ($wasStarted) {
        Start-WebAppPool -Name $targetPool
    }
}

Write-Host "App Pool: $((Get-WebAppPoolState -Name $targetPool).Value)"
