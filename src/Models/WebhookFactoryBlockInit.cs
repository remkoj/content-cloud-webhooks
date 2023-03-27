using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace DeaneBarker.Optimizely.Webhooks.Blocks
{
    [InitializableModule]
    [ModuleDependency(typeof(WebhooksInit))]
    public class WebhookFactoryBlockInit : IInitializableModule
    {
        protected static IContentEvents ContentEvents => ServiceLocator.Current.GetInstance<IContentEvents>();
        protected static WebhookSettings WebhookSettings => ServiceLocator.Current.GetInstance<WebhookSettings>();

        public void Initialize(InitializationEngine context)
        {
            RegisterFactories();
            ContentEvents.DeletedContent += ContentEvents_DeletedContent;
            ContentEvents.PublishedContent += ContentEvents_PublishedContent;
            ContentEvents.MovedContent += ContentEvents_MovedContent;
        }

        public void Uninitialize(InitializationEngine context)
        {
            ContentEvents.DeletedContent -= ContentEvents_DeletedContent;
            ContentEvents.MovedContent -= ContentEvents_MovedContent;
            ContentEvents.PublishedContent -= ContentEvents_PublishedContent;
        }

        private void ContentEvents_MovedContent(object sender, ContentEventArgs e)
        {
            if (e.Content is WebhookFactoryBlock factoryBlock)
            {
                if (((IContent)factoryBlock).ParentLink == ContentReference.WasteBasket)
                    RemoveFactory(factoryBlock);
                else
                    RegisterFactory(factoryBlock);
            }
        }

        private void ContentEvents_PublishedContent(object sender, ContentEventArgs e)
        {
            if (e.Content is WebhookFactoryBlock factoryBlock)
                RegisterFactory(factoryBlock);
        }

        private void ContentEvents_DeletedContent(object sender, DeleteContentEventArgs e)
        {
            if (e.Content is WebhookFactoryBlock factoryBlock)
                RemoveFactory(factoryBlock);
        }


        public static void RegisterFactories() =>
            WebhookFactoryBlock.GetAllInstances().ForEach(RegisterFactory);

        public static void RegisterFactory(WebhookFactoryBlock instance) =>
            WebhookSettings.RegisterWebhookFactory(instance, instance.FactoryID);

        public static void RemoveFactory(WebhookFactoryBlock instance) =>
            WebhookSettings.RemoveWebhookFactory(instance.FactoryID);
    }
}