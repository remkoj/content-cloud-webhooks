using DeaneBarker.Optimizely.Webhooks.Factories;
using DeaneBarker.Optimizely.Webhooks.Serializers;
using DeaneBarker.Optimizely.Webhooks.Blocks.Editing;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ObjectEditing;
using System.ComponentModel.DataAnnotations;
using System.Data;
using EPiServer.Cms.Shell;

namespace DeaneBarker.Optimizely.Webhooks.Blocks
{
    [ContentType(
        GUID = "AEECADF2-3E89-4117-ADEB-F8D43565D3F4", 
        DisplayName = "Webhook Factory", 
        Description = "Add/manage a webhook",
        GroupName = "Advanced"
    )]
    public class WebhookFactoryBlock : BlockData, IWebhookFactory
    {
        [Required]
        [UIHint("UrlBox")]
        public virtual string Target { get; set; }

        [SelectMany(SelectionFactoryType = typeof(ActionSelectionFactory))]
        public virtual IList<string> Actions { get; set; }

        [SelectMany(SelectionFactoryType = typeof(TypeSelectionFactory))]
        public virtual IList<string> Types { get; set; }

        [Display(Name = "Webhook Type")]
        [SelectOne(SelectionFactoryType = typeof(SerializerSelectionFactory))]
        public virtual string WebhookType { get; set; }

        [Display(Name = "Serialization Configuration")]
        [ClientEditor(ClientEditingClass = "webhooks/Editor")]
        public virtual string SerializationConfig { get; set; }

        public string Name => $"{((IContent)this).Name} / {((IContent)this).ContentLink}";

        public string FactoryID => ((IContent)this).ContentGuid.ToString();

        public IEnumerable<Webhook> Generate(string action, IContent content)
        {
            var target = Target.Replace("{action}", action).Replace("{id}", ((IContent)this).ContentLink.ID.ToString());

            var factory = new SimpleWebhookFactoryProfile(target)
            {
                IncludeActions = Actions,
                IncludeTypes = Types.Select(t => TypeSelectionFactory.ResolveValue(t)).ToList(),
                Serializer = GetSerializer()
            };

            // Set the config, if we even have any
            // (I originally did this in the constructor, but then I had to require a constructor on every implementation...)
            factory.Serializer.SerializationConfig = SerializationConfig;

            return factory.Process(action, content);
        }

        public static IEnumerable<WebhookFactoryBlock> GetAllInstances()
        {
            var contentModelUsage = ServiceLocator.Current.GetInstance<IContentModelUsage>();
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();

            var webhookBlockType = contentTypeRepository.Load<WebhookFactoryBlock>();
            return contentModelUsage.ListContentOfContentType(webhookBlockType)
                .GroupBy(f => f.ContentLink.ToReferenceWithoutVersion())
                .Select(g => contentLoader.Get<WebhookFactoryBlock>(g.First().ContentLink.ToReferenceWithoutVersion()))
                .Where(b => 
                    ((IContent)b).ParentLink != ContentReference.WasteBasket && 
                    ((IContent)b).IsPublished()
                );
        }

        public IWebhookSerializer GetSerializer() => SerializerSelectionFactory.ResolveValue(WebhookType);
    }
}
