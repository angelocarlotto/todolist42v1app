#!/usr/bin/env pwsh

# Simple PowerShell script to test the API endpoints

Write-Host "Testing API endpoints..." -ForegroundColor Green

# Test basic endpoint (should work without auth)
Write-Host "`nTesting /api/test (no auth required):" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5175/api/test" -Method GET
    Write-Host "SUCCESS: $($response | ConvertTo-Json)" -ForegroundColor Green
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.ErrorDetails.Message)" -ForegroundColor Red
}

# Test tasks endpoint (should return 401 without auth)
Write-Host "`nTesting /api/tasks (should return 401):" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5175/api/tasks" -Method GET
    Write-Host "SUCCESS: $($response | ConvertTo-Json)" -ForegroundColor Green
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response.StatusCode) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Yellow
    }
    if ($_.ErrorDetails.Message) {
        Write-Host "Response: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
}