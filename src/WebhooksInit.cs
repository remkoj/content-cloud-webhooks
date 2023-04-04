using DeaneBarker.Optimizely.Webhooks.Factories;
using DeaneBarker.Optimizely.Webhooks.HttpProcessors;
using DeaneBarker.Optimizely.Webhooks.Queues;
using DeaneBarker.Optimizely.Webhooks.Stores;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Modules;

namespace DeaneBarker.Optimizely.Webhooks
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class WebhooksInit : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            // This manages the entire webhook process
            // All other services are injected into this one
            // This is where the event handlers are located
            context.Services.AddSingleton<IWebhookManager, WebhookManager>();

            // This makes the actual HTTP call
            // I broke this out to its own service so it could be mocked -- I needed a way to test if the call failed
            context.Services.AddSingleton<IWebhookHttpProcessor, WebhookHttpProcessor>();
            //context.Services.AddSingleton<IWebhookHttpProcessor, UnstableWebhookHttpProcessor>();

            // This persists the webhook and its history to some data source
            context.Services.AddSingleton<IWebhookStore, FileSystemWebhookStore>();

            // This holds the pending webhooks and manages the process that works them
            context.Services.AddSingleton<IWebhookQueue, InMemoryWebhookQueue>();

            // Holds various settings
            context.Services.AddSingleton<WebhookSettings, WebhookSettings>();

            // Executes all the IWebhookFactoryProfiles to produce webhooks
            context.Services.AddSingleton<WebhookFactoryManager, WebhookFactoryManager>();

            // Register the Shell Client side resources
            context.Services.Configure<ProtectedModuleOptions>(o => o.Items.Add(new ModuleDetails { Name = "DeaneBarker.Optimizely.Webhooks" }));
        }

        public void Initialize(InitializationEngine context)
        {
            var webhookManager = ServiceLocator.Current.GetInstance<IWebhookManager>();

            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            contentEvents.PublishedContent += webhookManager.QueuePublishedWebhook;
            contentEvents.MovedContent += webhookManager.QueueMovedWebhook;
            contentEvents.DeletedContent += webhookManager.QueueDeletedWebhook;
        }

        public void Uninitialize(InitializationEngine context)
        {
            var webhookQueue = ServiceLocator.Current.GetInstance<IWebhookQueue>(); // This is registered as a Singleton, so this should get the same instance where the thread were defined
            webhookQueue.Dispose();
        }
    }
}