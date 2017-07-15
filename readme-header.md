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

