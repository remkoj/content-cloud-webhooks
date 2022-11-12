using DeaneBarker.Optimizely.Webhooks.Blocks;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks.Blocks
{
    [InitializableModule]
    [ModuleDependency(typeof(WebhooksInit))]
    public class WebhookFactoryBlockInit : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            WebhookFactoryBlock.RegisterFactories();

            // If anything in the Webhooks folder is published, re-register all the blocks
            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();

            contentEvents.PublishedContent += (object s, ContentEventArgs e) =>
            {
                if (e.Content.ParentLink == WebhookFactoryBlock.GetWebhooksFolderRoot())
                {
                    WebhookFactoryBlock.RegisterFactories();
                }

            };

            contentEvents.MovedContent += (object s, ContentEventArgs e) =>
            {

                if (((MoveContentEventArgs)e).OriginalParent == WebhookFactoryBlock.GetWebhooksFolderRoot())
                {
                    WebhookFactoryBlock.RegisterFactories();
                }

            };
        }

        public void Uninitialize(InitializationEngine context)
        {
            throw new NotImplementedException();
        }
    }


}