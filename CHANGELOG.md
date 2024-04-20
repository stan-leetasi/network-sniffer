# Changelog

All notable changes to this project will be documented in this file.

### Current state
The current version of ipk24chat-client supports communication through the tcp protocol. Communication with a server has been tested locally on the mock server and on the provided reference server during system testing and should work correctly.
#### Known limitations
The client is not able to process more than 1 message pre millisecond. If more than one message is received in such a short time, they will get recognised as 1 and not get parsed correctly, resulting in an "Unknown message type" error being thrown by the MsgHandler.

## [0.1.0] - 2024-04-20

### Added

- Initial solution and project files


## [0.2.0] - 2024-04-20

### Added

- Argument parser class


## [0.3.0] - 2024-04-21

### Added

- Printing of available devices

### Changed

- bug fixes in ArgParser
