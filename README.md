<a id="readme-top"></a>

<h3 align="center">gitMoH - MOduleHelper</h3>

  <p align="center">
    Small binary tool to switch modules in a repository between links, submodules and packages (in future).
    <br />
    (AKA Submodule Switcher)
  </p>
</div>

## About The Project

Software Engineers develops applications using modules (or packages).
Modules may be included as git-submodules or using a packet-manager into applications.

Software Engineers also develops modules.

Some Software Engineers do both.

In this case it might be useful to switch from git-submodules or packet-manager to a local linked directory with the source code of the module.
This especially makes sense if you have multiple applications that are using your modules.

gitmoh helps you with the task of switching modules.

gitmoh is released as binary (executable) image. No framework installation is needed.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Current Status

Early version. Does support git-submodule but no packet-manager, yet.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Usage

Download the binary and set your PATH to the binary folder.

gitmoh can be used in _AdHoc-Mode_ (without configuration) or in _Config-Mode_.
If no configuration is existing, _AdHoc-Mode_ is used by default.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Usage in AdHoc-Mode

Switch the module `eely-base-web` from git-submodule to a local folder.
Ensure that the local folder does already exists and contains the source code of the module.
```
eely> gitmoh toLink --module=eely-base-web --commonPath=../commons/eely-base-web
```

Switch the module `eely-base-web` back from local folder to git-submodule.
```
eely> gitmoh toSubmodule --module=eely-base-web
```

Switch all modules from git-submodule to local folders.
```
eely> gitmoh toLink --module=* --commonRoot=../commons
```

Switch all modules back from local folders to git-submodule.
```
eely> gitmoh toSubmodule --module=*
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Usage in Config-Mode

Initialize configuration
```
eely> gitmoh init
```
You may want to edit the configuration file.

Switch the module `eely-base-web` from git-submodule to a local folder.
Ensure that the local folder does already exists and contains the source code of the module.
```
eely> gitmoh toLink --module=eely-base-web
```

Switch the module `eely-base-web` back from local folder to git-submodule.
```
eely> gitmoh toSubmodule --module=eely-base-web
```

Switch all modules from git-submodule to local folders.
```
eely> gitmoh toLink --module=*
```

Switch all modules back from local folders to git-submodule.
```
eely> gitmoh toSubmodule --module=*
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Ideas / Roadmap

* Switch from/to Nuget (DotNet)
* Switch from/to Pubget (Flutter/Dart)
* Switch from/to NPM (Javascript)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## License

Distributed under GPLv3. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>