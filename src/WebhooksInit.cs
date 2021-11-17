using DeaneBarker.Optimizely.Webhooks.HttpProcessors;
using DeaneBarker.Optimizely.Webhooks.Queues;
using DeaneBarker.Optimizely.Webhooks.Routers;
using DeaneBarker.Optimizely.Webhooks.Serializers;
using DeaneBarker.Optimizely.Webhooks.Stores;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

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

            // This determines the URL to send the request to
            // This is where you set all the logic to "route" your webhook; if you return NULL from "Route," the webhook will abandon (it just won't be put in queue)
            // If you only have one URL, just set that on WebhookManager master and the default router will use it from there
            context.Services.AddSingleton<IWebhookRouter, WebhookRouter>();

            // This turns the webhook into an HttpWebRequest
            // This is where you would manipulate the URL or add custom headers or whatever
            context.Services.AddSingleton<IWebhookSerializer, WebhookSerializer>();

            // This makes the actual HTTP call
            // I broke this out to its own service so it could be mocked -- I needed a way to test if the call failed
            context.Services.AddSingleton<IWebhookHttpProcessor, WebhookHttpProcessor>();
            //context.Services.AddSingleton<IWebhookHttpProcessor, UnstableWebhookHttpProcessor>();

            // This persists the webhook and its history to some data source
            context.Services.AddSingleton<IWebhookStore, FileSystemWebhookStore>();

            // This holds the pending webhooks and manages the process that works them
            context.Services.AddSingleton<IWebhookQueue, InMemoryWebhookQueue>();
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