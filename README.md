# Rhino Agent
10/19/2020 - 5 minutes to read  

## Important Announcement
Rhino Widget and other UI related to Rhino are currently under a major refactoring, in order to make Rhino services more accessible and user friendly. Among the upcoming changes:  

* Widget UI - Completely revised and will allow to import testing, running and creating multiple tests, save/load from files and much more.
* Back Office UI - For managing Rhino state, including soft plugins, configurations, models, import & export states, etc.
* Plugin Manager - Will be part of the back office and will allow to upload and manage hard plugins such as actions, reports, connectors and macros.
* Significant CLI enhancements - Will allow event more easy and seamless CI/CD integration.  

We will update on the progress of the changes which be viewed under the different branches.


> Rhino is a pure code-less platform. It does not generate any scripts or building code at the background. It is purely based on the input text.
>
> Rhino is fully compliant with [WebDriver]("https://www.w3.org/TR/webdriver/") protocol, utilizing Selenium, Appium and custom clients to interact with the different Web Drivers.  

Rhino is a new/old concept which allows to automate manual tests. Rhino utilize a special freestyle language of manual tests and uses a sophisticated engine to understand and automate these tests.  

Much like other well known spec based languages as [Gherkin](https://cucumber.io/docs/gherkin/reference/), each test case is actually a spec which Rhino engine can understand. Unlike other spec based languages, _**Rhino does not requires you to implement**_ the back-end actions of each spec line as you would have otherwise.  

Rhino API Standalone agent. You can connect your application or Application Lifecycle Manager (i.e. Jira, Azure DevOps, etc.) to Rhino and execute Rhino Specs.

## Resources
* [Rhino Documentations](./docs/pages/Home.md)

## Available Connectors
* [Rhino Text Connector](https://github.com/savanna-projects/rhino-connectors-text)
* [Rhino Gurock (TestRail) Connector](https://github.com/savanna-projects/rhino-connectors-gurock)
* [Rhino Atlassian (Jira + XRay) Connector](https://github.com/savanna-projects/rhino-connectors-atlassian)
* [Rhino Azure (TFS/Azure DevOps) Connector](https://github.com/savanna-projects/rhino-connectors-azure)