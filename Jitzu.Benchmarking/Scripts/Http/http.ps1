$url = $args[0]
$response = Invoke-RestMethod -Uri $url -Method Get
Write-Host $response.Content | ConvertFrom-Json