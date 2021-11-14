# Optimizely Content Cloud Webhooks

This is an add-on for Optimizely Content Cloud that allows for webhook functionality -- content operations can generate HTTP requests to external resources.

Some features:

* Operates in a separate thread. It will not block the UI, and any failures will not affect the Content Cloud installation
* Can queue infinite webhooks in a thread-safe worker environment. A separate thread works webhooks in the queue, and multiple threads can be started to work the queue.
* Allows for a specific number of retries after a specified retry delay ("if this webhook call fails, retry five more times, once every 15 seconds")
* Allows for throttling per thread ("each thread should only make one webhook call per second")
* Saves a history of each webhook execution, including multiple attempts in the event of failure

It is designed to be extended:

* The core logic a series of injected services which can be replaced as needed
* Replaceable services handle low-level operations such as:
   * Determination of the webhook URL target
   * Serialization of the webhook into a web request
   * Persistence of the webhook history to a data store
* The default implementations of all services are designed to be inherited -- nothing is `final` or `sealed` and very little is `private`. Methods have been kept small, with liberal helper functions -- overriding the main method in a derived class doesn't hide all the helper code.