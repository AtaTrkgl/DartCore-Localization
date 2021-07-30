# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2021-07-30

### Changed
- Key & Value files now have an empty line at the line to work better with version controls. Note that you'll need to add an empty line if you are upgrading from an older version.
- Added size constraints to the Key Browser.

### Fixed
- Key Browser not displaying the last key.
- Fixed a bug where changing the value of a key switched the key browsers sorting mode but the display stayed the same.