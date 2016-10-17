# XrmUnitTest [![Build status](https://ci.appveyor.com/api/projects/status/e4x424jxt92vk00a?svg=true)](https://ci.appveyor.com/project/daryllabar/xrmunittest)

[![Join the chat at https://gitter.im/daryllabar/XrmUnitTest](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/daryllabar/XrmUnitTest?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)


Xrm Unit Testing Framework Provides a long list of features that makes developing Unit Tests in CRM quicker, faster, easier, and more reliable.

## What Is It?

XrmUnitTest contains two seperate, but complementary items:

- **DLaB.Xrm** - A library of XRM Extensions intended to be used by any XRM project, in any Xrm Plugin.  Ever had a QueryExpression, and wished you get generate SQL from the QueryExpression while debugging?  Now you can:  `queryExpression.GetSqlStatement()`.  Ever wish your plugins had a built in method to prevent recusive lookps?  Just inherit from the `GenericPluginHandlerBase`, they will.  Contains both [Xrm 2015](https://www.nuget.org/packages/DLaB.Xrm.2015/) and [Xrm 2016](https://www.nuget.org/packages/DLaB.Xrm.2016/) versions
- **XrmUnitTest** - A XrmUnitTesting framework that is designed to maximize the userfullness of your Unit Tests, while minimizing the amount of time to create or maintain them.  You can even utilize the in memory, fake CRM server to test your plugin, before ever deploying it to CRM.

Please check out the [Wiki](https://github.com/daryllabar/XrmUnitTest/wiki) for more information!

## How Can I Use It?

There are two methods for utilizing the code base, depending on how you'd like to consume the code base.  

1.  **Use NuGet** - This is by far the quickest and easiet method.  Search for and add the NuGet Pacakge "DLaB.Xrm(2015 or 2016)" to your Xrm Plugin\Service Assemblies.  Then search for and add the NuGet Package "XrmUnitTest(2015 or 2016)" to your Unit Test projects and base unit test project (See [Wiki](https://github.com/daryllabar/XrmUnitTest/wiki) on what is meant by base unit test project).
2.  **Use The Source Directly** - The only reason I list this method is for anyone that doesn't want to utilize ILMerge in order to create their CRM Plugin.  The classes are all implemented using the new Shared Project functionality in VS 2015, which allow you to reference the source code, without duplicating it.  This allows for your plugins to not require any IlMerge when running in a sandboxed (online) environment.
  
Again, more information is available on the [Wiki](https://github.com/daryllabar/XrmUnitTest/wiki)!


## How Can I Help?

XrmUnitTest is designed to be a community focused, open sourced project.  There are two main ways to help:
Please submit an issue for bugs/feature requests.  If you'd like to contir

1.  **Create an issue for a bug/feature request.** - If you find a bug or have an idea for a new feature or just need something that is missing, [please create an issue](https://github.com/daryllabar/XrmUnitTest/issues/new)!  There is no better way to make things better than to share.
2.  **Submit a Pull Request.** - I would highly recommend using the [XrmUnitTest Gitter Room](https://gitter.im/daryllabar/XrmUnitTest), before doing any major coding.  It's a great place to discuss what you'd like to do and get an offical blessing before starting any work!


For more information check out the [Wiki](https://github.com/daryllabar/XrmUnitTest/wiki)!
