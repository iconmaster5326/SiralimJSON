$version = (Get-Content dumped\metadata.json | ConvertFrom-Json).version

if (-not (Test-Path -Path exported\combined\.git\)) {
    mkdir temp\
    Copy-Item .git temp\ -Recurse -Force
    Set-Location temp
    git switch combined
    if (-not (Test-Path exported\combined\README.md)) {
        Copy-Item README.md exported\combined\
    }
    Set-Location ..
    Copy-Item temp\.git exported\combined\ -Recurse -Force
    Remove-Item temp -Recurse -Force
}
Set-Location exported\combined
git add .
git commit -m "$version"
git push --set-upstream origin combined
Set-Location ..\..

if (-not (Test-Path -Path exported\aggregate\.git\)) {
    mkdir temp\
    Copy-Item .git temp\ -Recurse -Force
    Set-Location temp
    git switch aggregate
    if (-not (Test-Path exported\aggregate\README.md)) {
        Copy-Item README.md exported\aggregate\
    }
    Set-Location ..
    Copy-Item temp\.git exported\aggregate\ -Recurse -Force
    Remove-Item temp -Recurse -Force
}
Set-Location exported\aggregate
git add .
git commit -m "$version"
git push --set-upstream origin aggregate
Set-Location ..\..

if (-not (Test-Path -Path exported\individual\.git\)) {
    mkdir temp\
    Copy-Item .git temp\ -Recurse -Force
    Set-Location temp
    git switch individual
    if (-not (Test-Path exported\individual\README.md)) {
        Copy-Item README.md exported\individual\
    }
    Set-Location ..
    Copy-Item temp\.git exported\individual\ -Recurse -Force
    Remove-Item temp -Recurse -Force
}
Set-Location exported\individual
git add .
git commit -m "$version"
git push --set-upstream origin individual
Set-Location ..\..

Compress-Archive -Path exported\combined\* -DestinationPath combined.zip -Force
Compress-Archive -Path exported\aggregate\* -DestinationPath aggregate.zip -Force
Compress-Archive -Path exported\individual\* -DestinationPath individual.zip -Force

Write-Output "All done! Make a new GitHub release and upload the three ZIP files."
