using DeaneBarker.Optimizely.Webhooks.Factories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;
using System.ComponentModel.DataAnnotations;

namespace DeaneBarker.Optimizely.Webhooks.Blocks
{
    [ContentType(GUID = "AEECADF2-3E89-4117-ADEB-F8D43565D3F4")]
    public class WebhookFactoryBlock : BlockData, IWebhookFactory
    {
        public static int WebhookFactoryBlockFolderId { get; set; } = 0;

        [Required]
        [UIHint("UrlBox")]
        public virtual string Target { get; set; }

        [SelectMany(SelectionFactoryType = typeof(ActionSelectionFactory))]
        public virtual IList<string> Actions { get; set; }

        [SelectMany(SelectionFactoryType = typeof(TypeSelectionFactory))]
        public virtual IList<string> Types { get; set; }

        [SelectOne(SelectionFactoryType = typeof(MethodSelectionFactory))]
        public virtual string Method { get; set; }

        public string Name => $"{((IContent)this).Name} / {((IContent)this).ContentLink}";

        public IEnumerable<Webhook> Generate(string action, IContent content)
        {
            IWebhookFactory baseFactory;
            var target = Target.Replace("{action}", action).Replace("{id}", ((IContent)this).ContentLink.ID.ToString());

            if (Method == "GET")
            {
                baseFactory = new SimplePingWebhookFactory(target);
                ((SimplePingWebhookFactory)baseFactory).IncludeActions = Actions;
                ((SimplePingWebhookFactory)baseFactory).IncludeTypes = Types.Select(t => Type.GetType(t)).ToList();
            }
            else
            {
                baseFactory = new PostContentWebhookFactory(target);
                ((PostContentWebhookFactory)baseFactory).IncludeActions = Actions;
                ((PostContentWebhookFactory)baseFactory).IncludeTypes = Types.Select(t => Type.GetType(t)).ToList();
            }

            return baseFactory.Generate(action, content);
        }

        public static void RegisterFactories()
        {
            var webhookSettings = ServiceLocator.Current.GetInstance<WebhookSettings>();

            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var webhooksFolder = GetWebhooksFolderRoot();
            var webhookFactories = contentLoader.GetChildren<BlockData>(webhooksFolder);
            foreach(var f in webhookFactories.Where(i => i is WebhookFactoryBlock).Cast<WebhookFactoryBlock>())
            {
                webhookSettings.RegisterWebhookFactory(f, ((IContent)f).ContentGuid.ToString());
            }

        }

        public static ContentReference GetWebhooksFolderRoot()
        {
            return new ContentReference(WebhookFactoryBlockFolderId);
        }
    }

    public class MethodSelectionFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            return new[] {
                new SelectItem() { Text = "GET", Value = "GET"  },
                new SelectItem() { Text = "POST (with content)", Value = "POST"  }
            };
        }
    }

    public class ActionSelectionFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            return new[] {
                new SelectItem() { Text = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(Actions.Published), Value = Actions.Published  },
                new SelectItem() { Text = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(Actions.Moved), Value = Actions.Moved},
                new SelectItem() { Text = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(Actions.Trashed), Value = Actions.Trashed},
                new SelectItem() { Text = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(Actions.Deleted), Value = Actions.Deleted  }
            };
        }
    }

    public class TypeSelectionFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            var contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
            var types = contentTypeRepository.List();
            
            var options = new List<ISelectItem>();
            options.AddRange(types.ToList().OrderBy(t => t.Name).Select(t => new SelectItem() { Text = t.DisplayName ?? t.Name, Value = t.ModelTypeString }));

            return options;    
        }
    }

    [EditorDescriptorRegistration(TargetType = typeof(IList<String>), EditorDescriptorBehavior = EditorDescriptorBehavior.ExtendBase)]
    public class StringListEditorDescriptor : EditorDescriptor
    {
        public override void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
        {
            //Episervers check box editor handles valus as csv, aka comma separated sting, as default.
            //Need to make sure that the value is handled as an array.
            metadata.EditorConfiguration["valueIsCsv"] = false;
        }
    }

    [EditorDescriptorRegistration(TargetType = typeof(string), UIHint = "UrlBox", EditorDescriptorBehavior = EditorDescriptorBehavior.ExtendBase)]
    public class UrlBoxEditorDescriptor : EditorDescriptor
    {
        public override void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
        {
            base.ModifyMetadata(metadata, attributes);
            metadata.EditorConfiguration.Add("style", "width: 600px; font-size: 130%; font-family: consolas; padding: 5px;");
        }
    }

    // Hide the category selector...
    [EditorDescriptorRegistration(TargetType = typeof(CategoryList))]
    public class HideCategoryEditorDescriptor : EditorDescriptor
    {
        public override void ModifyMetadata(
           ExtendedMetadata metadata,
           IEnumerable<Attribute> attributes)
        {
            base.ModifyMetadata(metadata, attributes);
            if (metadata.PropertyName == "icategorizable_category")
            {
                metadata.GroupName = SystemTabNames.Settings;
            }
        }
    }

    public enum NumberOfColumns
    {
        One,
        Two,
        Three,
    }
}
