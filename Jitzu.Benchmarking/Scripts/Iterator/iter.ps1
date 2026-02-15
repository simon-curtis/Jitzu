param(
    [Parameter(Mandatory=$true)]
    [int]$MaxCount
)

# Example "class" (implemented as a PSCustomObject with ScriptMethods)
$e = [PSCustomObject]@{
    run = 0;
    limit = $MaxCount
}

# Add the "next" method
$e | Add-Member -MemberType ScriptMethod -Name next -Value {
    if ($this.run -eq $this.limit) {
        return $false
    }
    $this.run++
    return $true
}

# Main loop
while ($e.next()) {
    Write-Host $e.run
}