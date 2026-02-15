param(
    [int]$limit = 10000
)

$i = 0
while ($i -lt $limit) {
    Write-Output $i
    $i++
}