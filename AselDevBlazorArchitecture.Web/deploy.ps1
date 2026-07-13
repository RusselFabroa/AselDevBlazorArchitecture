Write-Host "Publishing project..." -ForegroundColor Cyan
dotnet publish -c Release -o publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed" -ForegroundColor Red
    exit 1
}

Write-Host "Uploading files to VPS..." -ForegroundColor Cyan
scp -r publish/* deploy@72.62.198.97:/var/www/apps/tpc-dx/

if ($LASTEXITCODE -ne 0) {
    Write-Host "Upload failed" -ForegroundColor Red
    exit 1
}

Write-Host "Restarting service..." -ForegroundColor Cyan
ssh deploy@72.62.198.97 "sudo systemctl restart blazorapp"

Write-Host "Checking service status..." -ForegroundColor Cyan
ssh deploy@72.62.198.97 "sudo systemctl status blazorapp --no-pager"

Write-Host "Deployment complete!" -ForegroundColor Green