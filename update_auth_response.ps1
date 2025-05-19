$filePath = "c:\Users\kaoti\CascadeProjects\Inventario\Inventario.Web\Services\Implementations\AuthService.cs"
$content = Get-Content -Path $filePath -Raw

# Reemplazar todas las instancias de "Success =" por "IsSuccess ="
$updatedContent = $content -replace 'Success =', 'IsSuccess ='

# Escribir el contenido actualizado de vuelta al archivo
Set-Content -Path $filePath -Value $updatedContent -NoNewline

Write-Host "Se han actualizado todas las referencias de 'Success' a 'IsSuccess' en el archivo."
