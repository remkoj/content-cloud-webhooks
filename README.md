# Optimizely Content Cloud Webhooks

This is an add-on for Optimizely Content Cloud that allows for webhook functionality -- content operations can generate HTTP requests to external resources.

Some features (of the default implementation):

* Posts a JSON-serialized version of the content involved in the operation
* Generates a webhook for four content actions: (1) published, (2) moved, (3) trashed (moved to the wastebasket), and (4) deleted
* Generic webhooks without content attached can be generated manually
* Operates in a separate thread. It will not block the UI, and any failures will not affect the Content Cloud installation.
* Placing a webhook in queue (the only thing done in the main thread) takes single-digit milliseconds (depending on the factories registered and executed)
* Will queue infinite webhooks in a thread-safe worker environment. A separate thread works webhooks in the queue, and multiple threads can be started to work the queue.
* Allows for a specific number of retries after a specified retry delay ("if this webhook call fails, retry five more times, once every 15 seconds")
* Allows for throttling per thread ("each thread should only make one webhook call per second")
* Saves a history of each webhook execution, including multiple attempts in the event of failure
* Low ceremony installation -- it's just raw source code, with no external dependencies outside of what's already in your Content Cloud project

It is designed to be extended:

* The core logic a series of injected services which can be replaced as needed
* Replaceable services handle low-level operations such as:
   * Determination of whether the webhook should execute or cancel
   * Determination of the webhook URL target
   * Serialization of the content and manipulation of the body, URL, and headers of the web request
   * HTTP request and response of the webhook
   * Persistence of the webhook history to a data store
* The default implementations of all services are designed to be inherited -- nothing is `final` or `sealed` and very little is `private`. Methods have been kept small, with liberal `protected` helper functions -- overriding the main method in a derived class doesn't hide all the helper code.

## The Components

In the case of interfaces *which are injected*, the default is in parentheses. This injection occurs (and can be changed) in `WebhooksInit`.

These are listed roughly in the order of invocation.

### IWebhookManager (WebhookManager)

This exposes the event handlers that Content Cloud calls when events are raised, and calls the factories to generate the webhooks it will place in queue.

By default, event handlers are attached to:

* `ContentPublished`
* `ContentMoved`
* `ContentDeleted`

Note that `ContentMoved` covers "soft deletes" as well, since those are technically just moves to the wastebasket.

The event handlers will create an `Webhook` object and place the webhook in the `IWebhookQueue`.

### IWebhookQueue (InMemoryWebhookQueue)

This is the pool of pending webhooks. It is responsible for holding the webhooks and launching and managing a process to work through them.

The default `InMemoryWebhookQueue` creates a blocking collection that holds webhooks, and launches a background thread to work them serially.

### Webhook and WebhookAttempt

This represents a single webhook generated from an event. A webhook contains the following information:

* The target URL it will call
* (optionally) The content object that generated the webhook
* A string representing the action that generated it ("Published", "Deleted", etc.)
* An `IWebhookSerializer` which generates the `HttpWebRequest` when the webhook is executed

Webhooks are placed in the `IWebhookQueue`. That object is responsible for working the queue and executing the webhooks.

A `Webhook` will generate a HTTP request when it is executed, and the results of that will be placed in its `History` property. If the HTTP request fails (returns any status code other than 200), the webhook might be retried several times. Each time will generate another `WebhookAttempt` record.

Once a `Webhook` has succeeded (the last `WebhookAttempt` in its `History` was successful), it will never execute again. Future content operations on the same content object will create a new `Webhook` object.

### WebhookFactoryManager

Iterates the registered `IWebhookFactory` objects and calls `Generate` on each, aggregating the returned webhooks.

### IWebhookFactory

(There is no default for this. You need to register your selected factories in your startup code.)

The interface contains one method: `Generate` which returns a `List<Webhook>` or `null`.

The default implementation requires you to pass in a target `Uri` and allows you to set the following:

* *IncludeTypes:* A list of content types that _should_ generate a webhook.
* *ExcludeTypes:* A list of content types that _should not_ generate a webhook.
* *IncludeActions:* A list of action strings that _should_ generate a webhook.
* *ExcludeActions:* A list of action strings that _should not_ generate a webhook.

*The exclusions are primary* -- if a type of action string is excluded, it will negate the webhook even if that type or action is included later. Inclusions are optional -- if they are not set, it's assumed that *everything* should generate a webhook.

The system works purely at the interface level. If you want custom logic, it's easy to reimplement in your own class and register that in the settings.

```csharp
public class MyWebhookFactoryProfile : IWebhookFactoryProfile
{
    public IEnumerable<Webhook> Process(string action, IContent content)
    {
        // Allow webhooks in the bottom half of each minute because...reasons
        if(DateTime.Now.Seconds <= 30) return null
        return new[] { new Webhook("http://webhooks.com", "something happened", new PostContentWebhookSerializer(), content) };
    }
}

var settings = ServiceLocator.Current.GetInstance<WebhookSettings>();
settings.RegisterWebhookFactory(new MyWebhookFactoryProfile());
```

You can add as many factories as you like. They will be evaluated serially, and all webhooks returned in aggregate will be placed in queue.

### IWebhookSerializer

(There is no default for this. Each `IWebhookFactory` assigns its own serializer.)

This turns a webhook into an `HttpWebRequest`.

The default implementation creates a POST request to the webhook target (which was populated by `IWebhookRouter`) with a JSON-serialized version of the content as the body of the request, and the `Action` as a querystring argument.

A helper class is provided to make it easier to create requests.

### IWebhookHttpProcessor (WebhookHttpProcessor)

This simply executes the `HttpWebRequest` created by `IWebhookSerializer`.

It's injected mainly so you can mock the HTTP request for testing.

The default implementation simply executes and returns a `WebhookAttempt` to be placed into history. Another implementation is provided which mocks a unstable receiver -- it fails a specified number of times before succeeding.

### IWebhookStore (FileSystemWebhookStore)

This persists the webhook. It's called when the webhook is placed in queue, and after every attempt to execute it (each attempt will append a `WebhookAttempt` record to it).

Two default implementations are provided: one writes JSON to the file system, the other just holds the webhooks in memory.

(When a UI is created, more methods will be created for this which will allow listing and searching the webhooks.)

## Basic Flow

Here is the basic flow. A lot of this is dependent on the default implementation of services. If you inject your own implementations, things could be different.

1. Your app starts up, and `WebhooksInit`:
   * Injects all the services as singletons
   * Binds the event handlers
   * Starts a worker thread on `InMemoryWebhookQueue`
1. When a content operation occurs in Content Cloud and an event is raised, the bound event handler on `IWebhookManager`:
   * Iterates all the `WebhookSettings.Factories`, calling `Generate` on each
   * Adds each produced webhook to the `IWebhookQueue`
4. When added to `IWebhookQueue`, that object:
   * Passes it to `IWebhookStore` to persist it
   * The `Webhook` object is found in the queue by the worker thread
   * The worker thread calls the `IWebhookSerializer` on the webhook and gets an `HttpWebRequest` back
   * The worker thread passes the `HttpWebRequest` to `IWebhookHttpProcessor` and gets back a `WebhookAttempt`
   * The worker thread attaches the `WebhookAttempt` to the history of the webhook
   * The worker thread passes the `Webhook` to `IWebhookStore` to persist it
   * If the webhook execution succeeded, we're all done 
   * If the webhook execution failed, the worker thread *might* set a timer for the default retry delay, then place the `Webhook` back in the queue (this depends on the settings)
   * The worker thread waits the specified throttle time delay, then blocks while waiting for a new object in the queue

## To Install and Configure

Compile the code into your project. This is not a complete VS project -- there is no project or solution file. The code is simply the class files, with no external dependencies or required Nuget packages.

in your startup code, add a single instance of a factory to the `WebhookSettings.Factories`:

```csharp
var settings = ServiceLocator.Current.GetInstance<WebhookSettings>();
settings.RegisterWebhookFactory(new PostContentWebhookFactory("http://webhook.com"));
```

That is enough to have the system start generating and processing webhooks. The `PostContentWebhookFactory` will serialize the content from any tracked event into JSON and POST it to the provided URL.

On `InMemoryWebhookQueue`, you can set the following static properties:

* `MaxAttempts` (default: 5): The maximum number of times a webhook should execute. If it fails on every attempt, it will abandon
* `DelayBetweenRetries` (default: 10 seconds): The number of milliseconds the worker should wait before putting a failed webhook back in queue
* `Throttle` (default: 1 second): The number of milliseconds each worker thread should wait before retrieving a new webhook from the queue

By default, `InMemoryWebhookQueue` will create one worker thread. If you desire more, you can call `InMemoryWebhookQueue.StartWatcher(int count)` and start as many as you like. The queue is thread-safe, but this will increase load on your endpoint.

By default, webhooks are persisted to memory. If you want to persist them to the file system, change the `IWebhookStore` service injection to use `FileSystemWebhookStore` and set the `FileSystemWebhookStore.StorePath` static property.

## To Inject Your Own Services

If you re-implement any services, they must be injected _after_ `WebhooksInit` has run, or they will be over-written. To do this, put a `ModuleDependency` on your initialization code:

```csharp
[InitializableModule]
[ModuleDependency(typeof(WebhooksInit))]
public class MyWebhooksInit : IConfigurableModule
{
    public void ConfigureContainer(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<IWebhookRouter, MyWebhookRouter>();
    }

    public void Initialize(InitializationEngine context) { }

    public void Uninitialize(InitializationEngine context) { }
}
```

This will wait until `WebhooksInit` has executed, then overwrite *those* services with your own implementations.

## Webhook Log Configuration

To create a separate log for webhooks, edit `EpiserverLog.config`

Add a new appender (this is a file appender, but the general concept applies for other logging methods):

```xml
<appender name="webhooksAppender" type="log4net.Appender.RollingFileAppender" >
  <file value="App_Data\webhooks.log" />  <!-- Adjust the path as you like -->
  <encoding value="utf-8" />
  <staticLogFileName value="true"/>
  <datePattern value=".yyyyMMdd.'log'" />
  <rollingStyle value="Date" />
  <threshold value="debug" />
  <appendToFile value="true" />
  <layout type="log4net.Layout.PatternLayout">
    <conversionPattern value="%date [%thread] %level %logger: %message%n" />
  </layout>
</appender>
```

Then, add a new logger:

```xml
<logger name="DeaneBarker.Optimizely.Webhooks" >
  <level value="DEBUG" />
  <appender-ref ref="webhooksAppender" />
</logger>
```

This will capture all logging activity for any class in the `DeaneBarker.Optimizely.Webhooks` namespace.