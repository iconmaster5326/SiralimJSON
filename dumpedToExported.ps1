Import-Module -Name .\siralim-json.psm1 -Force

if (-not (Test-Path dumped\progress.log)) {
    Write-Error "Dumper not yet started! Run the dumper and re-try this script."
    return
}

if ((Get-Content dumped\progress.log) -ne "32") {
    Write-Error "Dumper wasn't finished! Run the dumper again and re-try this script."
    return
}

$DEPTH = 100

$MetaJSON = Get-Content dumped\metadata.json | ConvertFrom-Json

Write-Output "Exporting individual..."

function Get-IndividualJSON {
    param(
        [Parameter(Mandatory = $true, Position = 0, ValueFromPipeline = $false)]
        [System.IO.FileInfo]$File
    )

    $schemaname = $cat.Long
    $json = Get-Content $File | ConvertFrom-Json
    $json | Add-Member -Type NoteProperty -Name "`$schema" -Value "https://github.com/iconmaster5326/SiralimJSON/blob/main/schema/$schemaname.yaml"

    $cat.Filenames["$($json.$($cat.IdField))"] = "$($cat.Short)/$($File.Name)".Replace("\", "/")

    return $json
}

foreach ($cat in $SiralimCategories) {
    Write-Output "    $($cat.Short)"
    foreach ($file in (Get-ChildItem "dumped\$($cat.Short)\*.json")) {
        Get-IndividualJSON $file | ConvertTo-Json -Depth $DEPTH | Set-ContentAndParents exported\individual\$($cat.Short)\$($file.Name)
    }
}

Write-Output "    metadata"
$IndivdualJSON = @{
    "`$schema" = "https://github.com/iconmaster5326/SiralimJSON/blob/main/schema/individual.yaml";
    metadata = $MetaJSON;
}
foreach ($cat in $SiralimCategories) {
    $IndivdualJSON[$cat.Plural] = $cat.Filenames
}
ConvertTo-Json $IndivdualJSON -Depth $DEPTH | Set-ContentAndParents exported\individual\individual.json

Write-Output "Exporting aggregate..."

foreach ($cat in $SiralimCategories) {
    Write-Output "    $($cat.Short)"
    $result = [System.Collections.ArrayList]@()
    foreach ($file in (Get-ChildItem "dumped\$($cat.Short)\*.json")) {
        $result.Add((Get-IndividualJSON $file)) | Out-Null
    }
    $result | Sort-Object -Property $cat.IdField | ConvertTo-Json -Depth $DEPTH | Set-ContentAndParents exported\aggregate\$($cat.Plural).json
}

Write-Output "    metadata"
ConvertTo-Json $MetaJSON | Set-ContentAndParents exported\aggregate\metadata.json

Write-Output "Exporting combined..."

$CombinedJSON = @{
    "`$schema" = "https://github.com/iconmaster5326/SiralimJSON/blob/main/schema/combined.yaml";
    metadata = $MetaJSON;
}

foreach ($cat in $SiralimCategories) {
    $result = [System.Collections.Generic.List[object]]@()
    foreach ($file in (Get-ChildItem "dumped\$($cat.Short)\*.json")) {
        $result.Add((Get-IndividualJSON $file)) | Out-Null
    }
    $CombinedJSON[$cat.Plural] = $result | Sort-Object -Property $cat.IdField
}

ConvertTo-Json $CombinedJSON -Depth $DEPTH | Set-ContentAndParents exported\combined\combined.json

Remove-Item dumped\progress.log

Write-Output "Done!"
