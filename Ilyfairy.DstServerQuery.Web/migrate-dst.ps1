param(
    [string]$databaseType,
    [string]$migrationName
)

if (-not $databaseType) {
    Write-Host "请输入数据库类型"
    exit 1
}
if (-not $migrationName) {
    Write-Host "请输入迁移名称"
    exit 1
}

if ($databaseType -ieq "SqlServer") {
    dotnet ef migrations add $migrationName --context SqlServerDstDbContext --output-dir Migrations/SqlServer -- SqlType=SqlServer
}
elseif ($databaseType -ieq "MySql") {
    dotnet ef migrations add $migrationName --context MySqlDstDbContext --output-dir Migrations/MySql -- SqlType=MySql
}
elseif ($databaseType -ieq "Sqlite") {
    dotnet ef migrations add $migrationName --context SqliteDstDbContext --output-dir Migrations/Sqlite -- SqlType=Sqlite
}
elseif ($databaseType -ieq "PostgreSql") {
    dotnet ef migrations add $migrationName --context PostgreSqlDstDbContext --output-dir Migrations/PostgreSql -- SqlType=PostgreSql
}
else {
    Write-Host "数据库类型错误"
}
