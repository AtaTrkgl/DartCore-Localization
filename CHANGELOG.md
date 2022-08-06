# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.1] - 2022-08-06

### Fixed

-   Fixed an error in dependencies.

## [1.2.0] - 2021-05-03

### Fixed

-   Fixed the problem where newly added languages weren't getting added to the lng_files.txt file, making them unreadable.

### Added

-   Key Browser now highlights the key currently being edited in the Key Editor.

## [1.1.2] - 2021-12-17

### Fixed

-   Fixed the python script for importing csv files caused by previous versions.

## [1.1.1] - 2021-07-31

### Fixed

-   Fixed renaming issues caused by the previous version.

## [1.1.0] - 2021-07-30

### Changed

-   Key & Value files now have an empty line at the line to work better with version controls. Note that you'll need to add an empty line if you are upgrading from an older version.
-   Added size constraints to the Key Browser.

### Fixed

-   Key Browser not displaying the last key.
-   Fixed a bug where changing the value of a key switched the key browsers sorting mode but the display stayed the same.
