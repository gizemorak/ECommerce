var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server
var sqlPassword = builder.AddParameter("sql-password", secret: true);
var sqlServer = builder.AddSqlServer("mssql", password: sqlPassword, port: 1433)
    .WithDataVolume("mssql_data");

var ecommerceDb = sqlServer.AddDatabase("ecommercedb");

// Add RabbitMQ
var rabbitMqPassword = builder.AddParameter("rabbitmq-password", secret: true);
var rabbitMq = builder.AddRabbitMQ("rabbitmq", password: rabbitMqPassword, port: 5672)
    .WithManagementPlugin(port: 15672)
    .WithDataVolume("rabbitmq_data");

// Add Kafka
var kafka = builder.AddKafka("kafka", port: 9094)
    .WithKafkaUI()
    .WithDataVolume("kafka_data");

// Add Redis
var redis = builder.AddRedis("redis", port: 6379)
    .WithRedisCommander()
    .WithDataVolume("redis_data");

// Add Order API
var orderApi = builder.AddProject<Projects.OrderApi>("orderapi")
    .WithReference(ecommerceDb)
    .WithReference(rabbitMq)
    .WithReference(kafka)
    .WithReference(redis)
    .WaitFor(sqlServer)
    .WaitFor(rabbitMq)
    .WaitFor(kafka)
    .WaitFor(redis);

// Add Worker Service
var workerService = builder.AddProject<Projects.WorkerService>("workerservice")
    .WithReference(ecommerceDb)
    .WithReference(rabbitMq)
    .WithReference(kafka)
    .WithReference(redis)
    .WaitFor(sqlServer)
    .WaitFor(rabbitMq)
    .WaitFor(kafka)
    .WaitFor(redis);

builder.Build().Run();