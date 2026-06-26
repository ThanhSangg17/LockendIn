param (
    [string]$bookingId
)

if ([string]::IsNullOrEmpty($bookingId)) {
    Write-Error "Booking ID is required."
    exit 1
}

$connString = "Server=(localdb)\MSSQLLocalDB;Database=LockedInDb;Trusted_Connection=True;TrustServerCertificate=True;"
$connection = New-Object System.Data.SqlClient.SqlConnection($connString)
$connection.Open()

# Update booking status to PaidPendingAcceptance (2)
$command = $connection.CreateCommand()
$command.CommandText = "UPDATE bookings SET status = 2, paid_at = GETUTCDATE() WHERE id = '$bookingId'"
$rows = $command.ExecuteNonQuery()

if ($rows -gt 0) {
    # Insert a dummy successful payment record if not exists
    $payId = [Guid]::NewGuid()
    $orderCode = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds().ToString()
    $command.CommandText = "IF NOT EXISTS (SELECT 1 FROM payments WHERE booking_id = '$bookingId') INSERT INTO payments (id, booking_id, provider, order_code, amount, status, created_at, paid_at) VALUES ('$payId', '$bookingId', 'PayOS-Sim', '$orderCode', 1000, 2, GETUTCDATE(), GETUTCDATE())"
    $command.ExecuteNonQuery() | Out-Null
    
    Write-Output "SUCCESS: Booking $bookingId marked as PAID."
} else {
    Write-Error "Booking $bookingId not found or not updated."
}

$connection.Close()
