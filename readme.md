# VSTS CLI

The VSTS CLI is a tool for working with VSTS from a terminal environment. Its intent is to help with the more used functions that a developer would be using on a day to day basis. All commands start with `vsts` then followed by the primary `argument`.

## Authentication

VSTS CLI uses a personal access token that you can create in your [VSTS profile](https://www.visualstudio.com/en-us/docs/setup-admin/team-services/use-personal-access-tokens-to-authenticate). This token, along with some other profile information is stored in a `.config` directory in your profile root.

Since VSTS is a project first based service versus a repo first service like GitHub. This means that a single project can have multiple repostiories so the VSTS Cli configuration file has some hierarchy to it. The primary concept is that it will have a high level project with a repository collection where a repository is registred with a directory on your file system.

The token will be promped when you run the tool for the first time and attempt to link a remoted VSTS git repo with the current directory.

## Current Supported Commands

- browse
- work items
- pull requests

### Browse

From a directory that is registered with a vsts project calling `vsts browse` will launch the default web browser to your project root page.

Argument options for `vsts browse` are

- `vsts browse builds`
- `vsts browse releases`
- `vsts browse work-items`
- `vsts browse pull-requests`

where these arguments will launch the default web browser to those sections landing pages attempting to 'show all' instead of the 'just mine' when possible.

`vsts browse -h` will show these options of the version you have installed.

### Work Items

The `vsts work-items` argument allows us to get detail of a single work item or a query of work item types in various states.

`vsts work-items you-work-item-number` will return the detail of a given work item.

`vsts work-items bugs` will return the projects bug work item types in a default state of new or assigned.

`vsts work-items bugs --states closed` will return the projects bug work item types in a default state of new or assigned.

`vsts work-items add --workitemtype userstory --title "Useful title" --description "Useful description" --priority 1 --tag abc --tag xyz` will create a user story work item in your project tagged with abc & xyz and a priority set to 1.

`vsts work-items -h` will show these options of the version you have installed.

### Pull Requests

The `vsts pull-requests` argument allows us to get detail of a single pull request or a query the active pull requests for the current repository.

`vsts pull-requests` will display a list of active pull requests.

`vsts pull-requests you-work-pull-requeset-number` will open the default browser to the   argument allows us to get detail of a single pull request or a query the active pull requests for the current repository.

`vsts pull-requests -h` will show these options of the version you have installed.
