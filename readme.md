# VSTS CLI

The VSTS CLI is a tool for working with VSTS from a terminal environment. Its intent is to help with the more used functions that a developer would be using on a day to day basis. All commands start with `vsts` then followed by the primary `argument`.

## Authentication

VSTS CLI uses a personal access token that you can create in your [VSTS profile](https://www.visualstudio.com/en-us/docs/setup-admin/team-services/use-personal-access-tokens-to-authenticate). This token, along with some other profile information is stored in a `.config` directory in your profile root.

Since VSTS is a project first based service versus a repo first service like GitHub. This means that a single project can have multiple repostiories so the VSTS Cli configuration file has some hierarchy to it. The primary concept is that it will have a high level project with a repository collection where a repository is registred with a directory on your file system. All of which are per VSTS account such as:

- Configuration
  - Account Alpha
    - Repo-1
    - Repo-2
  - Account Beta
    - Repo-1
    - Repo-2
  - Account Charlie
    - Repo-1
    - Repo-2

The token will be promped when you run the tool for the first time and attempt to link a remoted VSTS git repo with the current directory.

## Current Supported Commands

- browse
- builds
- code
- pull requests
- work items

### vsts -h
```
Usage: vsts [options] [command]

Options:
  -? | -h | --help  Show help information

Commands:
  browse        Launches the default browser to the VSTS project root
  builds        commands for working with VSTS build definitions
  code          launches the default browser to the current repos code dashboard
  pullrequests  commands for working with VSTS pull requests
  workitems     commands for working with VSTS work items

Use "vsts [command] --help" for more information about a command.
```
### vsts browse -h
```
Usage: vsts browse [arguments] [options]

Arguments:
  dashboard  General dashboard to view. Opionts are builds, releases, workitems, pullrequests or testmanagement

Options:
  -? | -h | --help  Show help information
```

### vsts builds -h
```
Usage: vsts builds [options] [command]

Options:
  -? | -h | --help  Show help information

Commands:
  log  view latest build log for build definition

Use "builds [command] --help" for more information about a command.
```
### vsts builds log -h
```
Usage: vsts builds log [options]

Options:
  --id              build definition
  --detail          build definition
  -? | -h | --help  Show help information
```
### vsts code -h
```
Usage: vsts code [options]

Options:
  -? | -h | --help  Show help information
```
### vsts pullrequests -h
```
Usage: vsts pullrequests [arguments] [options]

Arguments:
  pull request identifier  pull request id to browse to

Options:
  -? | -h | --help  Show help information
```
### vsts workitems -h
```
Usage: vsts workitems [arguments] [options] [command]

Arguments:
  work item identifier  work item id or type, such as epic, user story, task or bug

Options:
  --states          filter by states such as new, active, resolved, closed or removed
  --tags            filter by any tag that assigned to work items
  --description     include description
  --browse          browse specific work item in VSTS
  -? | -h | --help  Show help information

Commands:
  add  command for adding new work items to the current project

Use "workitems [command] --help" for more information about a command.
```
### vsts workitems add -h
```
Usage: vsts workitems add [options]

Options:
  --workitemtype    work item type [required]
  --title           work item title [required] 
  --description     work item description
  --priority        work item priority
  --tag             work item tags
  -? | -h | --help  Show help information
```
