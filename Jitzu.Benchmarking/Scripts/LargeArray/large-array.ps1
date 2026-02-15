param(
    [Parameter(Mandatory=$true)]
    [int]$size
)

# Generate random integers
$numbers = @()
for ($i = 0; $i -lt $size; $i++) {
    $numbers += Get-Random -Minimum 0 -Maximum 1000000
}

# Record start time
$start = Get-Date

# Sort the array
$sorted = $numbers | Sort-Object

# Record end time
$end = Get-Date

# Print results
$elapsed = ($end - $start).TotalMilliseconds
Write-Output "Sorted $size integers in $elapsed ms"