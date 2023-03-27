using DeaneBarker.Optimizely.Webhooks.Serializers;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell;
using EPiServer.Validation;
using EPiServer.ServiceLocation;

namespace DeaneBarker.Optimizely.Webhooks.Blocks.Editing
{
    public class SerializerSelectionFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && t.IsAssignableTo(typeof(IWebhookSerializer))))
                .Select(t => new SelectItem()
                {
                    Text = t.Name.TrimEnd("Serializer").PascalCaseToSpaced(),
                    Value = t.FullName
                });
        }

        public static IWebhookSerializer ResolveValue(object selectedValue)
        {
            var selectedTypeName = selectedValue.ToString();
            if (string.IsNullOrEmpty(selectedTypeName))
                return null;

            var serializerType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType(selectedTypeName))
                .WhereNotNull()
                .FirstOrDefault();

            if (serializerType is not null && serializerType.IsAssignableTo(typeof(IWebhookSerializer)))
                return (IWebhookSerializer)Activator.CreateInstance(serializerType);

            return null;
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
        protected static IContentTypeRepository ContentTypeRepository => ServiceLocator.Current.GetInstance<IContentTypeRepository>();

        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata) => 
            ContentTypeRepository
                .List()
                .Where(t => !t.Name.ToLower().StartsWith("sys") && t.Name != "WebhookFactoryBlock")
                .Select(t => new SelectItem() { Text = GetDisplayName(t), Value = t.GUID.ToString() })
                .OrderBy(i => i.Text);

        protected static string GetDisplayName(ContentType t)
        {
            var suffix = t.Base.ToString();
            var name = t.DisplayName ?? t.Name;
            name = name.TrimEnd(suffix);
            return $"{name} ({suffix})";
        }

        public static ContentType ResolveValue(object selectedValue)
        {
            if (Guid.TryParse(selectedValue.ToString(), out var contentTypeGuid))
                return ContentTypeRepository.Load(contentTypeGuid);
            return null;
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

    public class WebhookFactoryBlockValidator : IValidate<WebhookFactoryBlock>
    {
        public IEnumerable<ValidationError> Validate(WebhookFactoryBlock instance)
        {
            if (instance.WebhookType != null)
            {
                try
                {
                    return instance.GetSerializer().ValidateConfig(instance.SerializationConfig);
                }
                catch (NotImplementedException)
                {
                    // If it wasn't implemented, then no errors
                    return new List<ValidationError>();
                }
            }

            return new List<ValidationError>();
        }
    }
}
