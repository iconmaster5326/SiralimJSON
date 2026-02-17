class SiralimCategory {
    [string]$Short
    [string]$Long
    [string]$Plural
    [string]$IdField = "id"
    [hashtable]$Patches = @{}
    [hashtable]$Filenames = @{}
}

$SiralimCategories = @(
    [SiralimCategory]@{Short="accessory"; Long="accessory"; Plural="accessories";},
    [SiralimCategory]@{Short="bg"; Long= "background"; Plural="backgrounds";},
    [SiralimCategory]@{Short="condition"; Long= "condition"; Plural="conditions";},
    [SiralimCategory]@{Short="costume"; Long= "costume"; Plural="costumes";},
    [SiralimCategory]@{Short="creature"; Long="creature"; Plural="creatures";},
    [SiralimCategory]@{Short="decoration"; Long= "decoration"; Plural="decorations";},
    [SiralimCategory]@{Short="falsegod"; Long= "falseGod"; Plural="falseGods";},
    [SiralimCategory]@{Short="floor"; Long= "floorStyle"; Plural="floorStyles";},
    [SiralimCategory]@{Short="god"; Long= "god"; Plural="gods";},
    [SiralimCategory]@{Short="item\artifact"; Long= "artifactItem"; Plural="artifacts";},
    [SiralimCategory]@{Short="item\material"; Long= "materialItem"; Plural="materials";},
    [SiralimCategory]@{Short="item\project"; Long= "projectItem"; Plural="projectItems";},
    [SiralimCategory]@{Short="item\spellprop"; Long= "spellPropertyItem"; Plural="spellPropertyItems";},
    [SiralimCategory]@{Short="music"; Long= "music"; Plural="music";},
    [SiralimCategory]@{Short="netherboss"; Long= "netherBoss"; Plural="netherBosses";},
    [SiralimCategory]@{Short="perk"; Long= "perk"; Plural="perks";},
    [SiralimCategory]@{Short="personality"; Long= "personality"; Plural="personalities";},
    [SiralimCategory]@{Short="project"; Long= "project"; Plural="projects";},
    [SiralimCategory]@{Short="race"; Long="race"; Plural="races"; IdField="name";},
    [SiralimCategory]@{Short="realm"; Long= "realm"; Plural="realms";},
    [SiralimCategory]@{Short="realmprop"; Long= "realmProperty"; Plural="realmProperties";},
    [SiralimCategory]@{Short="rune"; Long= "rune"; Plural="runes";},
    [SiralimCategory]@{Short="skin"; Long= "skin"; Plural="skins";},
    [SiralimCategory]@{Short="spec"; Long= "specialization"; Plural="specializations";},
    [SiralimCategory]@{Short="spell"; Long= "spell"; Plural="spells";},
    [SiralimCategory]@{Short="spellprop"; Long= "spellProperty"; Plural="spellProperties";},
    [SiralimCategory]@{Short="trait"; Long= "trait"; Plural="traits";},
    [SiralimCategory]@{Short="wall"; Long= "wallStyle"; Plural="wallStyles";},
    [SiralimCategory]@{Short="weather"; Long= "weather"; Plural="weather";}
)

function Set-ContentAndParents {
    param(
        [Parameter(Mandatory = $true, Position = 0, ValueFromPipeline = $false)]
        [System.IO.FileInfo]$Path,
        [Parameter(Mandatory = $true, Position = 1, ValueFromPipeline = $true)]
        [string]$Value
    )

    New-Item -Path $Path -ItemType File -Force | Out-Null
    Set-Content $Path $Value
}

Export-ModuleMember -Variable SiralimCategories -Function Set-ContentAndParents
