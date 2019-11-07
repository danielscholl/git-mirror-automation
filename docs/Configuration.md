# Configuration

Configuration of the service is done via a storage account.

Once deployed the Azure Function will execute on a schedule (default: every 5 minutes) and load configuration files from the storage account.

It will load all json files from the container `configuration`.

Simply place files like the [example](Examples.md) into the container and then check app insights to verify it is working correctly.