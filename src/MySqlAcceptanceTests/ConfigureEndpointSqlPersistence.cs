﻿using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Support;
using NServiceBus.Persistence.Sql.ScriptBuilder;

public class ConfigureEndpointSqlPersistence : IConfigureEndpointTestExecution
{
    ConfigureEndpointHelper endpointHelper;

    public Task Configure(string endpointName, EndpointConfiguration configuration, RunSettings settings, PublisherMetadata publisherMetadata)
    {
        if (configuration.IsSendOnly())
        {
            return Task.FromResult(0);
        }
        var tablePrefix = TableNameCleaner.Clean(endpointName).Substring(0, Math.Min(endpointName.Length, 35));
        endpointHelper = new ConfigureEndpointHelper(configuration, tablePrefix, MySqlConnectionBuilder.Build, BuildSqlDialect.MySql);
        var persistence = configuration.UsePersistence<SqlPersistence>();
        persistence.ConnectionBuilder(MySqlConnectionBuilder.Build);
        persistence.SqlDialect<SqlDialect.MySql>();
        persistence.TablePrefix($"{tablePrefix}_");
        var subscriptions = persistence.SubscriptionSettings();
        subscriptions.DisableCache();
        persistence.DisableInstaller();
        return Task.FromResult(0);
    }

    public Task Cleanup()
    {
        return endpointHelper?.Cleanup();
    }
}