# toofz Services Core

[![Build status](https://ci.appveyor.com/api/projects/status/ra5o1lcdc1hh3e29?svg=true)](https://ci.appveyor.com/project/leonard-thieu/toofz-necrodancer-leaderboards-services-common)
[![codecov](https://codecov.io/gh/leonard-thieu/toofz-services-core/branch/master/graph/badge.svg)](https://codecov.io/gh/leonard-thieu/toofz-services-core)
[![MyGet](https://img.shields.io/myget/toofz/v/toofz.Services.svg)](https://www.myget.org/feed/toofz/package/nuget/toofz.Services)

## Overview

**toofz Services Core** contains common code used by **toofz Services**.

---

**toofz Services Core** is a component of **toofz**. 
Information about other projects that support **toofz** can be found in the [meta-repository](https://github.com/leonard-thieu/toofz-necrodancer).

## Description

toofz Services Core provides the following features for toofz Services.

* Start as console application or service
* Command-line argument parsing
* Settings persistence
* Secrets encryption
* Execution cycle scheduling

## Installing via NuGet

Add a NuGet.Config to your solution directory with the following content:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="toofz" value="https://www.myget.org/F/toofz/api/v3/index.json" />
  </packageSources>
</configuration>
```

```powershell
Install-Package toofz.Services
```

### Dependents

* [toofz Leaderboards Service](https://github.com/leonard-thieu/leaderboards-service)
* [toofz Daily Leaderboards Service](https://github.com/leonard-thieu/daily-leaderboards-service)
* [toofz Players Service](https://github.com/leonard-thieu/players-service)
* [toofz Replays Service](https://github.com/leonard-thieu/replays-service)

### Dependencies

* [toofz Build](https://github.com/leonard-thieu/toofz-build)

## Requirements

* .NET Framework 4.6.1

## Contributing

Contributions are welcome for toofz Services Core.

* Want to report a bug or request a feature? [File a new issue](https://github.com/leonard-thieu/toofz-steam/issues).
* Join in design conversations.
* Fix an issue or add a new feature.
  * Aside from trivial issues, please raise a discussion before submitting a pull request.

### Development

#### Requirements

* Visual Studio 2017

#### Getting started

Open the solution file and build. Use Test Explorer to run tests.

## License

**toofz Services Core** is released under the [MIT License](LICENSE).
