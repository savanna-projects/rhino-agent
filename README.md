# Rhino Agent
10/19/2020 - 5 minutes to read  

> Rhino is a pure code-less platform. It does not generate any scripts or building code at the background. It is purely based on the input text.
>
> Rhino is fully compliant with [WebDriver]("https://www.w3.org/TR/webdriver/") protocol, utilizing Selenium, Appium and custom clients to interact with the different Web Drivers.  

Rhino is a new/old concept which allows to automate manual tests. Rhino utilize a special freestyle language of manual tests and uses a sophisticated engine to understand and automate these tests.  

Much like other well known spec based languages as [Gherkin](https://cucumber.io/docs/gherkin/reference/), each test case is actually a spec which Rhino engine can understand. Unlike other spec based languages, _**Rhino does not requires you to implement**_ the back-end actions of each spec line as you would have otherwise.  

Rhino API Standalone agent. You can connect your application or Application Lifecycle Manager (i.e. Jira, Azure DevOps, etc.) to Rhino and execute Rhino Specs.

## Resources
* [Rhino Documentations](./docs/pages/Home.md)

## Available Connectors
* [Plain Text](https://github.com/savanna-projects/rhino-connectors-text)
* [Xray Test Management for Jira](https://github.com/savanna-projects/rhino-connectors-atlassian)
* [TestRail: Comprehensive Test Case Management](https://github.com/savanna-projects/rhino-connectors-gurock)