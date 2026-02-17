Import-Module powershell-yaml
Import-Module -Name .\siralim-json.psm1 -Force

$DEPTH = 100

function Merge-Patch {
    param(
        [Parameter(Mandatory = $true, Position = 0, ValueFromPipeline = $false)]
        [object]$Category,
        [Parameter(Mandatory = $true, Position = 1, ValueFromPipeline = $false)]
        [psobject]$Json
    )

    $patchobject = $Category.Patches[$Json.$($Category.IdField)]
    foreach ($field in $patchobject.Keys) {
        if ($field -eq $Category.IdField) {
            continue
        }
        Write-Output "    Patching field $field..."
        $Json.$field = $patchobject[$field]
    }
}

function Merge-Individuals {
    param(
        [Parameter(Mandatory = $true, Position = 0, ValueFromPipeline = $false)]
        [object]$Category
    )

    Write-Output "Patching $($Category.Plural)..."

    foreach ($file in (Get-ChildItem fixups\$($Category.Short)\*.yaml)) {
        Write-Output "Reading patch $((Get-Item $file).BaseName)..."
        $yaml = Get-Content $file | ConvertFrom-Yaml
        $Category.Patches[$yaml.$($Category.IdField)] = $yaml
    }

    foreach ($file in (Get-ChildItem dumped\$($Category.Short)\*.json)) {
        $json = Get-Content $file | ConvertFrom-Json
        if ($Category.Patches.ContainsKey($json.$($Category.IdField))) {
            Write-Output "Patching $($Category.Long) $((Get-Item $file).BaseName)..."
            Merge-Patch $Category $json
            ConvertTo-Json $json -Depth $DEPTH | Set-Content $file
        }
    }
}

Write-Output "Appling patches..."

foreach ($cat in $SiralimCategories) {
    Merge-Individuals $cat
}

Write-Output "Done."
