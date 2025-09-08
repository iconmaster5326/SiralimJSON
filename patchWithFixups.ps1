Import-Module powershell-yaml

class Patch {
    [hashtable]$Patches
    [string]$Short
    [string]$Long
    [string]$Plural
    [string]$IdField = "id"
}

$allPatches = @{}

function Merge-Patch {
    param(
        [Parameter(Mandatory = $true, Position = 0, ValueFromPipeline = $false)]
        [Patch]$Patch,
        [Parameter(Mandatory = $true, Position = 1, ValueFromPipeline = $false)]
        [psobject]$Json
    )

    $patchobject = $Patch.Patches[$Json.$($Patch.IdField)]
    foreach ($field in $patchobject.Keys) {
        if ($field -eq $Patch.IdField) {
            continue
        }
        Write-Output "    Patching field $field..."
        $Json.$field = $patchobject[$field]
    }
}

function Merge-Individuals {
    param(
        [Parameter(Mandatory = $true, Position = 0, ValueFromPipeline = $false)]
        [string]$Short,
        [Parameter(Mandatory = $true, Position = 1, ValueFromPipeline = $false)]
        [string]$Long,
        [Parameter(Mandatory = $true, Position = 2, ValueFromPipeline = $false)]
        [string]$Plural,
        [Parameter(Mandatory = $false, ValueFromPipeline = $false)]
        [string]$IdField = "id"
    )

    Write-Output "Patching individual $Plural..."

    $patches = @{}

    foreach ($file in (Get-ChildItem fixups\$Short\*.yaml)) {
        Write-Output "Reading patch $((Get-Item $file).BaseName)..."
        $yaml = Get-Content $file | ConvertFrom-Yaml
        $patches[$yaml.$IdField] = $yaml
    }

    $allPatches[$Plural] = [Patch]@{
        Patches = $patches
        Short   = $Short
        Long    = $Long
        Plural  = $Plural
        IdField = $IdField
    }

    foreach ($file in (Get-ChildItem exported\individual\$Short\*.json)) {
        $json = Get-Content $file | ConvertFrom-Json
        if ($patches.ContainsKey($json.$IdField)) {
            Write-Output "Patching individual $Long $((Get-Item $file).BaseName)..."
            Merge-Patch $allPatches[$Plural] $json
            ConvertTo-Json $json | Set-Content $file
        }
    }
}

Merge-Individuals "creature" "creature" "creatures"
Merge-Individuals "race" "race" "races"
Merge-Individuals "trait" "trait" "traits"
Merge-Individuals "spell" "spell" "spells"
Merge-Individuals "spellprop" "spell property" "spellProperties"
Merge-Individuals "item\spellprop" "spell property item" "spellPropertyItems"
Merge-Individuals "item\material" "material item" "materials"
Merge-Individuals "item\artifact" "artifact item" "artifacts"
Merge-Individuals "personality" "personality" "personalities"
Merge-Individuals "skin" "skin" "skins"
Merge-Individuals "costume" "costume" "costumes"
Merge-Individuals "decoration" "decoration" "decorations"
Merge-Individuals "wall" "wall style" "wallStyles"
Merge-Individuals "floor" "floor style" "floorStyles"
Merge-Individuals "bg" "background" "backgrounds"
Merge-Individuals "weather" "weather" "weather"
Merge-Individuals "music" "music" "music"
Merge-Individuals "god" "god" "gods"
Merge-Individuals "realm" "realm" "realms"
Merge-Individuals "condition" "condition" "conditions"
Merge-Individuals "spec" "specialization" "specializations"
Merge-Individuals "perk" "perk" "perks"
Merge-Individuals "realmprop" "realm property" "realmProperties"
Merge-Individuals "falsegod" "false god" "falseGods"
Merge-Individuals "rune" "false god rune" "runes"
Merge-Individuals "netherboss" "nether boss" "netherBosses"
Merge-Individuals "project" "project" "projects"
Merge-Individuals "item\project" "project item" "projectItems"
Merge-Individuals "accessory" "accessory" "accessories"

foreach ($patch in $allPatches.Values) {
    Write-Output "Patching aggregate $($patch.Long)..."
    $json = Get-Content exported\aggregate\$($patch.Plural).json | ConvertFrom-Json
    foreach ($item in $json) {
        if ($patch.Patches.ContainsKey($item.$($patch.IdField))) {
            Write-Output "Patching aggregate $($patch.Long) $($item.$($patch.IdField))..."
            Merge-Patch $patch $item
        }
    }
    ConvertTo-Json $json | Set-Content exported\aggregate\$($patch.Plural).json
}

Write-Output "Patching combined..."
$json = Get-Content exported\combined\combined.json | ConvertFrom-Json
foreach ($patch in $allPatches.Values) {
    foreach ($items in $json.$($patch.Plural)) {
        foreach ($item in $items) {
            if ($patch.Patches.ContainsKey($item.$($patch.IdField))) {
                Write-Output "Patching combined $($patch.Long) $($item.$($patch.IdField))..."
                Merge-Patch $patch $item
            }
        }
    }
}
ConvertTo-Json $json | Set-Content exported\combined\combined.json
