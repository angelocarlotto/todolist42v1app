# Test API Comments - PowerShell Script
Write-Host "=== Testing Task Comments API ===" -ForegroundColor Cyan

# 1. Login to get token
Write-Host "`n1. Logging in..." -ForegroundColor Yellow
$loginBody = @{
    username = "admin"
    password = "12345678"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5175/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    
    # Debug: Show the response
    Write-Host "  Response properties:" -ForegroundColor Gray
    $loginResponse | Get-Member -MemberType NoteProperty | ForEach-Object { 
        Write-Host "    - $($_.Name): $($loginResponse.($_.Name))" -ForegroundColor Gray 
    }
    
    # Try different token property names
    if ($loginResponse.token) {
        $token = $loginResponse.token
    } elseif ($loginResponse.Token) {
        $token = $loginResponse.Token
    } elseif ($loginResponse.accessToken) {
        $token = $loginResponse.accessToken
    } elseif ($loginResponse.access_token) {
        $token = $loginResponse.access_token
    } else {
        Write-Host "✗ Could not find token in response!" -ForegroundColor Red
        Write-Host "  Full response: $($loginResponse | ConvertTo-Json -Depth 3)" -ForegroundColor Gray
        exit
    }
    
    Write-Host "✓ Login successful! Token received." -ForegroundColor Green
    Write-Host "  Token (first 50 chars): $($token.Substring(0, [Math]::Min(50, $token.Length)))..." -ForegroundColor Gray
} catch {
    Write-Host "✗ Login failed: $_" -ForegroundColor Red
    Write-Host "  Error details: $($_.Exception.Message)" -ForegroundColor Red
    exit
}

# 2. Get list of tasks
Write-Host "`n2. Getting tasks..." -ForegroundColor Yellow
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "  Using Authorization header: Bearer $($token.Substring(0, [Math]::Min(30, $token.Length)))..." -ForegroundColor Gray

try {
    $tasks = Invoke-RestMethod -Uri "http://localhost:5175/api/tasks" -Method Get -Headers $headers
    Write-Host "✓ Found $($tasks.Count) tasks" -ForegroundColor Green
    
    if ($tasks.Count -eq 0) {
        Write-Host "✗ No tasks found. Please create a task first." -ForegroundColor Red
        exit
    }
    
    $taskId = $tasks[0].id
    $taskTitle = $tasks[0].shortTitle
    Write-Host "  Using task: $taskTitle (ID: $taskId)" -ForegroundColor Cyan
} catch {
    Write-Host "✗ Failed to get tasks: $_" -ForegroundColor Red
    Write-Host "  Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    Write-Host "  Status Description: $($_.Exception.Response.StatusDescription)" -ForegroundColor Red
    Write-Host "  Error Message: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  Headers used:" -ForegroundColor Gray
    $headers.GetEnumerator() | ForEach-Object { Write-Host "    $($_.Key): $($_.Value)" -ForegroundColor Gray }
    exit
}

# 3. Add a comment
Write-Host "`n3. Adding comment to task..." -ForegroundColor Yellow
$commentBody = @{
    text = "This is a test comment added via API at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
} | ConvertTo-Json

try {
    $commentResponse = Invoke-RestMethod -Uri "http://localhost:5175/api/tasks/$taskId/comments" -Method Post -Body $commentBody -Headers $headers
    Write-Host "✓ Comment added successfully!" -ForegroundColor Green
    Write-Host "  Comment ID: $($commentResponse.id)" -ForegroundColor Cyan
    Write-Host "  Text: $($commentResponse.text)" -ForegroundColor Cyan
    Write-Host "  Username: $($commentResponse.username)" -ForegroundColor Cyan
    Write-Host "  Created: $($commentResponse.createdAt)" -ForegroundColor Cyan
} catch {
    Write-Host "✗ Failed to add comment: $_" -ForegroundColor Red
    Write-Host "  Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    exit
}

# 4. Get all comments for the task
Write-Host "`n4. Retrieving all comments..." -ForegroundColor Yellow
try {
    $comments = Invoke-RestMethod -Uri "http://localhost:5175/api/tasks/$taskId/comments" -Method Get -Headers $headers
    Write-Host "✓ Found $($comments.Count) comment(s)" -ForegroundColor Green
    
    foreach ($comment in $comments) {
        Write-Host "  ---" -ForegroundColor Gray
        Write-Host "  ID: $($comment.id)" -ForegroundColor Cyan
        Write-Host "  User: $($comment.username)" -ForegroundColor Cyan
        Write-Host "  Text: $($comment.text)" -ForegroundColor White
        Write-Host "  Created: $($comment.createdAt)" -ForegroundColor Gray
    }
} catch {
    Write-Host "✗ Failed to get comments: $_" -ForegroundColor Red
    exit
}

# 5. Add another comment
Write-Host "`n5. Adding second comment..." -ForegroundColor Yellow
$commentBody2 = @{
    text = "This is a SECOND test comment! Real-time updates should work! 🚀"
} | ConvertTo-Json

try {
    $commentResponse2 = Invoke-RestMethod -Uri "http://localhost:5175/api/tasks/$taskId/comments" -Method Post -Body $commentBody2 -Headers $headers
    Write-Host "✓ Second comment added!" -ForegroundColor Green
    Write-Host "  Text: $($commentResponse2.text)" -ForegroundColor Cyan
} catch {
    Write-Host "✗ Failed to add second comment: $_" -ForegroundColor Red
}

# 6. Get updated task to see comments and activity log
Write-Host "`n6. Getting task details with comments and activity..." -ForegroundColor Yellow
try {
    $taskDetails = Invoke-RestMethod -Uri "http://localhost:5175/api/tasks/$taskId" -Method Get -Headers $headers
    Write-Host "✓ Task retrieved" -ForegroundColor Green
    Write-Host "  Title: $($taskDetails.shortTitle)" -ForegroundColor Cyan
    Write-Host "  Comments: $($taskDetails.comments.Count)" -ForegroundColor Cyan
    Write-Host "  Activity Log Entries: $($taskDetails.activityLog.Count)" -ForegroundColor Cyan
    
    Write-Host "`n  Recent Activities:" -ForegroundColor Yellow
    $taskDetails.activityLog | Select-Object -Last 5 | ForEach-Object {
        Write-Host "    - [$($_.activityType)] $($_.description) by $($_.username)" -ForegroundColor Gray
    }
} catch {
    Write-Host "✗ Failed to get task details: $_" -ForegroundColor Red
}

Write-Host "`n=== Test Complete! ===" -ForegroundColor Cyan
Write-Host "Now check the UI at http://localhost:3000 to see real-time updates!" -ForegroundColor Green
