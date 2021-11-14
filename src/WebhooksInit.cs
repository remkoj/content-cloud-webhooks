using DeaneBarker.Optimizely.Webhooks.HttpProcessors;
using DeaneBarker.Optimizely.Webhooks.Routers;
using DeaneBarker.Optimizely.Webhooks.Serializers;
using DeaneBarker.Optimizely.Webhooks.Stores;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class WebhooksInit : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<IWebhookHttpProcessor, WebhookHttpProcessor>();
            //context.Services.AddSingleton<IWebhookHttpProcessor, UnstableWebhookHttpProcessor>();
            context.Services.AddSingleton<IWebhookStore, FileSystemWebhookStore>();
            context.Services.AddSingleton<IWebhookManager, WebhookManager>();
            context.Services.AddSingleton<IWebhookSerializer, WebhookSerializer>();
            context.Services.AddSingleton<IWebhookRouter, WebhookRouter>();
        }

        public void Initialize(InitializationEngine context)
        {
            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            var webhookManager = ServiceLocator.Current.GetInstance<IWebhookManager>();

            contentEvents.PublishedContent += webhookManager.QueuePublishedWebhook;
            contentEvents.MovedContent += webhookManager.QueueMovedWebhook;
            contentEvents.DeletedContent += webhookManager.QueueDeletedWebhook;
        }

        public void Uninitialize(InitializationEngine context)
        {
            var webhookManager = ServiceLocator.Current.GetInstance<IWebhookManager>(); // This is registered as a Singleton, so this should get the same instance where the thread were defined
            webhookManager.Dispose();
        }
    }
}