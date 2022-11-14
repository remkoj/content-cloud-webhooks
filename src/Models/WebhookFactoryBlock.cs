using Castle.MicroKernel.SubSystems.Conversion;
using DeaneBarker.Optimizely.Webhooks.Factories;
using DeaneBarker.Optimizely.Webhooks.Serializers;
using EPiServer.ServiceLocation;
using EPiServer.Shell;
using EPiServer.Shell.Configuration;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DeaneBarker.Optimizely.Webhooks.Blocks
{
    [ContentType(GUID = "AEECADF2-3E89-4117-ADEB-F8D43565D3F4", DisplayName = "Webhook Factory", GroupName = "Advanced")]
    public class WebhookFactoryBlock : BlockData, IWebhookFactory
    {
        public static int WebhookFactoryBlockFolderId { get; set; } = 0;
        public static Dictionary<string, Type> AvailableSerializers { get; set; } = new();

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

        public string Name => $"{((IContent)this).Name} / {((IContent)this).ContentLink}";

        public IEnumerable<Webhook> Generate(string action, IContent content)
        {
            var target = Target.Replace("{action}", action).Replace("{id}", ((IContent)this).ContentLink.ID.ToString());
            var serializerType = GetTypeFromString(WebhookType);

            var factory = new SimpleWebhookFactoryProfile(target)
            {
                IncludeActions = Actions,
                IncludeTypes = Types.Select(t => Type.GetType(t)).ToList(),
                Serializer = (IWebhookSerializer)Activator.CreateInstance(serializerType)
            };

            return factory.Process(action, content);
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

        private Type GetTypeFromString(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var foundType = assembly.GetType(typeName);

                if (foundType != null)
                    return foundType;
            }

            return null;
        }
    }

    public class SerializerSelectionFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            return WebhookFactoryBlock.AvailableSerializers.Select(i => new SelectItem() { Text = i.Key, Value = i.Value.FullName });
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
            options.AddRange(types
                .Where(t => !t.Name.ToLower().StartsWith("sys"))
                .Where(t => t.Name != "WebhookFactoryBlock")
                .Select(t => new SelectItem() {

                    Text = getDisplayName(t),
                    Value = t.ModelTypeString

                })
                .OrderBy(i => i.Text));

            return options;
           
        }
        public string getDisplayName(ContentType t)
        {
            var suffix = t.Base.ToString();
            var name = t.DisplayName ?? t.Name;

            name = RemoveFromEnd(name, t.Base.ToString());

            return $"{name} ({suffix})";
        }

        private string RemoveFromEnd(string input, string remove)
        {
            if (input.EndsWith(remove) && input != remove)
            {
                input = input.Substring(0, input.Length - remove.Length);
            }

            return input;
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


    [UIDescriptorRegistration]
    public class WebhookFactoryBlockUIDescriptor : UIDescriptor<WebhookFactoryBlock>
    {
        public WebhookFactoryBlockUIDescriptor() : base()
        {
            DefaultView = CmsViewNames.AllPropertiesView;
            EnableStickyView = false;
            DisabledViews = new[] { CmsViewNames.PreviewView, CmsViewNames.OnPageEditView };
        }
    }
}
