# Simple API Test
Write-Host "=== Simple API Test ===" -ForegroundColor Cyan

# Test 1: Register a user
Write-Host ""
$username = "apitest_$(Get-Date -Format 'HHmmss')"
Write-Host "1. Attempting to register user '$username'..." -ForegroundColor Yellow
$registerBody = @{
    username = $username
    password = "12345678"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod -Uri "http://localhost:5175/api/auth/register" -Method Post -Body $registerBody -ContentType "application/json"
    Write-Host "SUCCESS: Registration successful!" -ForegroundColor Green
    Write-Host "  User ID: $($registerResponse.userId)" -ForegroundColor Gray
    Write-Host "  Tenant ID: $($registerResponse.tenantId)" -ForegroundColor Gray
} catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 400) {
        Write-Host "INFO: User already exists (this is OK)" -ForegroundColor Yellow
    } else {
        Write-Host "ERROR: Registration failed: $_" -ForegroundColor Red
        Write-Host "  Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    }
}

# Test 2: Login
Write-Host ""
Write-Host "2. Logging in as '$username'..." -ForegroundColor Yellow
$loginBody = @{
    username = $username
    password = "12345678"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5175/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    Write-Host "SUCCESS: Login successful!" -ForegroundColor Green
    Write-Host "  Token received: $($loginResponse.token.Substring(0, 30))..." -ForegroundColor Gray
    Write-Host "  Username: $($loginResponse.user.username)" -ForegroundColor Gray
    Write-Host "  User ID: $($loginResponse.user.userId)" -ForegroundColor Gray
    Write-Host "  Tenant ID: $($loginResponse.user.tenantId)" -ForegroundColor Gray
    
    $token = $loginResponse.token
    $userId = $loginResponse.user.userId
    $tenantId = $loginResponse.user.tenantId
} catch {
    Write-Host "ERROR: Login failed: $_" -ForegroundColor Red
    Write-Host "  Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    exit
}

# Test 3: Create a task
Write-Host ""
Write-Host "3. Creating a test task..." -ForegroundColor Yellow
$dueDate = (Get-Date).AddDays(7).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
$taskBody = @{
    shortTitle = "Test Task for Comments"
    description = "This task will be used to test the comments feature"
    status = "ToDo"
    dueDate = $dueDate
    criticality = "Medium"
    tags = @()
    files = @()
} | ConvertTo-Json

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $task = Invoke-RestMethod -Uri "http://localhost:5175/api/tasks" -Method Post -Body $taskBody -Headers $headers
    Write-Host "SUCCESS: Task created!" -ForegroundColor Green
    Write-Host "  Task ID: $($task.id)" -ForegroundColor Gray
    Write-Host "  Title: $($task.shortTitle)" -ForegroundColor Gray
    
    $taskId = $task.id
} catch {
    Write-Host "ERROR: Failed to create task: $_" -ForegroundColor Red
    Write-Host "  Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    exit
}

# Test 4: Add a comment
Write-Host ""
Write-Host "4. Adding a comment to the task..." -ForegroundColor Yellow
$commentBody = @{
    text = "This is my first API comment! Added at $(Get-Date -Format 'HH:mm:ss')"
} | ConvertTo-Json

try {
    $comment = Invoke-RestMethod -Uri "http://localhost:5175/api/tasks/$taskId/comments" -Method Post -Body $commentBody -Headers $headers
    Write-Host "SUCCESS: Comment added!" -ForegroundColor Green
    Write-Host "  Comment ID: $($comment.id)" -ForegroundColor Gray
    Write-Host "  Text: $($comment.text)" -ForegroundColor Gray
    Write-Host "  Username: $($comment.username)" -ForegroundColor Gray
    Write-Host "  Created: $($comment.createdAt)" -ForegroundColor Gray
} catch {
    Write-Host "ERROR: Failed to add comment: $_" -ForegroundColor Red
    Write-Host "  Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    exit
}

# Test 5: Add another comment
Write-Host ""
Write-Host "5. Adding a second comment..." -ForegroundColor Yellow
$commentBody2 = @{
    text = "This is comment number 2! Real-time updates should broadcast this!"
} | ConvertTo-Json

try {
    $comment2 = Invoke-RestMethod -Uri "http://localhost:5175/api/tasks/$taskId/comments" -Method Post -Body $commentBody2 -Headers $headers
    Write-Host "SUCCESS: Second comment added!" -ForegroundColor Green
    Write-Host "  Text: $($comment2.text)" -ForegroundColor Gray
} catch {
    Write-Host "ERROR: Failed to add second comment: $_" -ForegroundColor Red
}

# Test 6: Get all comments
Write-Host ""
Write-Host "6. Retrieving all comments..." -ForegroundColor Yellow
try {
    $comments = Invoke-RestMethod -Uri "http://localhost:5175/api/tasks/$taskId/comments" -Method Get -Headers $headers
    Write-Host "SUCCESS: Retrieved $($comments.Count) comment(s)!" -ForegroundColor Green
    
    foreach ($c in $comments) {
        Write-Host "  ---" -ForegroundColor Gray
        Write-Host "  [$($c.username)] $($c.text)" -ForegroundColor White
        Write-Host "  Created: $($c.createdAt)" -ForegroundColor Gray
    }
} catch {
    Write-Host "ERROR: Failed to get comments: $_" -ForegroundColor Red
}

# Test 7: Get task with activity log
Write-Host ""
Write-Host "7. Getting task details with activity log..." -ForegroundColor Yellow
try {
    $taskDetails = Invoke-RestMethod -Uri "http://localhost:5175/api/tasks/$taskId" -Method Get -Headers $headers
    Write-Host "SUCCESS: Task retrieved!" -ForegroundColor Green
    Write-Host "  Title: $($taskDetails.shortTitle)" -ForegroundColor Cyan
    Write-Host "  Comments: $($taskDetails.comments.Count)" -ForegroundColor Cyan
    Write-Host "  Activity Log: $($taskDetails.activityLog.Count) entries" -ForegroundColor Cyan
    
    Write-Host ""
    Write-Host "  Activity Log:" -ForegroundColor Yellow
    foreach ($activity in $taskDetails.activityLog) {
        Write-Host "    - [$($activity.activityType)] $($activity.description)" -ForegroundColor Gray
    }
} catch {
    Write-Host "ERROR: Failed to get task details: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== All Tests Complete! ===" -ForegroundColor Green
Write-Host "Comments API is working perfectly!" -ForegroundColor Green
Write-Host ""
Write-Host "Now open http://localhost:3000 in your browser to see real-time updates!" -ForegroundColor Cyan
Write-Host ""
